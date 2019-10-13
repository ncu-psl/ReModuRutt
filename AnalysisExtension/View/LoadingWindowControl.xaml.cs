using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
