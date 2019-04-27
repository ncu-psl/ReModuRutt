using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace AnalysisExtension.PlugInMode
{
    public class PlugInTool
    {
        private static PlugInTool currentPlugInTool = null;

        private FileTreeNode fileList = null;

        private DTE2 dte;
        private ProjectItems projs;

        //-----init-----
        private PlugInTool()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            projs = dte.Solution.Item(1).ProjectItems;
            string projectPath = dte.Solution.FileName;
            string projectName = StaticValue.GetNameFromPath(projectPath);

            fileList = new FileTreeNode(projectName, projectPath);
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
            AddProjectItem(projs,fileList);
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
