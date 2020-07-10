using AnalysisExtension.ExceptionModel;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Windows;

namespace AnalysisExtension.PlugInMode
{
    public class PlugInTool
    {
        private static PlugInTool currentPlugInTool = null;

        private FileTreeNode fileList = null;

        private DTE2 dte;
        private ProjectItems projs;
        private bool isProject = false;

        //-----init-----
        private PlugInTool()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                dte = (DTE2)Package.GetGlobalService(typeof(DTE));
                projs = dte.Solution.Item(1).ProjectItems;
            }
            catch
            {
                throw new ProjectNotOpenException();
            }

            string projectPath;
            string projectName;

            if (dte.Solution.FileName.Length <= 0)
            {//if not project
                projectPath = dte.ActiveDocument.FullName;
                projectName = dte.ActiveDocument.Name;
                fileList = new FileTreeNode(projectName, projectPath);
                isProject = false;
            }
            else
            {
                projectPath = dte.Solution.FileName;
                projectName = StaticValue.GetNameFromPath(projectPath);
                isProject = true;

                fileList = new FileTreeNode(projectName, projectPath);
            }        
        }

        public static PlugInTool GetInstancePlugInTool()
        {
            
            if (currentPlugInTool == null)
            {
                currentPlugInTool = new PlugInTool();
            }

            return currentPlugInTool;
        }

        //-----get file list-----
        public FileTreeNode GetFileList()
        {
            fileList.InitSubNode();
            if (isProject)
            {
                AddProjectItem(projs, fileList);
            }
            else
            {
                FileTreeNode treeNode = new FileTreeNode(fileList.Name, fileList.Path);
                fileList.AddSubNode(treeNode);
            }
            return fileList;
        }

        private void AddProjectItem(ProjectItems projs, FileTreeNode topNode)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //add list
            foreach (ProjectItem item in projs)
            {
                ProjectItem itemNow = item;
                FileTreeNode node = new FileTreeNode(item.Name, item.FileNames[0]);

                //if have subitem and is file
                if (itemNow.ProjectItems != null && !StaticValue.IsFile(node.Name))
                {
                    //add folder and subFile
                    topNode.AddSubNode(node);
                    AddProjectItem(itemNow.ProjectItems, node);
                }
                else
                {
                    //add file
                    topNode.AddSubNode(node);
                }
            }            
        }
 
    }
}
