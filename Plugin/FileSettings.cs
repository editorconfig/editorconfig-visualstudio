using System;

namespace EditorConfig.VisualStudio
{
    internal class FileSettings
    {
        public FileSettings(Results results)
        {
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            TabWidth = IntOrNull(results, "tab_width");
            IndentSize = IntOrNull(results, "indent_size");
            InsertFinalNewLine = BoolOrNull(results, "insert_final_newline");
            TrimTrailingWhitespace = BoolOrNull(results, "trim_trailing_whitespace");

            if (results.ContainsKey("indent_style"))
            {
                switch (results["indent_style"])
                {
                    case "tab":
                        ConvertTabsToSpaces = false;
                        break;
                    case "space":
                        ConvertTabsToSpaces = true;
                        break;
                }
            }

            if (results.ContainsKey("end_of_line"))
            {
                switch (results["end_of_line"])
                {
                    case "lf":
                        EndOfLine = "\n";
                        break;
                    case "cr":
                        EndOfLine = "\r";
                        break;
                    case "crlf":
                        EndOfLine = "\r\n";
                        break;
                }
            }
        }

        public int? TabWidth { get; private set; }
        public int? IndentSize { get; private set; }
        public bool? ConvertTabsToSpaces { get; private set; }
        public string EndOfLine { get; private set; }
        public bool? InsertFinalNewLine { get; private set; }
        public bool? TrimTrailingWhitespace { get; private set; }

        private int? IntOrNull(Results results, string key)
        {
            if (!results.ContainsKey(key))
            {
                return null;
            }

            try
            {
                return Convert.ToInt32(results[key]);
            }
            catch
            {
                return null;
            }
        }

        private bool? BoolOrNull(Results results, string key)
        {
            if (!results.ContainsKey(key))
            {
                return null;
            }

            switch (results[key])
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return null;
            }
        }
    }
}
