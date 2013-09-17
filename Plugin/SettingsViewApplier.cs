using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace EditorConfig.VisualStudio
{
    internal class SettingsViewApplier
    {
        public static void Update(IWpfTextView view, FileSettings settings)
        {
            if (settings.InsertFinalNewLine.HasValue
                && settings.InsertFinalNewLine.Value)
            {
                EnsureTrailingNewLine(view);
            }
        }

        private static void EnsureTrailingNewLine(IWpfTextView view)
        {
            ITextSnapshot snapshot = view.TextSnapshot;
            var lineCount = snapshot.LineCount;

            if (lineCount == 0)
            {
                return;
            }

            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineCount - 1);
            if (line.Length == 0)
            {
                return;
            }

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                edit.Insert(snapshot.Length, view.Options.GetNewLineCharacter());
                edit.Apply();
            }
        }
    }
}
