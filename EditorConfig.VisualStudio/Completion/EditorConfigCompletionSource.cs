using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Intel = Microsoft.VisualStudio.Language.Intellisense;
using EditorConfig.VisualStudio.ContentType;

namespace EditorConfig.VisualStudio
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(EditorConfigContentTypeDefinition.EditorConfigContentType)]
    [Name("Editor Config Completion")]
    class EditorConfigCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        IClassifierAggregatorService ClassifierAggregatorService = null;

        [Import]
        ITextStructureNavigatorSelectorService NavigatorService = null;

        [Import]
        IGlyphService GlyphService = null;

        private static ImageSource _glyph;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            if (_glyph == null)
            _glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupProperty, StandardGlyphItem.GlyphItemPublic);

            return new EditorConfigCompletionSource(textBuffer, ClassifierAggregatorService, NavigatorService, _glyph);
        }
    }

    class EditorConfigCompletionSource : ICompletionSource
    {
        private ITextBuffer _buffer;
        private bool _disposed = false;
        private IClassifier _classifier;
        private ITextStructureNavigatorSelectorService _navigator;
        private ImageSource _glyph;

        public EditorConfigCompletionSource(ITextBuffer buffer, IClassifierAggregatorService classifier, ITextStructureNavigatorSelectorService navigator, ImageSource glyph)
        {
            _buffer = buffer;
            _classifier = classifier.GetClassifier(buffer);
            _navigator = navigator;
            _glyph = glyph;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_disposed)
                return;

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(snapshot);

            if (triggerPoint == null)
                return;

            SnapshotSpan extent = FindTokenSpanAtPosition(session).GetSpan(snapshot);
            var line = triggerPoint.Value.GetContainingLine().Extent;

            var spans = _classifier.GetClassificationSpans(line);

            List<Intel.Completion> list = new List<Completion>();
            string current = string.Empty;

            foreach (var span in spans)
            {
                if (span.ClassificationType.IsOfType(PredefinedClassificationTypeNames.SymbolDefinition))
                {
                    current = span.Span.GetText();

                    if (!span.Span.Contains(extent))
                        continue;

                    foreach (var key in CompletionItem.Items)
                        list.Add(CreateCompletion(key.Name, key.Description));
                }
                else if (span.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Literal))
                {
                    if (!span.Span.Contains(extent))
                        continue;

                    CompletionItem item = CompletionItem.GetCompletionItem(current);
                    if (item != null){
                    foreach (var value in item.Values)
                        list.Add(CreateCompletion(value));
                }}
            }

            var applicableTo = snapshot.CreateTrackingSpan(extent, SpanTrackingMode.EdgeInclusive);

            completionSets.Add(new CompletionSet("All", "All", applicableTo, list, Enumerable.Empty<Intel.Completion>()));
        }

        private Completion CreateCompletion(string name, string description = null)
        {
            return new Completion(name, name, description, _glyph, null);
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = _navigator.GetTextStructureNavigator(_buffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}