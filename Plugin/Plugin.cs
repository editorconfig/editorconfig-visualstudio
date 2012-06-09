using System.Collections.Generic;
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
        IWpfTextView view;
        Dictionary<string, string> settings;

        public Plugin(IWpfTextView view, ITextDocument document)
        {
            this.view = view;

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
            try
            {
                settings = Core.Parse(path);
                if (settings != null)
                    ApplyLocalSettings();
            }
            catch
            {
                //TODO: Display an appropriate error message
                System.Media.SystemSounds.Beep.Play();
            }
        }

        /// <summary>
        /// Applies settings to the local text editor instance
        /// </summary>
        private void ApplyLocalSettings()
        {
            IEditorOptions options = view.Options;

            if (settings.ContainsKey("tab_width"))
            {
                int value = System.Convert.ToInt32(settings["tab_width"]);
                options.SetOptionValue<int>(DefaultOptions.TabSizeOptionId, value);
            }

            if (settings.ContainsKey("indent_size"))
            {
                int value = System.Convert.ToInt32(settings["indent_size"]);
                options.SetOptionValue<int>(DefaultOptions.IndentSizeOptionId, value);
            }

            if (settings.ContainsKey("indent_style"))
            {
                string value = settings["indent_style"];
                if (value == "tab")
                    options.SetOptionValue<bool>(DefaultOptions.ConvertTabsToSpacesOptionId, false);
                else if (value == "space")
                    options.SetOptionValue<bool>(DefaultOptions.ConvertTabsToSpacesOptionId, true);
            }

            if (settings.ContainsKey("end_of_line"))
            {
                string value = settings["end_of_line"];
                if (value == "lf")
                {
                    options.SetOptionValue<string>(DefaultOptions.NewLineCharacterOptionId, "\n");
                    options.SetOptionValue<bool>(DefaultOptions.ReplicateNewLineCharacterOptionId, false);
                }
                else if (value == "cr")
                {
                    options.SetOptionValue<string>(DefaultOptions.NewLineCharacterOptionId, "\r");
                    options.SetOptionValue<bool>(DefaultOptions.ReplicateNewLineCharacterOptionId, false);
                }
                else if (value == "crlf")
                {
                    options.SetOptionValue<string>(DefaultOptions.NewLineCharacterOptionId, "\r\n");
                    options.SetOptionValue<bool>(DefaultOptions.ReplicateNewLineCharacterOptionId, false);
                }
            }
        }
    }
}
