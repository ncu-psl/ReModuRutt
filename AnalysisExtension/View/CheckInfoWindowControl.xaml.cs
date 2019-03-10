using AnalysisExtension.Model;
using AnalysisExtension.View;
using LoadingControl.Control;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

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
            List<CodeBlock> beforeList = new List<CodeBlock>();
            List<CodeBlock> afterList = new List<CodeBlock>();

            for (int i = 0; i < 10; i++)
            {
                CodeBlock before = new CodeBlock();
                CodeBlock after = new CodeBlock();

                before.Content = "code before" + "\n" + "code Before" + i;
                before.BlockId = i;
                before.BackgroundColor = Colors.White;
                after.Content = "code after" + "\n" + "code After" + i;
                after.BlockId = i;
                after.BackgroundColor = Colors.White;

                beforeList.Add(before);
                afterList.Add(after);
            }

            TransformWindowControl codeWindow = new TransformWindowControl(beforeList,afterList);
            window = new System.Windows.Window
            {
                Title = "result",
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
