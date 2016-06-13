using EnvDTE;
using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.Text;
using EditorConfig.Core;

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
        /// <param name="textBuffer">The text buffer for the document.</param>
        internal void Cleanup(Document document, ITextBuffer textBuffer)
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
                    RunCodeCleanupGeneric(document, textBuffer);

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
        /// <param name="textBuffer">The text buffer for the document.</param>
        private void RunCodeCleanupGeneric(Document document, ITextBuffer textBuffer)
        {
            ITextDocument textDocument;

            var doc = (TextDocument)document.Object("TextDocument");
            textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument);

            var path = doc.Parent.FullName;

            FileConfiguration settings;
            if (!ConfigLoader.TryLoad(path, out settings))
                return;

            using (ITextEdit edit = textBuffer.CreateEdit())
            {
                ITextSnapshot snapshot = edit.Snapshot;

                if (settings.Charset != null && textDocument != null)
                    FixDocumentCharset(textDocument, settings.Charset.Value);

                if (settings.TryKeyAsBool("trim_trailing_whitespace"))
                    TrimTrailingWhitespace(snapshot, edit);

                if (settings.TryKeyAsBool("insert_final_newline"))
                    InsertFinalNewline(snapshot, edit, settings.EndOfLine());

                var eol = settings.EndOfLine();
                FixLineEndings(snapshot, edit, eol);

                edit.Apply();
            }
        }

        private void FixDocumentCharset(ITextDocument document, Charset charset)
        {
            if (charset == Charset.Latin1)
                document.Encoding = Encoding.GetEncoding("ISO-8859-1");
            else if (charset == Charset.UTF8)
                document.Encoding = new UTF8Encoding(false);
            else if (charset == Charset.UTF8BOM)
                document.Encoding = new UTF8Encoding(true);
            else if (charset == Charset.UTF16LE)
                document.Encoding = Encoding.Unicode;
            else if (charset == Charset.UTF16BE)
                document.Encoding = Encoding.BigEndianUnicode;
        }

        internal void TrimTrailingWhitespace(ITextSnapshot snapshot, ITextEdit edit)
        {
            foreach (ITextSnapshotLine line in snapshot.Lines)
            {
                var text = line.GetText();

                if (text != null)
                {
                    int index = text.Length - 1;

                    while (index >= 0 && char.IsWhiteSpace(text[index]))
                        index--;

                    if (index < text.Length - 1)
                        edit.Delete(line.Start.Position + index + 1, text.Length - index - 1);
                }
            }
        }

        /// <summary>
        /// Inserts a newline at the end of the file, if it doesn't already exist.
        /// </summary>
        /// <param name="snapshot">The snapshot of the document to enforce.</param>
        /// <param name="edit">The edit context of the document to enforce.</param>
        /// <param name="eol">The eol character to add.</param>
        internal void InsertFinalNewline(ITextSnapshot snapshot, ITextEdit edit, string eol)
        {
            var line = snapshot.Lines.LastOrDefault();

            if (line != null && !string.IsNullOrWhiteSpace(line.GetText()))
            {
                if (eol == null)
                    eol = Environment.NewLine;

                edit.Insert(line.End.Position, eol);
            }
        }

        /// <summary>
        /// Enforce line endings to follow the rules defined in the EditorConfig file.
        /// </summary>
        /// <param name="snapshot">The snapshot of the document to enforce.</param>
        /// <param name="edit">The edit context of the document to enforce.</param>
        /// <param name="eol">The line ending to enforce.</param>
        internal void FixLineEndings(ITextSnapshot snapshot, ITextEdit edit, string eol)
        {
            if (eol == null)
            {
                return;
            }

            foreach (ITextSnapshotLine line in snapshot.Lines)
            {
                var lineBreak = line.GetLineBreakText();

                if (!string.IsNullOrEmpty(lineBreak) && lineBreak != eol)
                    edit.Replace(line.End.Position, line.LineBreakLength, eol);
            }
        }

        #endregion Private Language Methods
    }
}
