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
        private List<CodeBlock> beforeList = null;
        private List<CodeBlock> afterList = null;

        public TransformWindowControl(List<CodeBlock> codeBefore, List<CodeBlock> codeAfter)
        {
            InitializeComponent();
            beforeList = codeBefore;
            afterList = codeAfter;

            codeListBefore.ItemsSource = beforeList;
            codeListAfter.ItemsSource = afterList;
        }

        private void OnPreviewMouseWheelListener(object sender, MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender,e);
        }
    }
}
