using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using AnalysisExtension.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public partial class CheckInfoWindowControl : UserControl
    {
        private Analysis analysisMode = null;
        private UserControl previousControl;
        private List<string> typeList = null;
        private AnalysisTool analysisTool = AnalysisTool.GetInstance();
        private FileLoader fileLoader = FileLoader.GetInstance();

        BackgroundWorker backgroundWorker = new BackgroundWorker();
        LoadingWindowControl loading = new LoadingWindowControl();


        public CheckInfoWindowControl(UserControl previousControl)
        {
            this.previousControl = previousControl;
            analysisMode = analysisTool.GetAnalysisMode();

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompletedAsync;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;

            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            check_info_analysis_tb.Text = "Analysis mode : " + analysisMode.Name;      
        }

        //------------tool-------------
        private void ShowWaitAnimationWindow()
        {
            Window waitingWindow = new System.Windows.Window
            {
                Title = "wait",
                Content = loading,
                Width = 350,
                Height = 200
            };
            waitingWindow.ShowDialog();
        }

        private void ShowNextWindow()
        {
            Refresh();
            StaticValue.WINDOW.Content = new TransformWindowControl(this);            
        }

        //----------Listener---------------
        private void OnClickBtNextListener(object sender, System.Windows.RoutedEventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
            ShowWaitAnimationWindow();
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

        //-----backgroundworker test
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = analysisTool.AnalysisMethod(backgroundWorker);
        }

        private void BackgroundWorker_RunWorkerCompletedAsync(object sender, RunWorkerCompletedEventArgs e)
        {
            List<ICodeBlock>[][] result = (List<ICodeBlock>[][])e.Result;
            analysisTool.SetFinalList(result);//copy to final list 
            Dispatcher.Invoke(()=> {
                ShowNextWindow();
            });            
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            loading.ChangeProgress(e);
        }
    }
}
