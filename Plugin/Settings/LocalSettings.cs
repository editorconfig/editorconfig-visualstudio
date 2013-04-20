using Microsoft.VisualStudio.Text.Editor;
using System.Text.RegularExpressions;

namespace EditorConfig.VisualStudio.Settings
{
    using Helpers;

    internal class LocalSettings
    {
        private readonly IWpfTextView _view;
        private readonly Results _settings;
        private readonly Regex _cr = new Regex("^cr", RegexOptions.Compiled);
        private readonly Regex _lf = new Regex("lf$", RegexOptions.Compiled);

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

            if (!_settings.ContainsKey("end_of_line")) return;

            var newline = _lf.Replace(_cr.Replace(_settings["end_of_line"], "\r"), "\n");
            options.SetOptionValue(DefaultOptions.NewLineCharacterOptionId, newline);
            options.SetOptionValue(DefaultOptions.ReplicateNewLineCharacterOptionId, false);
        }
    }
}
