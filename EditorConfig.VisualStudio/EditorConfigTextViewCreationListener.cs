using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using EditorConfig.VisualStudio.ContentType;

namespace EditorConfig.VisualStudio
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(EditorConfigContentTypeDefinition.EditorConfigContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class VsTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        IVsEditorAdaptersFactoryService AdaptersFactory = null;

        [Import]
        ICompletionBroker CompletionBroker = null;

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        private ErrorListProvider _errorList;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView view = AdaptersFactory.GetWpfTextView(textViewAdapter);

            view.TextBuffer.Properties.GetOrCreateSingletonProperty(() => view);
            _errorList = view.TextBuffer.Properties.GetOrCreateSingletonProperty(() => new ErrorListProvider(ServiceProvider));

            if (_errorList == null)
                return;

            CommandFilter filter = new CommandFilter(view, CompletionBroker);

            IOleCommandTarget next;
            textViewAdapter.AddCommandFilter(filter, out next);
            filter.Next = next;

            view.Closed += OnViewClosed;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            IWpfTextView view = (IWpfTextView)sender;
            view.Closed -= OnViewClosed;

            if (_errorList != null)
            {
                _errorList.Tasks.Clear();
                _errorList.Dispose();
            }
        }
    }
}
