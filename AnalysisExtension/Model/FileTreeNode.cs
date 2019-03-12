
namespace AnalysisExtension
{
    public class FileTreeNode
    {
        public FileTreeNode(string name,string path)
        {
            this.Name = name;
            this.Path = path;
            this.IsChoose = false;
            this.Type = GetFileType();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsChoose { get; set; }
        public string Type { get; set; }

        private string GetFileType()
        {
            string rtType = null;
            string[] split = Name.Split('.');

            rtType = split[split.Length - 1];

            return rtType;
        }
    }
}