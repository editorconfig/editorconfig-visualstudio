﻿using System;
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

            if (settings.TrimTrailingWhitespace.HasValue
                && settings.TrimTrailingWhitespace.Value)
            {
                TrimTrailingWhitespace(view);
            }

            if (settings.EndOfLine != null)
            {
                EnsureEndOfLine(view, settings.EndOfLine);
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
                    while (pos >= 0 && IsWhiteSpace(content[pos]))
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

        private static void EnsureEndOfLine(IWpfTextView view, string endOfLine)
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

                    var existingLineBreakText = line.GetLineBreakText();
                    if (existingLineBreakText != endOfLine && !string.IsNullOrEmpty(existingLineBreakText))
                    {
                        edit.Delete(line.End.Position, existingLineBreakText.Length);
                        edit.Insert(line.End.Position, endOfLine);
                    }
                }

                edit.Apply();
            }
        }
    }
}
