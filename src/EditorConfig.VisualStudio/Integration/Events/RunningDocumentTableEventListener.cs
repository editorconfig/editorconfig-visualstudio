using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Linq;

namespace EditorConfig.VisualStudio.Integration.Events
{
    /// <summary>
    /// A class that listens for running document table events.
    /// </summary>
    internal class RunningDocumentTableEventListener : BaseEventListener, IVsRunningDocTableEvents3
    {
        #region Fields

        private IVsEditorAdaptersFactoryService _editorAdaptersFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RunningDocumentTableEventListener"/> class.
        /// </summary>
        /// <param name="package">The package hosting the event listener.</param>
        internal RunningDocumentTableEventListener(EditorConfigPackage package)
            : base(package)
        {
            _editorAdaptersFactory = Package.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();

            // Create and store a reference to the running document table.
            RunningDocumentTable = new RunningDocumentTable(package);

            // Register with the running document table for events.
            EventCookie = RunningDocumentTable.Advise(this);
        }

        #endregion Constructors

        #region Internal Events

        /// <summary>
        /// A delegate specifying the contract for a document save event.
        /// </summary>
        /// <param name="document">The document being saved.</param>
        internal delegate void OnDocumentSaveEventHandler(Document document, ITextBuffer textBuffer);

        /// <summary>
        /// An event raised before a document is saved.
        /// </summary>
        internal event OnDocumentSaveEventHandler BeforeSave;

        #endregion Internal Events

        #region Private Properties

        /// <summary>
        /// Gets or sets an event cookie used as a notification token.
        /// </summary>
        private uint EventCookie { get; set; }

        /// <summary>
        /// Gets or sets a reference to the running document table.
        /// </summary>
        private RunningDocumentTable RunningDocumentTable { get; set; }

        #endregion Private Properties

        #region Private Methods

        /// <summary>
        /// Gets the document object from a document cookie.
        /// </summary>
        /// <param name="docCookie">The document cookie.</param>
        /// <returns>The document object, otherwise null.</returns>
        private Document GetDocumentFromCookie(uint docCookie)
        {
            // Retrieve document information from the cookie to get the full document name.
            var documentName = RunningDocumentTable.GetDocumentInfo(docCookie).Moniker;

            // Search against the IDE documents to find the object that matches the full document name.
            return Package.IDE.Documents.OfType<Document>().FirstOrDefault(x => x.FullName == documentName);
        }

        private ITextBuffer GetTextBufferFromCookie(uint docCookie)
        {
            var documentInfo = RunningDocumentTable.GetDocumentInfo(docCookie);

            var textLines = documentInfo.DocData as IVsTextLines;
            var textBufferProvider = documentInfo.DocData as IVsTextBufferProvider;

            ITextBuffer textBuffer = null;

            if (textLines != null)
                textBuffer = _editorAdaptersFactory.GetDataBuffer(textLines);
            else if (textBufferProvider != null && textBufferProvider.GetTextBuffer(out textLines) == 0)
                textBuffer = _editorAdaptersFactory.GetDataBuffer(textLines);

            return textBuffer;
        }

        #endregion Private Methods

        #region IVsRunningDocTableEvents3 Members

        /// <summary>
        /// Called when a document is about to be saved.
        /// </summary>
        /// <param name="docCookie">An abstract value representing the document about to be saved.</param>
        /// <returns>S_OK if successful, otherwise an error code.</returns>
        public int OnBeforeSave(uint docCookie)
        {
            if (BeforeSave != null)
            {
                var document = GetDocumentFromCookie(docCookie);
                var textBuffer = GetTextBufferFromCookie(docCookie);

                BeforeSave(document, textBuffer);
            }

            return VSConstants.S_OK;
        }

        #region Unused Members

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        #endregion Unused Members

        #endregion IVsRunningDocTableEvents3 Members

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            IsDisposed = true;

            if (!disposing || RunningDocumentTable == null || EventCookie == 0) return;
            RunningDocumentTable.Unadvise(EventCookie);
            EventCookie = 0;
        }

        #endregion IDisposable Members
    }
}
