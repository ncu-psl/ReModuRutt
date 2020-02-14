using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.Windows;

namespace AnalysisExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RightClickAddRuleWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a9b3d248-37f8-4a02-b63f-acac06d82643");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RightClickAddRuleWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RightClickAddRuleWindowCommand(Package package/*, OleMenuCommandService commandService*/)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            string selectContent = GetSelection(ServiceProvider);
            StaticValue.AddTextIntoRuleCreateFrame(selectContent);
            MessageBox.Show(selectContent);
        }

        private string GetSelection(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning

            int ok = view.GetNearestPosition(startLine, startColumn, out int position1, out int piVirtualSpaces);
            ok = view.GetNearestPosition(endLine, endColumn, out int position2, out piVirtualSpaces);

            var startPosition = Math.Min(position1, position2);
            var endPosition = Math.Max(position1, position2);

            view.GetSelectedText(out string selectedText);

            return selectedText;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RightClickAddRuleWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
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
        public static void Initialize(Package package)
        {
            Instance = new RightClickAddRuleWindowCommand(package);
        }
    }
}
