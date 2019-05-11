using System.Collections.Generic;
using System.IO;

namespace AnalysisExtension.Model
{
    public class Analysis
    {
        public string Name { get; set; }
        public bool IsChoose { get; set; }
        public List<string> Type { get; set; }

        string[] fileList = null;
        List<CodeBlock>[] codeListBefore = null;// new List<CodeBlock>();
        List<CodeBlock>[] codeListAfter = null;// new List<CodeBlock>();

        public Analysis(string name)
        {
            this.Name = name;
            this.IsChoose = false;
            Type = new List<string>();
        }

        public Analysis(string name,string type)
        {
            this.Name = "name";
            this.IsChoose = false;
            Type = new List<string>();
            Type.Add(type);
        }

        public List<CodeBlock> GetBeforeCode(int fileIndex)
        {
            return codeListBefore[fileIndex];
        }

        public List<CodeBlock> GetAfterCode(int fileIndex)
        {
            return codeListAfter[fileIndex];
        }

        public string[] GetFileList()
        {
            return fileList;
        }

        public void ReadFile(List<FileTreeNode> list)
        {
            int fileCount = list.Count;
            this.fileList = new string[fileCount];
            codeListBefore = new List<CodeBlock>[fileCount];
            codeListAfter = new List<CodeBlock>[fileCount];
            InitListValueInArray(codeListBefore);
            InitListValueInArray(codeListAfter);

            for (int i = 0; i < fileCount; i++)
            {
                /*fake data
                 * for (int j = 0; j < 10; j++)
                {
                    CodeBlock before = new CodeBlock();
                    CodeBlock after = new CodeBlock();

                    before.Content = "code before" + "\n" + "code Before" + j;
                    before.BlockId = j;
                    after.Content = "code after" + "\n" + "code After" + j;
                    after.BlockId = j % 5;

                    codeListBefore[i].Add(before);
                    codeListAfter[i].Add(after);
                }*/
                fileList[i] = list[i].Path;

                 string content = File.ReadAllText(fileList[i]);

                 CodeBlock codeBlock = new CodeBlock(content,i);
                 codeListBefore[i].Add(codeBlock);
                 codeListAfter[i].Add(codeBlock);
            }
        }

        private void InitListValueInArray(List<CodeBlock>[] list)
        {
            for(int i = 0; i < list.Length; i++)
            {
                list[i] = new List<CodeBlock>();
            }
        }

        public virtual void AnalysisMethod()
        {
        }
    }
}
