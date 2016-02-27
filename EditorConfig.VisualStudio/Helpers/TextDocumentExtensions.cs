using EnvDTE;
using System.Collections.Generic;

namespace EditorConfig.VisualStudio.Helpers
{
    /// <summary>
    /// A static helper class for working with text documents.
    /// </summary>
    /// <remarks>
    /// Note:  All POSIXRegEx text replacements search against '\n' but insert/replace
    ///        with Environment.NewLine.  This handles line endings correctly.
    /// </remarks>
    internal static class TextDocumentExtensions
    {
        #region Internal Constants

        /// <summary>
        /// The common set of options to be used for find and replace patterns.
        /// </summary>
        internal const int StandardFindOptions = (int)(vsFindOptions.vsFindOptionsRegularExpression |
                                                       vsFindOptions.vsFindOptionsMatchInHiddenText);

        #endregion Internal Constants

        #region Internal Methods

        /// <summary>
        /// Finds all matches of the specified pattern within the specified text document.
        /// </summary>
        /// <param name="textDocument">The text document.</param>
        /// <param name="patternString">The pattern string.</param>
        /// <returns>The set of matches.</returns>
        internal static IEnumerable<EditPoint> FindMatches(this TextDocument textDocument, string patternString)
        {
            var matches = new List<EditPoint>();
            var cursor = textDocument.StartPoint.CreateEditPoint();
            EditPoint end = null;
            TextRanges dummy = null;

            while (cursor != null && cursor.FindPattern(patternString, StandardFindOptions, ref end, ref dummy))
            {
                matches.Add(cursor.CreateEditPoint());
                cursor = end;
            }

            return matches;
        }

        /// <summary>
        /// Substitutes all occurrences in the specified text document of
        /// the specified pattern string with the specified replacement string.
        /// </summary>
        /// <param name="textDocument">The text document.</param>
        /// <param name="patternString">The pattern string.</param>
        /// <param name="replacementString">The replacement string.</param>
        internal static void SubstituteAllStringMatches(this TextDocument textDocument, string patternString, string replacementString)
        {
            TextRanges dummy = null;
            var lastCount = -1;
            while (textDocument.ReplacePattern(patternString, replacementString, StandardFindOptions, ref dummy))
            {
                // it is possible that the replacements aren't actually being done.  In such a case, we can
                // detect the situation by seeing if the count always remains the same, and if so exiting early.
                if (lastCount == dummy.Count)
                {
                    OutputWindowHelper.WriteLine("EditorConfig had to force a break out of TextDocumentHelper's SubstituteAllStringMatches for a document.");
                    break;
                }
                lastCount = dummy.Count;
            }
        }

        #endregion Internal Methods
    }
}
