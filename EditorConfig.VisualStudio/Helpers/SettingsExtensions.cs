using EditorConfig.Core;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EditorConfig.VisualStudio.Helpers
{
    internal static class ConfigLoader
    {
        public static bool TryLoad(string path, out FileConfiguration fileConfiguration)
        {
            fileConfiguration = null;
            var parser = new EditorConfigParser();
            var configurations = parser.Parse(path);
            if (!configurations.Any()) return false;
            fileConfiguration = configurations.First();
            return true;
        }
    }

    internal static class SettingsExtensions
    {
        private static readonly Regex Cr = new Regex("^cr", RegexOptions.Compiled);
        private static readonly Regex Lf = new Regex("lf$", RegexOptions.Compiled);

        internal static bool IfHasKeyTrySetting(this FileConfiguration results, string key, Action<int> setter)
        {
            if (!results.Properties.ContainsKey(key)) return false;
            int value;
            if (!int.TryParse(results.Properties[key], NumberStyles.Integer, null, out value)) return false;
            setter(value);
            return true;
        }

        internal static bool TryKeyAsBool(this FileConfiguration results, string key)
        {
            return results.Properties.ContainsKey(key) && results.Properties[key] == "true";
        }

        internal static string EndOfLine(this FileConfiguration results)
        {
            const string key = "end_of_line";
            return results.Properties.ContainsKey(key) ? Lf.Replace(Cr.Replace(results.Properties[key], "\r"), "\n") : null;
        }
    }
}
