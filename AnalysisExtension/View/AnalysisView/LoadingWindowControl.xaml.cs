using System.ComponentModel;
using System.Windows.Controls;

namespace AnalysisExtension.View
{
    public partial class LoadingWindowControl : UserControl
    {
        public LoadingWindowControl()
        {
            InitializeComponent();
        }

        public void ChangeProgress(ProgressChangedEventArgs e)
        {
            loadingProgessBar.Value = e.ProgressPercentage;
            persentText.Text = e.ProgressPercentage + "%";

            if (e.ProgressPercentage == 100)
            {
                StaticValue.CloseWindow(this);
            }
        }
    }
}
