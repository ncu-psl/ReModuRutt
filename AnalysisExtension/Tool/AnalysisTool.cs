using AnalysisExtension.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisExtension.Model
{
    public class AnalysisTool
    {
        private static AnalysisTool codeBlockTool = null;
        private static List<ICodeBlock>[] finalBeforeBlockList = null;
        private static List<ICodeBlock>[] finalAfterBlockList = null;
        private static FileLoader fileLoader = FileLoader.GetInstance();

        private AnalysisTool()
        {
            finalBeforeBlockList = new List<ICodeBlock>[fileLoader.FILE_NUMBER];
            finalAfterBlockList = new List<ICodeBlock>[fileLoader.FILE_NUMBER];
            InitListValueInArray(finalBeforeBlockList);
            InitListValueInArray(finalAfterBlockList);
        }

        public static AnalysisTool GetInstance()
        {
            if (codeBlockTool == null)
            {
                codeBlockTool = new AnalysisTool();
            }

            return codeBlockTool;
        }

        //-----list method-----
        public void AddIntoBeforeList(ICodeBlock codeBlock,int fileIndex)
        {
            finalBeforeBlockList[fileIndex].Add(codeBlock);
        }

        public void AddIntoAfterList(ICodeBlock codeBlock,int fileIndex)
        {
            finalAfterBlockList[fileIndex].Add(codeBlock);
        }

        public List<ICodeBlock>[] GetFinalBeforeBlockList()
        {
            return finalBeforeBlockList;
        }

        public List<ICodeBlock>[] GetFinalAfterBlockList()
        {
            return finalAfterBlockList;
        }

        public List<ICodeBlock> GetFinalBeforeBlockList(int fileIndex)
        {
            return finalBeforeBlockList[fileIndex];
        }

        public List<ICodeBlock> GetFinalAfterBlockList(int fileIndex)
        {
            return finalAfterBlockList[fileIndex];
        }

        public void InitListValueInArray(List<ICodeBlock>[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = new List<ICodeBlock>();
            }
        }

        //-----tool method-----
        /*     public int CalculateBlockLayer(string startToken, string endToken, ICodeBlock codeBlock, int layerIdNow)
             {
                 int layerId = -1;
                 List<>

                 return layerId;
             }*/

        //-----read file-----
        public void AddFileIntoCodeBlock()
        {
            string[] list = fileLoader.GetFileList();

            for (int i = 0; i < fileLoader.FILE_NUMBER; i++)
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

                string content = File.ReadAllText(list[i]);

                //TODO : need to split codeBlock into before rule's block
                CodeBlock codeBlock = new CodeBlock(content);

                AddIntoBeforeList(codeBlock, i);
                AddIntoAfterList(codeBlock, i);
            }
        }

    }
}
