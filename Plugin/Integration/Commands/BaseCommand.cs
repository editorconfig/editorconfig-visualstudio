using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace EditorConfig.VisualStudio.Integration.Commands
{
    /// <summary>
    /// The base implementation of a command.
    /// </summary>
    internal abstract class BaseCommand : OleMenuCommand
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <param name="id">The id for the command.</param>
        protected BaseCommand(EditorConfigPackage package, CommandID id)
            : base(BaseCommand_Execute, id)
        {
            Package = package;

            BeforeQueryStatus += BaseCommand_BeforeQueryStatus;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the hosting package.
        /// </summary>
        protected EditorConfigPackage Package { get; private set; }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the BeforeQueryStatus event of the BaseCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void BaseCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as BaseCommand;
            if (command != null)
            {
                command.OnBeforeQueryStatus();
            }
        }

        /// <summary>
        /// Handles the Execute event of the BaseCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void BaseCommand_Execute(object sender, EventArgs e)
        {
            var command = sender as BaseCommand;
            if (command != null)
            {
                command.OnExecute();
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Called to update the current status of the command.
        /// </summary>
        protected virtual void OnBeforeQueryStatus()
        {
            // By default, commands are always enabled.
            Enabled = true;
        }

        /// <summary>
        /// Called to execute the command.
        /// </summary>
        protected abstract void OnExecute();

        #endregion Methods
    }
}
