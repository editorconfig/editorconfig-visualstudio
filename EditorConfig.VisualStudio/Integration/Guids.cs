// Guids.cs
// MUST match guids.h
using System;

namespace EditorConfig.VisualStudio.Integration
{
    static class Guids
    {
        // Package level Guids.

        public const string GuidEditorConfigPkgString = "6b4a6b64-eda9-4078-a549-905ed7d6b8aa";
        public static readonly Guid GuidEditorConfigPackage = new Guid(GuidEditorConfigPkgString);

        // Command guids.
        public const string GuidEditorConfigCmdSetString = "9e3b7eff-0961-4438-9461-29e4daaea152";
        public static readonly Guid GuidEditorConfigCmdSet = new Guid(GuidEditorConfigCmdSetString);
        public static readonly Guid GuidEditorConfigCommandCleanupActiveCode = new Guid("2db8f38b-1d85-4567-9c23-20339108788b");

        // Output pane guid.

        public static readonly Guid GuidEditorConfigOutputPane = new Guid("46d681c9-12d3-4c27-9e33-2563ffbecba5");
    };
}
