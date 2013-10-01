using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace EditorConfig.VisualStudio
{
    internal class SettingsViewApplier
    {
        public static void Update(IWpfTextView view, FileSettings settings)
        {
            if (settings.InsertFinalNewLine.HasValue)
            {
                ApplyFinalNewLineSettings(view, settings);
            }

            if (settings.TrimTrailingWhitespace.HasValue
                && settings.TrimTrailingWhitespace.Value)
            {
                TrimTrailingWhitespace(view);
            }
        }

        private static bool IsWhiteSpace(Char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                case '\f':
                case '\v':
                    return true;
                default:
                    return false;
            }
        }

        private static void TrimTrailingWhitespace(IWpfTextView view)
        {
            ITextSnapshot snapshot = view.TextSnapshot;
            var lineCount = snapshot.LineCount;

            if (lineCount == 0)
            {
                return;
            }

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                for (int i = 0; i < lineCount; i++)
                {
                    ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);

                    int length = line.Length;

                    if (length == 0)
                    {
                        continue;
                    }

                    string content = line.GetText();

                    int pos = length - 1;
                    while (IsWhiteSpace(content[pos]))
                    {
                        pos --;
                    }

                    if (pos == length - 1)
                    {
                        continue;
                    }

                    edit.Delete(line.Start.Position + pos + 1, length - 1 - pos);
                }

                edit.Apply();
            }
        }

        private static void ApplyFinalNewLineSettings(IWpfTextView view, FileSettings settings)
        {
            ITextSnapshot snapshot = view.TextSnapshot;
            var lineCount = snapshot.LineCount;

            if (lineCount == 0)
            {
                return;
            }

            var newlineCharLength = view.Options.GetNewLineCharacter().Length;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                for (int i = lineCount - 1; i >= 0; i--)
                {
                    ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);

                    int length = line.Length;

                    //if this line has a length we are no longer dealing with 
                    //trailing newlines
                    if (length != 0)
                        break;

                    edit.Delete(line.Start.Position - newlineCharLength, newlineCharLength);
                }
                if (settings.InsertFinalNewLine.Value)
                    edit.Insert(edit.Snapshot.Length, view.Options.GetNewLineCharacter());

                edit.Apply();
            }
        }
    }
}