using System;
using System.Text.RegularExpressions;
using EditorConfig.VisualStudio.Helpers;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using System.Linq;

namespace EditorConfig.VisualStudio.Logic.Cleaning
{
    /// <remarks>
    /// The intent of initial cleanup is to perform cleanups relevant to the opening of a new document. Combined
    /// with the IDE settings that have already been set, this prevents the user from blindly diverging from the
    /// settings defined in the .editorconfig file; thus, the potential to drastically reduce the accumulation of
    /// myriad OnBeforeSave cleanups.
    ///
    /// The commands run by initial cleanup cause the cursor to jump to the top of the document. This presents no
    /// issue at all for the opening of a new document, but is completely unacceptable and disruptive for OnBeforeSave
    /// commands. As such, we take advantage of the opportunities initial cleanups give us, such as built-in Visual
    /// Studio commands that perform incredibly faster than line-by-line regular expression matches and replacements.
    /// </remarks>
    internal class InitialCleanup
    {
        private readonly TextDocument _textDoc;
        private readonly _DTE _ide;
        private readonly Results _settings;
        private readonly string _eol;

        /// <summary>
        /// Gets the version of the running IDE instance.
        /// </summary>
        public Version IDEVersion { get { return new Version(_ide.Version); } }

        /// <summary>
        /// Gets a flag indicating if POSIX regular expressions should be used for TextDocument Find/Replace actions.
        /// Applies to pre-Visual Studio 11 versions.
        /// </summary>
        public bool UsePOSIXRegEx
        {
            get { return IDEVersion.Major < 11; }
        }

        internal InitialCleanup(ITextDocument textDocument, _DTE ide, Results settings)
        {
            var doc = ide.Documents.OfType<Document>()
                .FirstOrDefault(x => x.FullName == textDocument.FilePath);
            if (doc != null)
                _textDoc = (TextDocument) doc.Object("TextDocument");
            _ide = ide;
            _settings = settings;
            _eol = settings.EndOfLine();
        }

        /// <summary>
        /// Performs initial cleanup methods on the active document.
        /// </summary>
        internal void Execute()
        {
            if (_textDoc == null) return;

            TrimTrailingWhitespace();
            FixLineEndings();

            if (!_settings.ContainsKey("indent_style")) return;
            var indentStyle = _settings["indent_style"];

            var indentSize = 0;
            if (indentStyle == "tab" || !_settings.IfHasKeyTrySetting("indent_size", i => indentSize = i))
            {
                FixIndentStyle(indentStyle);
                return;
            }

            var indentSizeTrend = AnalyzeIndentSizeTrend(indentSize);
            if (indentSize == indentSizeTrend)
            {
                FixIndentStyle(indentStyle);
                return;
            }

            FixIndentStyle(indentStyle);

            if (indentStyle == "space")
                FixIndentSize(indentSizeTrend, indentSize);
        }

        /// <summary>
        /// Modifies indentation with spaces to match that of the current settings.
        /// </summary>
        /// <param name="indentSizeTrend">The current document's indent size trend.</param>
        /// <param name="indentSizeSetting">The indent size EditorConfig setting.</param>
        private void FixIndentSize(int indentSizeTrend, int indentSizeSetting)
        {
            var pattern = string.Format(@"^( {{{0}}})+", indentSizeTrend);
            var leadingSpaces = new Regex(pattern, RegexOptions.Compiled);
            foreach (var cursor in _textDoc.FindMatches(pattern))
            {
                var line = cursor.GetLine();
                var numberOfSpaces = leadingSpaces.Match(line).Length;
                var replaceWith = new string(
                    ' ', (int) Math.Ceiling(numberOfSpaces*(indentSizeSetting/(float) indentSizeTrend)));
                cursor.ReplaceText(numberOfSpaces, replaceWith, (int)vsFindOptions.vsFindOptionsFromStart);
            }
        }

