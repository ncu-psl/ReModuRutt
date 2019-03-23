using AnalysisExtension;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace AsyncToolWindowSample.ToolWindows
{
    public partial class ChooseFileWindowControl : UserControl
    {
        private DTE2 dte;

        private List<FileTreeNode> chooseNodeList = null;
        
        public ChooseFileWindowControl()
        {
            chooseNodeList = new List<FileTreeNode>();
            Refresh();
        }

        private void Refresh()
        {
            dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            InitializeComponent();

            //get file list in project
            ThreadHelper.ThrowIfNotOnUIThread();
           
            ProjectItems projs = dte.Solution.Item(1).ProjectItems;

            AddProjectItem(projs);
        }

        private void AddProjectItem(ProjectItems projs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            while(fileTreeView.HasItems)
            {
                fileTreeView.Items.RemoveAt(0);
            }

            //set header
            if (!fileTreeView.HasHeader)
            {
                fileTreeView.Header = GetNameFromPath(dte.Solution.FileName);
            }

            //add list
            foreach (ProjectItem item in projs)
            {
                ProjectItem itemNow = item;
                FileTreeNode node = new FileTreeNode(item.Name, item.FileNames[0]);

                //if have subitem and is file
                if (itemNow.ProjectItems != null && !IsFile(node.Name))
                {
                    fileTreeView.Items.Add(AddProjectSubItem(itemNow.ProjectItems, node));
                }
                else
                {
                    //add into list
                    fileTreeView.Items.Add(node);
                } 
            }
        }

        private TreeViewItem AddProjectSubItem(ProjectItems projs,FileTreeNode topNode)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.ItemTemplate = fileTreeView.ItemTemplate;

            if (!treeViewItem.HasHeader)
            {
                treeViewItem.Header = topNode.Name;
            }

            foreach (ProjectItem item in projs)
            {
                ProjectItem itemNow = item;
                FileTreeNode node = new FileTreeNode(item.Name, item.FileNames[0]);

                if (itemNow.ProjectItems != null && !IsFile(node.Name))
                {
                    treeViewItem.Items.Add(AddProjectSubItem(itemNow.ProjectItems,node));
                }
                else
                {
                    treeViewItem.Items.Add(node);
                }
            }
            return treeViewItem;
        }

        //-----------------tool--------------------------------
        private string GetNameFromPath(string path)
        {
            string name = null;
            string[] split = path.Split('\\');

            name = split[split.Length - 1];

            return name;
        }

        private bool IsFile(string fileName)
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

        private void ShowNextWindow()
        {
            System.Windows.Window window = new System.Windows.Window
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
            if (chooseNodeList.Count == 0)
            {
                MessageBox.Show("not choose file yet.");
            }
            else
            {
                Refresh();
                StaticValue.CloseWindow(this);
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
