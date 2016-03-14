// Guids.cs
// MUST match guids.h
using System;

namespace EditorConfig.VisualStudio
{
    static class GuidList
    {
        public const string guidPluginPkgString = "ab96e999-0d6d-4d1a-a034-debb50fe72d5";
        public const string guidPluginCmdSetString = "ab7de3bb-ac1d-41f9-9f00-ccc4ad314f5d";

        public static readonly Guid guidPluginCmdSet = new Guid(guidPluginCmdSetString);
    };
}