        /// <summary>
        /// Tabifies or untabifies the entire active document, according to the EditorConfig indent_style setting.
        /// </summary>
        private void FixIndentStyle(string indentStyle)
        {
            switch (indentStyle)
            {
                case "tab":
                    SelectAllAndExecuteCommand("Edit.TabifySelectedLines");
                    break;

                case "space":
                    SelectAllAndExecuteCommand("Edit.UntabifySelectedLines");
                    break;
            }
        }

        /// <summary>
        /// Trims all trailing whitespace in the active document.
        /// </summary>
        private void TrimTrailingWhitespace()
        {
            if (!_settings.TryKeyAsBool("trim_trailing_whitespace")) return;
            if (UsePOSIXRegEx)
                _textDoc.SubstituteAllStringMatches(@":b+{\n}", @"\1");
            else
                _textDoc.SubstituteAllStringMatches(@"[ \t]+(\r?\n)", "$1");
        }

        /// <summary>
        /// Replaces all EOL characters in the active document that are inconsistent with the EditorConfig
        /// end_of_line setting.
        /// </summary>
        private void FixLineEndings()
        {
            if (UsePOSIXRegEx)
            {
                // Defer line ending cleanup until OnBeforeSave, as POSIX expressions give us no way to
                // reliably change only the lines that are inconsistent.
                return;
            }
            switch (_eol)
            {
                case @"\n":
                    _textDoc.SubstituteAllStringMatches(@"\r", string.Empty);
                    break;
                case @"\r\n":
                    _textDoc.SubstituteAllStringMatches(@"(?<!\r)\n", _eol);
                    break;
            }
        }

        /// <summary>
        /// Runs the command on the entire active text document.
        /// </summary>
        /// <param name="commandName">The name of the command to invoke.</param>
        private void SelectAllAndExecuteCommand(string commandName)
        {
            ExecuteCommand("Edit.SelectAll");
            ExecuteCommand(commandName);
            ExecuteCommand("Edit.SelectionCancel");
        }

        /// <summary>
        /// Runs commands or macros listed in the Keyboard section of the Environment panel of Options dialog box on
        /// the Tools menu. This wrapper method first checks that the command is available and, if so, runs it.
        /// </summary>
        /// <param name="commandName">The name of the command to invoke.</param>
        /// <param name="commandArgs">A string containing the same arguments you would supply if you were invoking
        /// the command from the Command window. If a string is supplied, it is passed to the command line as the
        /// command's first argument and is parsed to form the various arguments for the command. This is similar
        /// to how commands are invoked in the Command window.</param>
        private void ExecuteCommand(string commandName, string commandArgs = "")
        {
            var commands = _ide.Commands;
            if (commands.Item(commandName).IsAvailable)
                _ide.ExecuteCommand(commandName, commandArgs);
        }

        /// <summary>
        /// Analyze the document for most common trend being used for indent size.
        /// </summary>
        /// <param name="defaultSetting">The default setting to use if no trend is identified.</param>
        /// <returns>The tab width with the highest score or largest tab width in the case of a tie.</returns>
        internal int AnalyzeIndentSizeTrend(int defaultSetting)
        {
            var indentWidthScores = TallyIndentationScores();
            var max = indentWidthScores.Max();
            if (max == 0) return defaultSetting;

            for (var i = 8; i >= 2; i--)
            {
                if (indentWidthScores[i - 1] == max)
                    return i;
            }

            return defaultSetting;
        }

        /// <summary>
        /// A series of tab widths from 2 to 8 are scored according to the leading spaces already present
        /// on the current document.
        /// </summary>
        /// <returns>A tally of all indent widths and their respective scores.</returns>
        private int[] TallyIndentationScores()
        {
            const string pattern = @"^( )+(?=[^ \t])";
            var leadingSpaces = new Regex(pattern, RegexOptions.Compiled);
            var indentWidthScores = new int[8];
            foreach (var spaces in _textDoc.FindMatches(pattern)
                                           .Select(editPoint => editPoint.GetLine())
                                           .Select(line => leadingSpaces.Match(line).Length))
            {
                for (var i = 2; i <= 8; i++)
                {
                    if (i > spaces) break;
                    if (spaces%i == 0)
                        indentWidthScores[i - 1] += spaces;
                }
            }
            return indentWidthScores;
        }
    }
}
