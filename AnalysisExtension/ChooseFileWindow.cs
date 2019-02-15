namespace AnalysisExtension
{
    using System;
    using System.Runtime.InteropServices;
    using AsyncToolWindowSample.ToolWindows;
    using Microsoft.VisualStudio.Shell;

    [Guid("f8d0639d-d2bb-46f3-978c-31eb7c1351a4")]
    public class ChooseFileWindow : ToolWindowPane
    {

        public ChooseFileWindow() : base(null)
        {
            this.Caption = "Choose File Window";

            this.Content = new ChooseFileWindowControl();
        }
    }
}
