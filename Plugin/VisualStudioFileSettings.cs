using System;
using EditorConfig.Core;

namespace EditorConfig.VisualStudio
{
    /// <summary>
    /// Small wrapper class that exposes specific visual studio editor config settings and in a way they are ready consumable 
    /// by the plugin
    /// </summary>
    internal class VisualStudioFileSettings
    {
        public VisualStudioFileSettings(FileConfiguration results)
        {
            if (results == null)
                throw new ArgumentNullException("results");

            TabWidth = results.TabWidth;
            IndentSize = results.IndentSize.NumberOfColumns;
            InsertFinalNewLine = results.InsertFinalNewline;
            TrimTrailingWhitespace = results.TrimTrailingWhitespace;

            if (results.IndentStyle.HasValue)
            {
                switch (results.IndentStyle.Value)
                {
                    case IndentStyle.Tab:
                        ConvertTabsToSpaces = false;
                        break;
                    case IndentStyle.Space:
                        ConvertTabsToSpaces = true;
                        break;
                }
            }

            if (results.EndOfLine.HasValue)
            {
                switch (results.EndOfLine.Value)
                {
                    case Core.EndOfLine.LF:
                        EndOfLine = "\n";
                        break;
                    case Core.EndOfLine.CR:
                        EndOfLine = "\r";
                        break;
                    case Core.EndOfLine.CRLF:
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
    }
}
