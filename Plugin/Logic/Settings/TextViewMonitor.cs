using EditorConfig.Core;
using EditorConfig.VisualStudio.Logic.Cleaning;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;

namespace EditorConfig.VisualStudio.Logic.Settings
{
    /// <summary>
    /// This plugin attaches to an editor instance and updates its settings at
    /// the appropriate times
    /// </summary>
    internal class TextViewMonitor : IDisposable
    {
        private readonly IWpfTextView _view;
        private readonly ITextDocument _doc;
        private readonly FileConfiguration _settings;
        private readonly SettingsManager _settingsManager;
        private readonly GlobalSettings _globalSettings;
        private readonly DTE _app;

        public TextViewMonitor(IWpfTextView view, ITextDocument document, DTE app, ErrorListProvider messageList)
        {
            _view = view;
            _doc = document;
            _app = app;
            _settingsManager = new SettingsManager(view, document, messageList);
            _settings = _settingsManager.Settings;

            if (_settings != null)
            {
                _globalSettings = new GlobalSettings(view, app, _settings);
                _view.GotAggregateFocus += ViewOnGotAggregateFocus;
            }

            document.FileActionOccurred += FileActionOccurred;
            view.Closed += Closed;
        }

        private void ViewOnGotAggregateFocus(object sender, EventArgs eventArgs)
        {
            new InitialCleanup(_doc, _app, _settings).Execute();
            _view.GotAggregateFocus -= ViewOnGotAggregateFocus;
        }

        /// <summary>
        /// Reloads the settings when the filename changes
        /// </summary>
        private void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.DocumentRenamed) != 0)
                _settingsManager.LoadSettings(e.FilePath);

            if (_settings != null && _view.HasAggregateFocus)
                _globalSettings.Apply();
        }

        private void Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            _settingsManager.Dispose();
            _globalSettings.Dispose();
            _doc.FileActionOccurred -= FileActionOccurred;
            _view.GotAggregateFocus -= ViewOnGotAggregateFocus;
            _view.Closed -= Closed;
        }
    }
}
