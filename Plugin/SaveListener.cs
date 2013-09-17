using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace EditorConfig.VisualStudio
{
    internal class SaveListener : ListenerBase, IViewSettingsContainer, IDisposable
    {
        private readonly Dictionary<string, Tuple<IWpfTextView, FileSettings>> views = new Dictionary<string, Tuple<IWpfTextView, FileSettings>>();

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
            Tuple<IWpfTextView, FileSettings> tuple;
            if (!views.TryGetValue(filepath, out tuple))
            {
                // This view isn't monitored by the Plugin
                // For instance, this will occur when working with
                // a non text view (e.g. An icon editor view)
                return;
            }

            SettingsViewApplier.Update(tuple.Item1, tuple.Item2);
        }

        public void Register(string filepath, IWpfTextView view, FileSettings settings)
        {
            Debug.Assert(!views.ContainsKey(filepath));

            views.Add(filepath, new Tuple<IWpfTextView, FileSettings>(view, settings));
        }

        public void Unregister(string filepath)
        {
            Debug.Assert(views.ContainsKey(filepath));

            views.Remove(filepath);
        }

        public void Update(string oldFilepath, string newFilePath, FileSettings newSettings)
        {
            var view = views[oldFilepath].Item1;

            Unregister(oldFilepath);
            Register(newFilePath, view, newSettings);
        }
    }
}
