using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension.View
{
    /// <summary>
    /// InputDialog.xaml 的互動邏輯
    /// </summary>
    public partial class InputDialog : UserControl
    {
        public string Input { get; set; }
        public bool HasInput { get; set; }

        public InputDialog(string content,string defaultInput)
        {            
            InitializeComponent();
            HasInput = false;
            Input = defaultInput;
            input_dialog_text_box.Text = Input;
            input_dialog_content.Text = content;
        }

        private void OnClickBtOKListener(object sender, RoutedEventArgs e)
        {
            HasInput = true;
            Input = input_dialog_text_box.Text;
            StaticValue.CloseWindow(this);
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            HasInput = false;
            StaticValue.CloseWindow(this);
        }
    }
}
