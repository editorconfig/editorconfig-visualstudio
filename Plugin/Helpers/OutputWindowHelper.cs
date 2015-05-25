using EditorConfig.VisualStudio.Integration;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace EditorConfig.VisualStudio.Helpers
{
    /// <summary>
    /// A helper class for writing messages to a EditorConfig output window pane.
    /// </summary>
    internal static class OutputWindowHelper
    {
        #region Fields

        private static IVsOutputWindowPane _editorConfigOutputWindowPane;

        #endregion Fields

        #region Properties

        private static IVsOutputWindowPane EditorConfigOutputWindowPane
        {
            get { return _editorConfigOutputWindowPane ?? (_editorConfigOutputWindowPane = GetEditorConfigOutputWindowPane()); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Writes the specified line to the EditorConfig output pane.
        /// </summary>
        /// <param name="message">The message.</param>
        internal static void WriteLine(string message)
        {
            var outputWindowPane = EditorConfigOutputWindowPane;
            if (outputWindowPane != null)
            {
                outputWindowPane.OutputString(message + Environment.NewLine);
            }
        }

        /// <summary>
        /// Attempts to create and retrieve the EditorConfig output window pane.
        /// </summary>
        /// <returns>The EditorConfig output window pane, otherwise null.</returns>
        private static IVsOutputWindowPane GetEditorConfigOutputWindowPane()
        {
            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null) return null;

            var outputPaneGuid = new Guid(Guids.GuidEditorConfigOutputPane.ToByteArray());
            IVsOutputWindowPane windowPane;

            outputWindow.CreatePane(ref outputPaneGuid, "EditorConfig", 1, 1);
            outputWindow.GetPane(ref outputPaneGuid, out windowPane);

            return windowPane;
        }

        #endregion Methods
    }
}
