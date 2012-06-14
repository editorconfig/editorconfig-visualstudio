using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace EditorConfig.VisualStudio
{
    /// <summary>
    /// Listens for editor-creation events and attaches a new plugin instance
    /// to each one. Visual Studio to magically instantiates this class and
    /// calls it at the correct time, thanks to the fancy metadata.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class PluginFactory : IWpfTextViewCreationListener
    {
        [Import]
        internal ITextDocumentFactoryService docFactory = null;

        [Import]
        internal SVsServiceProvider serviceProvider = null;

        /// <summary>
        /// Creates a plugin instance when a new text editor is opened
        /// </summary>
        public void TextViewCreated(IWpfTextView view)
        {
            ITextDocument document;
            if (!docFactory.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
                return;

            DTE dte = (DTE)serviceProvider.GetService(typeof(DTE));
            if (dte == null)
                return;

            new Plugin(view, document, dte);
        }
    }
}
