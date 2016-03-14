using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using EditorConfig.VisualStudio.ContentType;

namespace EditorConfig.VisualStudio.Classify
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(EditorConfigContentTypeDefinition.EditorConfigContentType)]
    class EditorConfigClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService Registry { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new EditorConfigClassifier(Registry));
        }
    }
}
