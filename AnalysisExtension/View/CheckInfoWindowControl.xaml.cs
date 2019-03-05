using AnalysisExtension.Model;
using AnalysisExtension.View;
using LoadingControl.Control;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public partial class CheckInfoWindowControl : UserControl
    {
        private List<string> fileType = null;
        private Analysis analysisMode = null;
        private UserControl previousControl;

        public CheckInfoWindowControl(List<string> fileType,Analysis analysisMode,UserControl previousControl)
        {
            this.previousControl = previousControl;
            this.fileType = fileType;
            this.analysisMode = analysisMode;
            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            check_info_analysis_tb.Text += analysisMode.Name;

            foreach (string type in fileType)
            {
                check_info_language_tb.Text = check_info_language_tb.Text + " " + type + " ";
            }
            
        }

        private void OnClickBtNextListener(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
             LoadingAnimation loading = new LoadingAnimation(analysisMode);
             System.Windows.Window window = new System.Windows.Window
             {
                 Title = "wait",
                 Content = loading,
                 Width = 350,
                 Height = 200
             };

            window.ShowDialog();
            //TODO : get transferred reslt
            List<CodeBlock> codeList = new List<CodeBlock>();

            for (int i = 0; i < 10; i++)
            {
                CodeBlock code = new CodeBlock();
                code.CodeBefore = "code before" + "\n" + i + "code Before";
                code.CodeAfter = "code after" + "\n" + i + "code After";
                if (code.CodeBefore.Equals(code.CodeAfter))
                {
                    code.IsDiff = false;
                }
                else
                {
                    code.IsDiff = true;
                }
                codeList.Add(code);
            }

            TransformWindowControl codeWindow = new TransformWindowControl(codeList);
            /*System.Windows.Window */window = new System.Windows.Window
            {
                Title = "wait",
                Content = codeWindow,
                Width = 800,
                Height = 450
            };

            window.ShowDialog();
        }

        private void OnClickBtPreviousListener(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
            System.Windows.Window window = new System.Windows.Window
            {
                Title = "Choose Analysis Window",
                Content = previousControl,
                Width = 800,
                Height = 450
            };
            window.ShowDialog();
        }

        private void OnClickBtCancelListener(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
        }

    }
}
