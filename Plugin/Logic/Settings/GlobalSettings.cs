using EnvDTE;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;

namespace EditorConfig.VisualStudio.Logic.Settings
{
    using Helpers;

    internal class GlobalSettings : IDisposable
    {
        private readonly IWpfTextView _view;
        private readonly DTE _app;
        private readonly Dictionary<string, object> _savedGlobalProps = new Dictionary<string, object>();
        private readonly string[] _globalPropKeys = new[] { "TabSize", "IndentSize", "InsertTabs" };
        private readonly Results _settings;
        private readonly Properties _editorProps;

        internal GlobalSettings(IWpfTextView view, DTE app, Results settings)
        {
            _view = view;
            _app = app;
            _settings = settings;

            try
            {
                var type = _view.TextDataModel.ContentType.TypeName;
                _editorProps = _app.Properties["TextEditor", type];
            }
            catch
            {
                // If the above code didn't work, this particular content type
                // didn't need its settings changed anyhow
            }

            view.GotAggregateFocus += GotAggregateFocus;
            view.LostAggregateFocus += LostAggregateFocus;
        }

        /// <summary>
        /// Updates the global settings when the local editor receives focus
        /// </summary>
        private void GotAggregateFocus(object sender, EventArgs e)
        {
            if (_settings == null) return;
            Save();
            Apply();
        }

        private void LostAggregateFocus(object sender, EventArgs eventArgs)
        {
            if (_settings == null) return;
            Restore();
        }

        /// <summary>
        /// Applies settings to the global Visual Studio application. Some
        /// source-code formatters, such as curly-brace auto-indenter, ignore
        /// the local text editor settings. This causes horrible bugs when
        /// the local text-editor settings disagree with the formatter's
        /// settings. To fix this, just apply the same settings at the global
        /// application level as well.
        /// </summary>
        internal void Apply()
        {
            if (_editorProps == null) return;

            _settings.IfHasKeyTrySetting("tab_width", i => _editorProps.Item("TabSize").Value = i);

            _settings.IfHasKeyTrySetting("indent_size", i => _editorProps.Item("IndentSize").Value = i);

            if (!_settings.ContainsKey("indent_style")) return;
            switch (_settings["indent_style"])
            {
                case "tab":
                    _editorProps.Item("InsertTabs").Value = true;
                    break;
                case "space":
                    _editorProps.Item("InsertTabs").Value = false;
                    break;
            }
        }

        internal void Save()
        {
            if (_editorProps == null) return;

            _savedGlobalProps.Clear();
            foreach (var key in _globalPropKeys)
                _savedGlobalProps.Add(key, _editorProps.Item(key).Value);
        }

        /// <summary>
        /// Restores the global Visual Studio settings back to the way they were
        /// when SaveGlobalSettings() was called.
        /// </summary>
        internal void Restore()
        {
            if (_editorProps == null) return;

            foreach (var key in _savedGlobalProps.Keys)
                _editorProps.Item(key).Value = _savedGlobalProps[key];
        }

        public void Dispose()
        {
            _view.GotAggregateFocus -= GotAggregateFocus;
            _view.LostAggregateFocus -= LostAggregateFocus;
            Restore();
        }
    }
}
