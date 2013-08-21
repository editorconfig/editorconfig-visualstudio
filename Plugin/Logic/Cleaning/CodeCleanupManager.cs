using EnvDTE;
using System;
using System.Text.RegularExpressions;

namespace EditorConfig.VisualStudio.Logic.Cleaning
{
    using Helpers;

    /// <summary>
    /// A manager class for cleaning up code.
    /// </summary>
    internal class CodeCleanupManager
    {
        #region Fields

        private readonly EditorConfigPackage _package;

        private readonly Regex _trailingWhitespaces = new Regex(@"[ \t]+$", RegexOptions.Compiled);

        private readonly UndoTransactionHelper _undoTransactionHelper;

        private readonly CodeCleanupAvailabilityLogic _codeCleanupAvailabilityLogic;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The singleton instance of the <see cref="CodeCleanupManager"/> class.
        /// </summary>
        private static CodeCleanupManager _instance;

        /// <summary>
        /// Gets an instance of the <see cref="CodeCleanupManager"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="CodeCleanupManager"/> class.</returns>
        internal static CodeCleanupManager GetInstance(EditorConfigPackage package)
        {
            return _instance ?? (_instance = new CodeCleanupManager(package));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeCleanupManager"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private CodeCleanupManager(EditorConfigPackage package)
        {
            _package = package;

            _undoTransactionHelper = new UndoTransactionHelper(_package.IDE, "EditorConfig Cleanup");

            _codeCleanupAvailabilityLogic = CodeCleanupAvailabilityLogic.GetInstance(_package);
        }

        #endregion Constructors

        #region Internal Methods

        /// <summary>
        /// Attempts to run code cleanup on the specified document.
        /// </summary>
        /// <param name="document">The document for cleanup.</param>
        internal void Cleanup(Document document)
        {
            if (!_codeCleanupAvailabilityLogic.ShouldCleanup(document, true)) return;

            // Make sure the document to be cleaned up is active, required for some commands like format document.
            document.Activate();

            if (_package.IDE.ActiveDocument != document)
            {
                OutputWindowHelper.WriteLine(document.Name + " did not complete activation before cleaning started.");
            }

            _undoTransactionHelper.Run(
                delegate
                {
                    _package.IDE.StatusBar.Text = String.Format("EditorConfig is cleaning '{0}'...", document.Name);

                    // Perform the set of configured cleanups based on the language.
                    RunCodeCleanupGeneric(document);

                    _package.IDE.StatusBar.Text = String.Format("EditorConfig cleaned '{0}'.", document.Name);
                },
                delegate(Exception ex)
                {
                    OutputWindowHelper.WriteLine(String.Format("EditorConfig stopped cleaning '{0}': {1}", document.Name, ex));
                    _package.IDE.StatusBar.Text = String.Format("EditorConfig stopped cleaning '{0}'.  See output window for more details.", document.Name);
                });
        }

        #endregion Internal Methods

        #region Private Language Methods

        /// <summary>
        /// Attempts to run code cleanup on the specified generic document.
        /// </summary>
        /// <param name="document">The document for cleanup.</param>
        private void RunCodeCleanupGeneric(Document document)
        {
            var doc = (TextDocument)document.Object("TextDocument");
            var settings = Core.Parse(doc.Parent.FullName);

            if (settings.TryKeyAsBool("insert_final_newline"))
                InsertFinalNewline(doc, settings.EndOfLine());

            if (settings.TryKeyAsBool("trim_trailing_whitespace"))
                TrimTrailingWhitespace(doc);

            var eol = settings.EndOfLine();
            FixLineEndings(doc, eol);
        }

        internal void TrimTrailingWhitespace(TextDocument doc)
        {
            foreach (var cursor in doc.FindMatches(_package.UsePOSIXRegEx ? @":b+\n" : @"[ \t]+\r?\n"))
                cursor.Delete(_trailingWhitespaces.Match(cursor.GetLine()).Length);
        }

        /// <summary>
        /// Inserts a newline at the end of the file, if it doesn't already exist.
        /// </summary>
        /// <param name="textDocument">The text document, on which to operate.</param>
        /// <param name="eol">The eol character to add.</param>
        internal void InsertFinalNewline(TextDocument textDocument, string eol)
        {
            var cursor = textDocument.EndPoint.CreateEditPoint();
            if (cursor.AtStartOfLine) return;

            if (eol == null)
            {
                cursor.LineUp();
                cursor.EndOfLine();
                eol = cursor.GetText(1);
                if (eol == @"\r")
                    eol += @"\n";
                else if (eol != @"\n")
                    eol = Environment.NewLine;
                cursor.EndOfDocument();
            }

            cursor.Insert(eol);
        }

        /// <summary>
        /// Enforce line endings to follow the rules defined in the EditorConfig file.
        /// </summary>
        /// <param name="doc">The document to enforce.</param>
        /// <param name="eol">The line ending to enforce.</param>
        internal void FixLineEndings(TextDocument doc, string eol)
        {
            if (eol == null)
            {
                return;
            }
            for (var cursor = doc.StartPoint.CreateEditPoint(); !cursor.AtEndOfDocument; cursor.LineDown())
            {
                cursor.EndOfLine();
                if (cursor.GetText(1) != eol)
                    cursor.ReplaceText(1, eol, 0);
            }
        }

        #endregion Private Language Methods
    }
}
