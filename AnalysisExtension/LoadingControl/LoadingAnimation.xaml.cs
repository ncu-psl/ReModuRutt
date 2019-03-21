using AnalysisExtension;
using AnalysisExtension.Model;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LoadingControl.Control
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {
        private Analysis analysisMode;

        public LoadingAnimation(Analysis analysis)
        {
            InitializeComponent();


            analysisMode = analysis;
            LoadAsync();
        }

        public void CloseWindow()
        {
            StaticValue.CloseWindow(this);
        }

        private async void LoadAsync()
        {
           await Task.Run(() => analysisMode.AnalysisMethod());
            CloseWindow();
        }
    }
}
