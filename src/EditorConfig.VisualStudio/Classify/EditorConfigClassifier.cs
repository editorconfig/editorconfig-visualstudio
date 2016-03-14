using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace EditorConfig.VisualStudio.Classify
{
    class EditorConfigClassifier : IClassifier
    {
        private static Regex _rxKeywords = new Regex(@"(?<=\=\s?)([a-zA-Z0-9-]+)\b", RegexOptions.Compiled);
        private static Regex _rxIdentifier = new Regex(@"^([^=]+)\b(?=\=?)", RegexOptions.Compiled);
        private static Regex _rxString = new Regex(@"\[([^\]]+)\]", RegexOptions.Compiled);
        private static Regex _rxComment = new Regex(@"#.*", RegexOptions.Compiled);
        private static List<Tuple<Regex, IClassificationType>> _map;

        public EditorConfigClassifier(IClassificationTypeRegistryService registry)
        {
            if (_map == null)
                _map = new List<Tuple<Regex, IClassificationType>>
                {
                    {Tuple.Create(_rxComment, registry.GetClassificationType(PredefinedClassificationTypeNames.Comment))},
                    {Tuple.Create(_rxString, registry.GetClassificationType(PredefinedClassificationTypeNames.String))},
                    {Tuple.Create(_rxIdentifier, registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition))},
                    {Tuple.Create(_rxKeywords, registry.GetClassificationType(PredefinedClassificationTypeNames.Literal))},
                };
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> list = new List<ClassificationSpan>();
            ITextSnapshotLine line = span.Start.GetContainingLine();
            string text = line.GetText();

            foreach (var tuple in _map)
                foreach (Match match in tuple.Item1.Matches(text))
                {
                    var str = new SnapshotSpan(line.Snapshot, line.Start.Position + match.Index, match.Length);

                    // Make sure we don't double classify
                    if (list.Any(s => s.Span.IntersectsWith(str)))
                        continue;

                    list.Add(new ClassificationSpan(str, tuple.Item2));
                }

            return list;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }
    }
}