using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EditorConfig.VisualStudio.Helpers
{
    internal static class SettingsExtensions
    {
        private static readonly Regex Cr = new Regex("^cr", RegexOptions.Compiled);
        private static readonly Regex Lf = new Regex("lf$", RegexOptions.Compiled);

        internal static bool IfHasKeyTrySetting(this Results results, string key, Action<int> setter)
        {
            if (!results.ContainsKey(key)) return false;
            int value;
            if (!int.TryParse(results[key], NumberStyles.Integer, null, out value)) return false;
            setter(value);
            return true;
        }

        internal static bool TryKeyAsBool(this Results results, string key)
        {
            return results.ContainsKey(key) && results[key] == "true";
        }

        internal static string EndOfLine(this Results results)
        {
            const string key = "end_of_line";
            return results.ContainsKey(key) ? Lf.Replace(Cr.Replace(results[key], "\r"), "\n") : null;
        }
    }
}
