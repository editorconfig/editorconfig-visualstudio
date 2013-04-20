// Guids.cs
// MUST match guids.h
using System;

namespace EditorConfig.VisualStudio.Integration
{
    static class GuidList
    {
        public const string GuidEditorConfigPkgString = "6b4a6b64-eda9-4078-a549-905ed7d6b8aa";
        public const string GuidEditorConfigCmdSetString = "a5864348-bcdd-48e4-8659-e2fd867768d0";

        public static readonly Guid GuidEditorConfigCmdSet = new Guid(GuidEditorConfigCmdSetString);
    };
}