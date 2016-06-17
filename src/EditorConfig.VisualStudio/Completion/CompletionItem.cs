using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.VisualStudio
{
    class CompletionItem
    {
        private static List<CompletionItem> _dic = new List<CompletionItem>
        {
            {new CompletionItem("root", "Special property that should be specified at the top of the file outside of any sections. Set to “true” to stop .editorconfig files search on current file.", "true")},
            {new CompletionItem("indent_style", "Indentation Style", "tab", "space")},
            {new CompletionItem("indent_size", "A whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported). When set to tab, the value of tab_width (if specified) will be used", "tab") },
            {new CompletionItem("tab_width", "A whole number defining the number of columns used to represent a tab character. This defaults to the value of indent_size and doesn't usually need to be specified.") },
            {new CompletionItem("end_of_line", "Line ending file format (Unix, DOS, Mac)", "lf", "crlf", "cr") },
            {new CompletionItem("charset", "File character encoding, NOTE: if visual studio thinks your file is ascii it will always save it as us-ascii", "latin1", "utf-8", "utf-16be", "utf-16le", "utf-8-bom")},
            {new CompletionItem("trim_trailing_whitespace", "Denotes whether whitespace is allowed at the end of lines", "true", "false")},
            {new CompletionItem("insert_final_newline", "Denotes whether file should end with a newline", "true", "false")},
            {new CompletionItem("max_line_length", "Forces hard line wrapping after the amount of characters specified (NOT YET SUPPORTED IN VISUAL STUDIO)")},
        };

        private CompletionItem(string name, string description, params string[] values)
        {
            this.Name = name;
            this.Description = description;
            this.Values = values;
        }

        public static IEnumerable<CompletionItem> Items
        {
            get { return _dic; }
        }

        public static CompletionItem GetCompletionItem(string name)
        {
            return _dic.SingleOrDefault(c => c.Name == name);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Values { get; set; }
    }
}
