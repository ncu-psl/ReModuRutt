using System.Collections.Generic;

namespace AnalysisExtension
{
    public class FileTreeNode
    {
        private List<FileTreeNode> subNodeList = null;

        public FileTreeNode(string name,string path)
        {
            this.Name = name;
            this.Path = path;
            this.Type = GetFileType();

            subNodeList = new List<FileTreeNode>();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }

        private string GetFileType()
        {
            string fileType = null;
            if (StaticValue.IsFile(Name))
            {
                string[] split = Name.Split('.');
                fileType = split[split.Length - 1];
            }
            else
            {
                fileType = StaticValue.FOLDER_TYPE;
            }
            return fileType;
        }

        //-----sub node-----
        public List<FileTreeNode> GetSubNode()
        {
            return subNodeList;
        }

        public void AddSubNode(FileTreeNode subNode)
        {
            subNodeList.Add(subNode);
        }

        public bool HasSubNode()
        {
            if (subNodeList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void InitSubNode()
        {
            subNodeList = new List<FileTreeNode>(); 
        }

    }
}