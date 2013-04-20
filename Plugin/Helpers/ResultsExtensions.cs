using System;
using System.Globalization;

namespace EditorConfig.VisualStudio.Helpers
{
    internal static class ResultsExtensions
    {
        internal static void IfHasKeyTrySetting(this Results results, string key, Action<int> setter)
        {
            if (!results.ContainsKey(key)) return;
            int value;
            if (int.TryParse(results[key], NumberStyles.Integer, null, out value))
                setter(value);
        }
    }
}
