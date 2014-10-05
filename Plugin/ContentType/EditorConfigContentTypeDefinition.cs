using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace EditorConfig.VisualStudio.ContentType
{
    class EditorConfigContentTypeDefinition
    {
        public const string EditorConfigContentType = "EditorConfig";

        [Export(typeof(ContentTypeDefinition))]
        [Name(EditorConfigContentType)]
        [BaseDefinition("code")]
        public ContentTypeDefinition IEditorConfigContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(EditorConfigContentType)]
        [FileExtension(".editorconfig")]
        public FileExtensionToContentTypeDefinition EditorConfigFileExtension { get; set; }
    }
}
