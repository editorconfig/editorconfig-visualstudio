using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace EditorConfig.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [Guid(GuidList.guidPluginPkgString)]
    public sealed class PluginPackage : Package
    {
        private static SaveListener _listener;

        internal static IViewSettingsContainer ViewSettingsContainer
        {
            get { return _listener; }
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Assert(_listener != null);

            _listener.Dispose();
            _listener = null;

            base.Dispose(disposing);
        }

        protected override void Initialize()
        {
            Debug.Assert(_listener == null);

            var rdt = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));
            _listener = new SaveListener(rdt);

            base.Initialize();
        }
    }
}
