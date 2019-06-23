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
        private Analysis analysisMode = null;
        private UserControl previousControl;
        private List<string> typeList = null;

        public CheckInfoWindowControl(Analysis analysisMode, UserControl previousControl, List<string> type)
        {
            this.previousControl = previousControl;
            this.analysisMode = analysisMode;
            this.typeList = type;
            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            check_info_analysis_tb.Text = "Analysis mode : " + analysisMode.Name;

            check_info_language_tb.Text = "Language : ";
            
            foreach (string type in typeList)
            {
                check_info_language_tb.Text = check_info_language_tb.Text + " " + type + " ";
            }
            
        }

        //------------tool-------------
        private void ShowWaitAnimationWindow()
        {
            LoadingAnimation loading = new LoadingAnimation(analysisMode);

            System.Windows.Window window = new System.Windows.Window
            {
                Title = "wait",
                Content = loading,
                Width = 350,
                Height = 200
            };

            window.ShowDialog();
        }

        private void ShowNextWindow(Analysis analysisMode)
        {
            StaticValue.WINDOW.Content = new TransformWindowControl(analysisMode,this);
        }

        //----------Listener---------------
        private void OnClickBtNextListener(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
            ShowWaitAnimationWindow();

            ShowNextWindow(analysisMode);
          //  GetTransferredResult();
        }

        private void OnClickBtPreviousListener(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
            StaticValue.WINDOW.Content = this.previousControl;
        }

        private void OnClickBtCancelListener(object sender, System.Windows.RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
        }

    }
}
