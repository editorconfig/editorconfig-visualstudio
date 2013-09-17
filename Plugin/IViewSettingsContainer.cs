using System;
using Microsoft.VisualStudio.Text.Editor;

namespace EditorConfig.VisualStudio
{
    internal interface IViewSettingsContainer
    {
        void Register(string filepath, IWpfTextView view, FileSettings settings);
        void Unregister(string filepath);
        void Update(string oldFilepath, string newFilePath, FileSettings newSettings);
    }
}
