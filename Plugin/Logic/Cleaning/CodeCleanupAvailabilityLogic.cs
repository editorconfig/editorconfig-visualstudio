using EnvDTE;

namespace EditorConfig.VisualStudio.Logic.Cleaning
{
    /// <summary>
    /// A class for determining if cleanup can/should occur on specified items.
    /// </summary>
    internal class CodeCleanupAvailabilityLogic
    {
        #region Fields

        private readonly EditorConfigPackage _package;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The singleton instance of the <see cref="CodeCleanupAvailabilityLogic"/> class.
        /// </summary>
        private static CodeCleanupAvailabilityLogic _instance;

        /// <summary>
        /// Gets an instance of the <see cref="CodeCleanupAvailabilityLogic"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="CodeCleanupAvailabilityLogic"/> class.</returns>
        internal static CodeCleanupAvailabilityLogic GetInstance(EditorConfigPackage package)
        {
            return _instance ?? (_instance = new CodeCleanupAvailabilityLogic(package));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeCleanupAvailabilityLogic"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private CodeCleanupAvailabilityLogic(EditorConfigPackage package)
        {
            _package = package;
        }

        #endregion Constructors

        #region Internal Methods

        /// <summary>
        /// Determines whether the environment is in a valid state for cleanup.
        /// </summary>
        /// <returns>True if cleanup can occur, false otherwise.</returns>
        internal bool IsCleanupEnvironmentAvailable()
        {
            return _package.IDE.Debugger.CurrentMode == dbgDebugMode.dbgDesignMode;
        }

        /// <summary>
        /// Determines if the specified document should be cleaned up.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="allowUserPrompts">A flag indicating if user prompts should be allowed.</param>
        /// <returns>True if item should be cleaned up, otherwise false.</returns>
        internal bool ShouldCleanup(Document document, bool allowUserPrompts = false)
        {
            return IsCleanupEnvironmentAvailable() && document != null;
        }

        #endregion Internal Methods

    }
}
