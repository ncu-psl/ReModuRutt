using AnalysisExtension;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AsyncToolWindowSample.ToolWindows
{
    public partial class ChooseFileWindowControl : UserControl
    {
        private List<FileTreeNode> nodeList = null;
        private List<FileTreeNode> chooseNodeList = null;

        public ChooseFileWindowControl()
        {
            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            nodeList = new List<FileTreeNode>();

            //TODO : get file list in project
            for (int i = 0; i < 30; i++)
            {
                nodeList.Add(new FileTreeNode("name" + i, "path" + i));
            }

            this.fileList.ItemsSource = nodeList;
        }

        private void ShowNextWindow()
        {
            Window window = new Window
            {
                Title = "Choose Analysis Window",
                Content = new ChooseAnalysisWindowControl(chooseNodeList)
            };

            window.ShowDialog();
        }
        
        private void CloseWindow()
        {
            Refresh();
            Window.GetWindow(this).Close();
        }
        //-----------------listener-----------------------------

        private void OnClickBtNextListener(object sender, EventArgs args)
        {
            chooseNodeList = new List<FileTreeNode>();

            foreach (FileTreeNode node in nodeList)
            {
                if (node.IsChoose)
                {
                    chooseNodeList.Add(node);
                }
            }

            if (chooseNodeList.Count == 0)
            {
                MessageBox.Show("not choose file yet.");
            }
            else
            {
                CloseWindow();
                ShowNextWindow();
                
            }
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void OnFileChooseListener(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            FileTreeNode node = checkBox.DataContext as FileTreeNode;
            node.IsChoose = true;
        }

        private void OnFileDisChooseListener(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            FileTreeNode node = checkBox.DataContext as FileTreeNode;
            node.IsChoose = false;
        }


        private void UIElement_OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

    }
}


//TODO : tree structure