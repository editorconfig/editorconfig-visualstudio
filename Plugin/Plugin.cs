using System.Collections.Generic;
using EnvDTE;
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
        DTE dte;
        Dictionary<string, string> settings;

        public Plugin(IWpfTextView view, ITextDocument document, DTE dte)
        {
            this.view = view;
            this.dte = dte;

            document.FileActionOccurred += FileActionOccurred;
            view.GotAggregateFocus += GotAggregateFocus;

            LoadSettings(document.FilePath);
        }

        /// <summary>
        /// Reloads the settings when the filename changes
        /// </summary>
        void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.DocumentRenamed) != 0)
                LoadSettings(e.FilePath);

            if (settings != null && view.HasAggregateFocus)
                ApplyGlobalSettings();
        }

        /// <summary>
        /// Updates the global settings when the local editor receives focus
        /// </summary>
        void GotAggregateFocus(object sender, System.EventArgs e)
        {
            if (settings != null)
                ApplyGlobalSettings();
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

        /// <summary>
        /// Applies settings to the global Visual Studio application. Some
        /// source-code formatters, such as curly-brace auto-indenter, ignore
        /// the local text editor settings. This causes horrible bugs when
        /// the local text-editor settings disagree with the formatter's
        /// settings. To fix this, just apply the same settings at the global
        /// application level as well.
        /// </summary>
        private void ApplyGlobalSettings()
        {
            try
            {
                string type = view.TextDataModel.ContentType.TypeName;
                EnvDTE.Properties props = dte.get_Properties("TextEditor", type);

                if (settings.ContainsKey("tab_width"))
                {
                    int value = System.Convert.ToInt32(settings["tab_width"]);
                    props.Item("TabSize").Value = value;
                }

                if (settings.ContainsKey("indent_size"))
                {
                    int value = System.Convert.ToInt32(settings["indent_size"]);
                    props.Item("IndentSize").Value = value;
                }

                if (settings.ContainsKey("indent_style"))
                {
                    string value = settings["indent_style"];
                    if (value == "tab")
                        props.Item("InsertTabs").Value = true;
                    else if (value == "space")
                        props.Item("InsertTabs").Value = false;
                }
            }
            catch
            {
                // If the above code didn't work, this particular content type
                // didn't need its settings changed anyhow
            }
        }
    }
}
