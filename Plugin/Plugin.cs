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
            document.FileActionOccurred += FileActionOccurred;

            LoadSettings(document.FilePath);
        }

        /// <summary>
        /// Reloads the settings when the filename changes
        /// </summary>
        void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.DocumentRenamed) != 0)
                LoadSettings(e.FilePath);
        }

        /// <summary>
        /// Loads the settings for the given file path
        /// </summary>
        private void LoadSettings(string path)
        {
            using (var log = new System.IO.StreamWriter(System.IO.Path.GetTempPath() + "editorconfig.log"))
                log.Write("Load settings for file " + path);
        }
    }
}
