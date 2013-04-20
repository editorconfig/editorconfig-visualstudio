using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;

namespace EditorConfig.VisualStudio
{
    using Settings;

    /// <summary>
    /// This plugin attaches to an editor instance and updates its settings at
    /// the appropriate times
    /// </summary>
    internal class Plugin : IDisposable
    {
        private readonly IWpfTextView _view;
        private readonly ITextDocument _doc;
        private readonly Results _settings;
        private readonly Loader _loader;
        private readonly GlobalSettings _globalSettings;

        public Plugin(IWpfTextView view, ITextDocument document, DTE app, ErrorListProvider messageList)
        {
            _view = view;
            _doc = document;
            _loader = new Loader(view, document, messageList);
            _globalSettings = new GlobalSettings(view, app, _settings = _loader.Settings);

            document.FileActionOccurred += FileActionOccurred;
            view.Closed += Closed;
        }

        /// <summary>
        /// Reloads the settings when the filename changes
        /// </summary>
        private void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.DocumentRenamed) != 0)
                _loader.LoadSettings(e.FilePath);

            if (_settings != null && _view.HasAggregateFocus)
                _globalSettings.Apply();
        }

        private void Closed(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            _loader.Dispose();
            _globalSettings.Dispose();
            _doc.FileActionOccurred -= FileActionOccurred;
            _view.Closed -= Closed;
        }
    }
}
