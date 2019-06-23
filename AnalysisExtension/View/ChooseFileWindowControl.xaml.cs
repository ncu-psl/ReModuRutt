using AnalysisExtension;
using AnalysisExtension.PlugInMode;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AsyncToolWindowSample.ToolWindows
{
    public partial class ChooseFileWindowControl : UserControl
    {
        private PlugInTool plugInTool = null;
        private List<FileTreeNode> chooseNodeList = null;
        
        public ChooseFileWindowControl()
        {
            chooseNodeList = new List<FileTreeNode>();
            plugInTool = PlugInTool.GetInstancePlugInTool();

            Refresh();
        }

        private void InitTreeView()
        {
            while (fileTreeView.HasItems)
            {
                fileTreeView.Items.RemoveAt(0);                
            }
        }

        private void Refresh()
        {
            InitializeComponent();
            InitTreeView();

            //get file list in project
            FileTreeNode fileList = plugInTool.GetFileList();

            AddProjectItem(fileList,fileTreeView);
        }

        private void AddProjectItem(FileTreeNode fileList,TreeViewItem treeView)
        {
            //set header
            if (!treeView.HasHeader)
            {
                treeView.Header = fileList.Name;
            }

            List<FileTreeNode> subFileNode = fileList.GetSubNode();
            
            //add list
            for (int i = 0; i < subFileNode.Count; i++)
            {
                if (subFileNode[i].HasSubNode() || !StaticValue.IsFile(subFileNode[i].Name))
                {
                    treeView.Items.Add(AddProjectSubItem(subFileNode[i]));
                }
                else
                {
                    treeView.Items.Add(subFileNode[i]);
                }
            }          
        }

        private TreeViewItem AddProjectSubItem(FileTreeNode topNode)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.ItemTemplate = fileTreeView.ItemTemplate;

            AddProjectItem(topNode, treeViewItem);

            return treeViewItem;
        }

        //-----------------tool--------------------------------

        public void ReadFile(List<FileTreeNode> list)
        {
            int fileCount = list.Count;
            StaticValue.FILE_NUMBER = fileCount;
            StaticValue.fileList = new string[fileCount];

            for (int i = 0; i < fileCount; i++)
            {
                StaticValue.fileList[i] = list[i].Path;
            }
        }

        private void ShowNextWindow()
        {
            ReadFile(chooseNodeList);
            List<string> type = StaticValue.GetFileType(chooseNodeList);
            StaticValue.WINDOW.Content = new ChooseAnalysisWindowControl(this, type);
        }
        
        //-----------------listener-----------------------------

        private void OnClickBtNextListener(object sender, EventArgs args)
        {
            if (chooseNodeList.Count == 0)
            {
                MessageBox.Show("not choose file yet.");
            }
            else
            {
                Refresh();
               // StaticValue.CloseWindow(this);
                ShowNextWindow();
                chooseNodeList = new List<FileTreeNode>();
            }
        }

        private void OnFileChooseListener(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            FileTreeNode node = checkBox.DataContext as FileTreeNode;
            if (!chooseNodeList.Contains(node))
            {
                chooseNodeList.Add(node);
            }
        }

        private void OnFileDisChooseListener(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            FileTreeNode node = checkBox.DataContext as FileTreeNode;
            if (chooseNodeList.Contains(node))
            {
                chooseNodeList.Remove(node);
            }
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
