using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public class StaticValue
    {

        //public static ResourceManager RESOURCE_MANAGER = new ResourceManager("AnalysisExtension.String",Assembly.GetExecutingAssembly());


        public static void BtCancelListener(object sender, RoutedEventArgs e,UserControl control)
        {
            CloseWindow(control);
        }

        public static void OnPreviewMouseWheelListener(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public static void CloseWindow(UserControl control)
        {
            Window.GetWindow(control).Close();
        }
    }

}
