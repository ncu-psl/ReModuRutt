namespace AnalysisExtension
{
    using AsyncToolWindowSample.ToolWindows;
    using Microsoft.VisualStudio.Shell;

    public class ChooseFileWindow : ToolWindowPane
    {

        public ChooseFileWindow() : base(null)
        {
            this.Caption = "Choose File Window";

            this.Content = new ChooseFileWindowControl();
        }
    }
}
