﻿using AnalysisExtension.ExceptionModel;
using AsyncToolWindowSample.ToolWindows;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace AnalysisExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AnalysisToolListCommand
    {
        public static AnalysisToolListCommand command = null;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("8b7b8f9b-d057-41bb-99f9-4d1a12203118");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisToolListCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AnalysisToolListCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            if (command == null)
            {
                command = this;
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AnalysisToolListCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ToolListCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new AnalysisToolListCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>

        private void Execute(object sender, EventArgs e)
        {
            ExecuteStartWin();
        }

        public void ExecuteStartWin()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                StaticValue.WINDOW = new Window
                {
                    Width = StaticValue.WINDOW_WIDTH,
                    Height = StaticValue.WINDOW_HEIGHT
                };

                StaticValue.WINDOW.Content = new ChooseFileWindowControl();
                StaticValue.WINDOW.ShowDialog();
            }
            catch(ProjectNotOpenException e)
            {
                MessageBox.Show(e.Message);
            }

}
    }
}
