using Microsoft.VisualStudio.Text.Editor;

namespace EditorConfig.VisualStudio.Logic.Settings
{
    using Helpers;

    internal class LocalSettings
    {
        private readonly IWpfTextView _view;
        private readonly Results _settings;

        internal LocalSettings(IWpfTextView view, Results settings)
        {
            _view = view;
            _settings = settings;
        }

        /// <summary>
        /// Applies settings to the local text editor instance
        /// </summary>
        internal void Apply()
        {
            var options = _view.Options;

            _settings.IfHasKeyTrySetting("tab_width", i => options.SetOptionValue(DefaultOptions.TabSizeOptionId, i));

            _settings.IfHasKeyTrySetting("indent_size", i => options.SetOptionValue(DefaultOptions.IndentSizeOptionId, i));

            if (_settings.ContainsKey("indent_style"))
            {
                switch (_settings["indent_style"])
                {
                    case "tab":
                        options.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, false);
                        break;
                    case "space":
                        options.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, true);
                        break;
                }
            }

            var newline = _settings.EndOfLine();
            if (newline == null) return;
            options.SetOptionValue(DefaultOptions.NewLineCharacterOptionId, newline);
            options.SetOptionValue(DefaultOptions.ReplicateNewLineCharacterOptionId, false);
        }
    }
}
