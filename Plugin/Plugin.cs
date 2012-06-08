using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace EditorConfig.VisualStudio
{
    /// <summary>
    /// This plugin attaches to an editor instance and updates its settings at
    /// the appropriate times
    /// </summary>
    internal class Plugin
    {
        public Plugin(IWpfTextView view, ITextDocument document)
        {
            using (var log = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "editorconfig.log"))
                log.Write("Plugin object created for document " + document.FilePath);
        }
    }
}
