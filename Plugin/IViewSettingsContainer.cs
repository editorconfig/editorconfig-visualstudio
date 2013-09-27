using System;
using Microsoft.VisualStudio.Text.Editor;

namespace EditorConfig.VisualStudio
{
    internal interface IViewSettingsContainer
    {
        void Register(string filepath, IWpfTextView view, FileSettings settings);
        void Unregister(string filepath, IWpfTextView view);
        void Update(string oldFilepath, string newFilePath, IWpfTextView view, FileSettings newSettings);
    }
}
