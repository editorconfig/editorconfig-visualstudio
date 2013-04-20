using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;

namespace EditorConfig.VisualStudio.Settings
{
    internal class Loader : IDisposable
    {
        internal Results Settings { get; private set; }
        private readonly IWpfTextView _view;
        private LocalSettings _localSettings;
        private readonly ErrorListProvider _messageList;
        private ErrorTask _message;

        internal Loader(IWpfTextView view, ITextDocument document, ErrorListProvider messageList)
        {
            _view = view;
            _messageList = messageList;
            _message = null;

            LoadSettings(document.FilePath);
        }

        /// <summary>
        /// Loads the settings for the given file path
        /// </summary>
        internal void LoadSettings(string path)
        {
            ClearMessage();
            Settings = null;

            // Prevent parsing of internet-located documents,
            // or documents that do not have proper paths.
            if (path.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || path.Equals("Temp.txt"))
                return;

            try
            {
                Settings = Core.Parse(path);
                if (_localSettings == null)
                    _localSettings = new LocalSettings(_view, Settings);
                _localSettings.Apply();
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
        /// Adds an error message to the Visual Studio tasks pane
        /// </summary>
        private void ShowError(string path, string text)
        {
            _message = new ErrorTask
            {
                ErrorCategory = TaskErrorCategory.Error,
                Category = TaskCategory.Comments,
                Document = path,
                Line = 0,
                Column = 0,
                Text = text
            };

            _messageList.Tasks.Add(_message);
            _messageList.Show();
        }

        /// <summary>
        /// Removes the file's messages, if any
        /// </summary>
        private void ClearMessage()
        {
            if (_message != null)
                _messageList.Tasks.Remove(_message);
            _message = null;
        }

        public void Dispose()
        {
            ClearMessage();
        }
    }
}
