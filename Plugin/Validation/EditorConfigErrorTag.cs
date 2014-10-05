using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using EditorConfig.VisualStudio.ContentType;

namespace EditorConfig.VisualStudio.Validation
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(EditorConfigContentTypeDefinition.EditorConfigContentType)]
    [TagType(typeof(ErrorTag))]
    class CheckTextErrorProvider : ITaggerProvider
    {
        [Import]
        IClassifierAggregatorService _classifierAggregatorService = null;

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
                throw new ArgumentException("Buffer is null");

            IWpfTextView view;
            ErrorListProvider errorlist;
            ITextDocument document;
            if (!buffer.Properties.TryGetProperty(typeof(ErrorListProvider), out errorlist) ||
                !buffer.Properties.TryGetProperty(typeof(IWpfTextView), out view) ||
                !TextDocumentFactoryService.TryGetTextDocument(buffer, out document))
            {
                return null;
            }

            return new CheckTextErrorTagger(view, _classifierAggregatorService, errorlist, document) as ITagger<T>;
        }
    }

    class CheckTextErrorTagger : ITagger<IErrorTag>
    {
        private IClassifier _classifier;
        private ErrorListProvider _errorlist;
        private ITextDocument _document;
        private IWpfTextView _view;

        public CheckTextErrorTagger(IWpfTextView view, IClassifierAggregatorService classifier, ErrorListProvider errorlist, ITextDocument document)
        {
            _view = view;
            _classifier = classifier.GetClassifier(view.TextBuffer);
            _errorlist = errorlist;
            _document = document;
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var span = spans[0];
            var line = span.Start.GetContainingLine();
            var classificationSpans = _classifier.GetClassificationSpans(line.Extent);
            string property = null;

            ClearError(line);

            foreach (var cspan in classificationSpans)
            {
                if (cspan.ClassificationType.IsOfType(PredefinedClassificationTypeNames.SymbolDefinition))
                {
                    property = cspan.Span.GetText();
                    CompletionItem item = CompletionItem.GetCompletionItem(property);

                    if (item == null)
                        yield return CreateError(line, cspan, "\"" + property + "\" is not a valid .editorconfig property"); ;
                }
                else if (cspan.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Literal))
                {
                    if (string.IsNullOrEmpty(property))
                        continue;

                    CompletionItem item = CompletionItem.GetCompletionItem(property);
                    if (item == null)
                        continue;

                    string value = cspan.Span.GetText();
                    int intValue;

                    if (!item.Values.Contains(value) && !int.TryParse(value, out intValue))
                        yield return CreateError(line, cspan, "\"" + value + "\" is not a valid value for the '" + property + "' property");
                }
            }
        }

        private TagSpan<ErrorTag> CreateError(ITextSnapshotLine line, ClassificationSpan cspan, string message)
        {
            ErrorTask task = CreateErrorTask(line, cspan, message);

            _errorlist.Tasks.Add(task);

            SnapshotSpan CheckTextSpan = new SnapshotSpan(cspan.Span.Snapshot, new Span(cspan.Span.Start, cspan.Span.Length));
            return new TagSpan<ErrorTag>(CheckTextSpan, new ErrorTag(message, message));
        }

        private ErrorTask CreateErrorTask(ITextSnapshotLine line, ClassificationSpan cspan, string text)
        {
            ErrorTask task = new ErrorTask
            {
                Text = text,
                Line = line.LineNumber,
                Column = cspan.Span.Start.Position - line.Start.Position,
                Category = TaskCategory.Misc,
                ErrorCategory = TaskErrorCategory.Warning,
                Priority = TaskPriority.Low,
                Document = _document.FilePath
            };

            task.Navigate += task_Navigate;

            return task;
        }

        private void task_Navigate(object sender, EventArgs e)
        {
            ErrorTask task = (ErrorTask)sender;
            _errorlist.Navigate(task, new Guid("{00000000-0000-0000-0000-000000000000}"));

            var line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(task.Line);
            var point = new SnapshotPoint(line.Snapshot, line.Start.Position + task.Column);
            _view.Caret.MoveTo(point);
        }

        private void ClearError(ITextSnapshotLine line)
        {
            foreach (ErrorTask existing in _errorlist.Tasks)
            {
                if (existing.Line == line.LineNumber)
                {
                    _errorlist.Tasks.Remove(existing);
                    break;
                }
            }
        }


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
