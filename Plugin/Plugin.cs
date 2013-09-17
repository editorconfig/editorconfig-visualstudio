using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
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
        ITextDocument document;
        DTE dte;
        ErrorListProvider messageList;
        ErrorTask message;
        FileSettings settings;

        public Plugin(IWpfTextView view, ITextDocument document, DTE dte, ErrorListProvider messageList)
        {
            this.view = view;
            this.document = document;
            this.dte = dte;
            this.messageList = messageList;
            this.message = null;

            document.FileActionOccurred += FileActionOccurred;
            view.GotAggregateFocus += GotAggregateFocus;
            view.Closed += Closed;

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
        void GotAggregateFocus(object sender, EventArgs e)
        {
            if (settings != null)
                ApplyGlobalSettings();
        }

        /// <summary>
        /// Removes the any messages when the document is closed
        /// </summary>
        void Closed(object sender, EventArgs e)
        {
            ClearMessage();

            document.FileActionOccurred -= FileActionOccurred;
            view.GotAggregateFocus -= GotAggregateFocus;
            view.Closed -= Closed;
        }

        /// <summary>
        /// Loads the settings for the given file path
        /// </summary>
        private void LoadSettings(string path)
        {
            ClearMessage();
            settings = null;

            // Prevent parsing of internet-located documents,
            // or documents that do not have proper paths.
            if (path.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || path.Equals("Temp.txt"))
                return;

            try
            {
                settings = new FileSettings(Core.Parse(path));
                ApplyLocalSettings();
            }
            catch (ParseException e)
            {
                ShowError(path, "EditorConfig syntax error in file \"" + e.File + "\", line " + e.Line);
            }
            catch (CoreException e)
            {
                ShowError(path, "EditorConfig core error: " + e.Message);
            }
        }

        /// <summary>
        /// Applies settings to the local text editor instance
        /// </summary>
        private void ApplyLocalSettings()
        {
            IEditorOptions options = view.Options;

            if (settings.TabWidth != null)
            {
                int value = settings.TabWidth.Value;
                options.SetOptionValue<int>(DefaultOptions.TabSizeOptionId, value);
            }

            if (settings.IndentSize != null)
            {
                int value = settings.IndentSize.Value;
                options.SetOptionValue<int>(DefaultOptions.IndentSizeOptionId, value);
            }

            if (settings.ConvertTabsToSpaces != null)
            {
                bool value = settings.ConvertTabsToSpaces.Value;
                options.SetOptionValue<bool>(DefaultOptions.ConvertTabsToSpacesOptionId, value);
            }

            if (settings.EndOfLine != null)
            {
                string value = settings.EndOfLine;
                options.SetOptionValue<string>(DefaultOptions.NewLineCharacterOptionId, value);
                options.SetOptionValue<bool>(DefaultOptions.ReplicateNewLineCharacterOptionId, false);
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
            Properties props;
            try
            {
                string type = view.TextDataModel.ContentType.TypeName;
                props = dte.Properties["TextEditor", type];
            }
            catch
            {
                // If the above code didn't work, this particular content type
                // didn't need its settings changed anyhow
                return;
            }

            if (settings.TabWidth != null)
            {
                int value = settings.TabWidth.Value;
                props.Item("TabSize").Value = value;
            }

            if (settings.IndentSize != null)
            {
                int value = settings.IndentSize.Value;
                props.Item("IndentSize").Value = value;
            }

            if (settings.ConvertTabsToSpaces != null)
            {
                bool value = settings.ConvertTabsToSpaces.Value;
                props.Item("InsertTabs").Value = value;
            }
        }

        /// <summary>
        /// Adds an error message to the Visual Studio tasks pane
        /// </summary>
        void ShowError(string path, string text)
        {
            message = new ErrorTask();
            message.ErrorCategory = TaskErrorCategory.Error;
            message.Category = TaskCategory.Comments;
            message.Document = path;
            message.Line = 0;
            message.Column = 0;
            message.Text = text;

            messageList.Tasks.Add(message);
            messageList.Show();
        }

        /// <summary>
        /// Removes the file's messages, if any
        /// </summary>
        void ClearMessage()
        {
            if (message != null)
                messageList.Tasks.Remove(message);
            message = null;
        }
    }
}
