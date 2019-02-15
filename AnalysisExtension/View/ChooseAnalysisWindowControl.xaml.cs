using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public partial class ChooseAnalysisWindowControl : UserControl
    {
        public ChooseAnalysisWindowControl(List<FileTreeNode> chooseFile)
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "We are inside {0}.Button1_Click()", this.ToString()));
        }
    }
}
