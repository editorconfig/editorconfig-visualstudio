using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace EditorConfig.VisualStudio
{
    internal class SaveListener : ListenerBase, IViewSettingsContainer, IDisposable
    {
        private readonly Dictionary<string, ViewsSettings> views = new Dictionary<string, ViewsSettings>();

        private readonly IVsRunningDocumentTable _rdt;
        private readonly uint _pdwCookie;

        public SaveListener(IVsRunningDocumentTable rdt)
        {
            if (rdt == null)
            {
                throw new ArgumentNullException("rdt");
            }

            _rdt = rdt;
            ErrorHandler.ThrowOnFailure(rdt.AdviseRunningDocTableEvents(this, out _pdwCookie));
        }

        public void Dispose()
        {
            Debug.Assert(views.Count == 0);

            _rdt.UnadviseRunningDocTableEvents(_pdwCookie);
        }

        public override int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs,
            IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld,
            IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {

            if ((pszMkDocumentOld == null) && (pszMkDocumentNew == null))
            {
                return VSConstants.S_OK;
            }

            ApplySettings(pszMkDocumentNew);

            return VSConstants.S_OK;
        }

        public override int OnBeforeSave(uint docCookie)
        {
            var filepath = NameFrom(docCookie);

            ApplySettings(filepath);

            return VSConstants.S_OK;
        }

        private string NameFrom(uint docCookie)
        {
            string pbstrMkDocument;
            var ppunkDocData = IntPtr.Zero;

            try
            {
                uint pgrfRdtFlags, pdwReadLocks, pdwEditLocks;
                IVsHierarchy ppHier;
                uint pitemid;

                ErrorHandler.ThrowOnFailure(_rdt.GetDocumentInfo(docCookie,
                    out pgrfRdtFlags, out pdwReadLocks, out pdwEditLocks,
                    out pbstrMkDocument, out ppHier, out pitemid, out ppunkDocData));
            }
            finally
            {
                if (ppunkDocData != IntPtr.Zero)
                    Marshal.Release(ppunkDocData);
            }

            return pbstrMkDocument;
        }

        private void ApplySettings(string filepath)
        {
            ViewsSettings viewsSettings;
            if (!views.TryGetValue(filepath, out viewsSettings))
            {
                // This view isn't monitored by the Plugin
                // For instance, this will occur when working with
                // a non text view (e.g. An icon editor view)
                return;
            }

            SettingsViewApplier.Update(viewsSettings.Views.First(), viewsSettings.Settings);
        }

        public void Register(string filepath, IWpfTextView view, FileSettings settings)
        {
            ViewsSettings viewsSettings;

            if (!views.TryGetValue(filepath, out viewsSettings))
            {
                viewsSettings = new ViewsSettings(view, settings);
                views.Add(filepath, viewsSettings);
            }
            else
            {
                Debug.Assert(!viewsSettings.Views.Contains(view));

                viewsSettings.Views.Add(view);
            }
        }

        public void Unregister(string filepath, IWpfTextView view)
        {
            Debug.Assert(views.ContainsKey(filepath));

            ViewsSettings viewsSettings;

            if (views.TryGetValue(filepath, out viewsSettings))
            {
                Debug.Assert(viewsSettings.Views.Contains(view));

                viewsSettings.Views.Remove(view);

                if (viewsSettings.Views.Count == 0)
                {
                    views.Remove(filepath);
                }
            }
        }

        public void Update(string oldFilepath, string newFilePath, IWpfTextView view, FileSettings newSettings)
        {
            Unregister(oldFilepath, view);
            Register(newFilePath, view, newSettings);
        }

        private class ViewsSettings
        {
            public HashSet<IWpfTextView> Views
            {
                get;
                private set;
            }

            public FileSettings Settings
            {
                get;
                private set;
            }

            public ViewsSettings(IWpfTextView view, FileSettings settings)
            {
                Settings = settings;
                Views = new HashSet<IWpfTextView>();
                Views.Add(view);
            }
        }
    }
}
