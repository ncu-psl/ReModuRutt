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

        private List<FileTreeNode> nodeList = null;
        private List<FileTreeNode> chooseNodeList = null;
        
        public ChooseFileWindowControl()
        {
            Refresh();
        }

        private void Refresh()
        {
            dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            InitializeComponent();

            nodeList = new List<FileTreeNode>();

            //get file list in project
            ThreadHelper.ThrowIfNotOnUIThread();
           
            ProjectItems projs = dte.Solution.Item(1).ProjectItems;

            AddProjectItem(projs);
        }

        private void AddProjectItem(ProjectItems projs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
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

                //if have subitem
                if (itemNow.ProjectItems != null)
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

            if (!treeViewItem.HasHeader)
            {
                treeViewItem.Header = topNode.Name;
            }

            foreach (ProjectItem item in projs)
            {
                ProjectItem itemNow = item;
                FileTreeNode node = new FileTreeNode(item.Name, item.FileNames[0]);                
                
                if (itemNow.ProjectItems != null)
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

        private string GetNameFromPath(string path)
        {
            string name = null;
            string[] split = path.Split('\\');

            name = split[split.Length - 1];

            return name;
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