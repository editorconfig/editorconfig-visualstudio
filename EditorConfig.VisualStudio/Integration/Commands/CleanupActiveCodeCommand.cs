using EnvDTE;
using System.ComponentModel.Design;
using EditorConfig.VisualStudio.Helpers;

namespace EditorConfig.VisualStudio.Integration.Commands
{
    using Logic.Cleaning;

    /// <summary>
    /// A command that provides for cleaning up code in the active document.
    /// </summary>
    internal class CleanupActiveCodeCommand : BaseCommand
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupActiveCodeCommand"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        internal CleanupActiveCodeCommand(EditorConfigPackage package)
            : base(package,
                   new CommandID(Guids.GuidEditorConfigCommandCleanupActiveCode, (int)PkgCmdIDList.CmdIDEditorConfigCleanupActiveCode))
        {
            CodeCleanupAvailabilityLogic = CodeCleanupAvailabilityLogic.GetInstance(Package);
            CodeCleanupManager = CodeCleanupManager.GetInstance(Package);
        }

        #endregion Constructors

        #region BaseCommand Members

        /// <summary>
        /// Called to update the current status of the command.
        /// </summary>
        protected override void OnBeforeQueryStatus()
        {
            Enabled = CodeCleanupAvailabilityLogic.ShouldCleanup(ActiveDocument);

            if (Enabled)
            {
                Text = "&Cleanup " + ActiveDocument.Name;
            }
            else
            {
                Text = "&Cleanup Code";
            }
        }

        /// <summary>
        /// Called to execute the command.
        /// </summary>
        protected override void OnExecute()
        {
            CodeCleanupManager.Cleanup(ActiveDocument);
        }

        #endregion BaseCommand Members

        #region Internal Methods

        /// <summary>
        /// Called before a document is saved in order to potentially run code cleanup.
        /// </summary>
        /// <param name="document">The document about to be saved.</param>
        internal void OnBeforeDocumentSave(Document document)
        {
            try
            {
                EditorConfigPackage.IsAutoSaveContext = true;

                using (new ActiveDocumentRestorer(Package))
                {
                    CodeCleanupManager.Cleanup(document);
                }
            }
            finally
            {
                EditorConfigPackage.IsAutoSaveContext = false;
            }
        }

        #endregion Internal Methods

        #region Private Properties

        /// <summary>
        /// Gets the active document.
        /// </summary>
        private Document ActiveDocument { get { return Package.IDE.ActiveDocument; } }

        /// <summary>
        /// Gets or sets the code cleanup availability logic.
        /// </summary>
        private CodeCleanupAvailabilityLogic CodeCleanupAvailabilityLogic { get; set; }

        /// <summary>
        /// Gets or sets the code cleanup manager.
        /// </summary>
        private CodeCleanupManager CodeCleanupManager { get; set; }

        #endregion Private Properties
    }
}
