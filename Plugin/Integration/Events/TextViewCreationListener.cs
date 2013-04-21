using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace EditorConfig.VisualStudio.Integration.Events
{
    using Logic.Settings;

    /// <summary>
    /// Listens for editor-creation events and attaches a new plugin instance
    /// to each one. Visual Studio to magically instantiates this class and
    /// calls it at the correct time, thanks to the fancy metadata.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TextViewCreationListener : IWpfTextViewCreationListener, IDisposable
    {
        [Import]
        internal ITextDocumentFactoryService DocFactory;

        [Import]
        internal SVsServiceProvider ServiceProvider;

        private ErrorListProvider _messageList;
        private TextViewMonitor _monitor;

        /// <summary>
        /// Creates a plugin instance when a new text editor is opened
        /// </summary>
        public void TextViewCreated(IWpfTextView view)
        {
            ITextDocument document;
            if (!DocFactory.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            var app = (DTE)ServiceProvider.GetService(typeof(DTE));
            if (app == null)
                return;

            if (_messageList == null)
            {
                _messageList = new ErrorListProvider(ServiceProvider)
                    {
                        ProviderGuid = Guids.GuidEditorConfigPackage,
                        ProviderName = "EditorConfig"
                    };
            }

            _monitor = new TextViewMonitor(view, document, app, _messageList);
        }

        public void Dispose()
        {
            _monitor.Dispose();
            _messageList.Dispose();
        }
    }
}
