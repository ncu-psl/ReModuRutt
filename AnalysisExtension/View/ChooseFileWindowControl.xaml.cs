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
                string name = "name";
                if (i % 3 == 0)
                {
                    name = name + i + ".cs";
                }
                else if (i % 3 == 1)
                {
                    name = name + i + ".c";
                }
                else
                {
                    name = name + i + ".f";
                }
                nodeList.Add(new FileTreeNode(name, "path" + i));
            }

            this.fileList.ItemsSource = nodeList;
        }

        private void ShowNextWindow()
        {
            Window window = new Window
            {
                Title = "Choose Analysis Window",
                Content = new ChooseAnalysisWindowControl(chooseNodeList),
                Width = 800,
                Height = 450
            };

            window.ShowDialog();
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
                Refresh();
                StaticValue.CloseWindow(this);
                ShowNextWindow();
                
            }
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

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.BtCancelListener(sender, e,this);
        }

        private void OnPreviewMouseWheelListener(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender, e);
        }
    }
}


//TODO : tree structure