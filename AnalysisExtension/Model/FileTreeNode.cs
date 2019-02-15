
namespace AnalysisExtension
{
    public class FileTreeNode
    {
        public FileTreeNode(string name,string path)
        {
            this.Name = name;
            this.Path = path;
            this.IsChoose = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsChoose { get; set; }
    }
}

//https://docs.microsoft.com/zh-tw/dotnet/framework/wpf/data/data-templating-overview
//https://blog.csdn.net/yl2isoft/article/details/38712449