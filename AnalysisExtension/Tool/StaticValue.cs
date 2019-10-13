using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public class StaticValue
    {
        //-----value-----
        public static int WINDOW_WIDTH = 800;
        public static int WINDOW_HEIGHT = 400;

        public static string FOLDER_TYPE = "folder";

        public static Window WINDOW = null;
        public static string PARAMETER_BLOCK_TYPE_NAME = "parameter type";
        public static string CODE_BLOCK_TYPE_NAME = "code block type";
        
        public static int CODE_BLOCK_ID_COUNT = 0;
      //  public static int PARAMETER_BLOCK_TYPE_ID_COUNT = 0;

        //-----method-----
        public static void BtCancelListener(object sender, RoutedEventArgs e, UserControl control)
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

        public static string GetNameFromPath(string path)
        {
            string name = null;
            string[] split = path.Split('\\');

            name = split[split.Length - 1];

            return name;
        }
        
        public static bool IsFile(string fileName)
        {
            bool result = false;
            string[] split = fileName.Split('.');

            if (split.Length < 2)
            {
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        public static int GetNextBlockId()
        {
            int idNow = CODE_BLOCK_ID_COUNT;
            CODE_BLOCK_ID_COUNT++;
            return idNow;
        }

    }

}
