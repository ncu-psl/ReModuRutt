using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace AnalysisExtension
{

    internal sealed class ChooseFileWindowCommand
    {
        public static ChooseFileWindowCommand command = null;
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("00606536-0755-446c-80a6-a9fb9217df5d");

        private readonly AsyncPackage package;

        private ChooseFileWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
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

        public static ChooseFileWindowCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ChooseFileWindowCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ExecuteStartWin();
        }

        public void ExecuteStartWin()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ToolWindowPane window = package.FindToolWindow(typeof(ChooseFileWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

    }
}
