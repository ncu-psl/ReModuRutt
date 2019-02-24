using AnalysisExtension.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public partial class TransformWindowControl : UserControl
    {
        private List<Code> code = null;

        public TransformWindowControl(List<Code> code)
        {
            InitializeComponent();
            this.code = code;
            codeList.ItemsSource = code;
        }

        private void OnPreviewMouseWheelListener(object sender, MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender,e);
        }
    }
}
