using System.Collections.Generic;
using System.IO;

namespace AnalysisExtension.Model
{
    public class Analysis
    {
        public string Name { get; set; }
        public bool IsChoose { get; set; }
        public List<string> Type { get; set; }

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

    /*    public List<CodeBlock> GetBeforeCode(int fileIndex)
        {
            return codeListBefore[fileIndex];
        }

        public List<CodeBlock> GetAfterCode(int fileIndex)
        {
            return codeListAfter[fileIndex];
        }*/

        

        /*private void InitListValueInArray(List<CodeBlock>[] list)
        {
            for(int i = 0; i < list.Length; i++)
            {
                list[i] = new List<CodeBlock>();
            }
        }*/

        public virtual void AnalysisMethod()
        {
        }
    }
}
