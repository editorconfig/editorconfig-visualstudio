using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace EditorConfig.VisualStudio
{
    /// <summary>
    /// Listens for editor-creation events and attaches a new plugin instance
    /// to each one. Visual Studio to magically instantiates this class and
    /// calls it at the correct time, thanks to the fancy metadata.
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class PluginFactory : IVsTextViewCreationListener
    {
        [Import]
        IVsEditorAdaptersFactoryService adapterFactory = null;

        [Import]
        ITextDocumentFactoryService docFactory = null;

        /// <summary>
        /// Creates a plugin instance when a new text editor is opened
        /// </summary>
        public void VsTextViewCreated(IVsTextView adapter)
        {
            IWpfTextView view = adapterFactory.GetWpfTextView(adapter);
            if (view == null)
                return;

            ITextDocument document;
            if (!docFactory.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            new Plugin(view, document);
        }
    }
}
