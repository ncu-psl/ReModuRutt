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
            InitBlockList();
        }

        public static AnalysisTool GetInstance()
        {
            if (codeBlockTool == null)
            {
                codeBlockTool = new AnalysisTool();
            }

            return codeBlockTool;
        }

        public void InitBlockList()
        {
            finalBeforeBlockList = new List<ICodeBlock>[fileLoader.FILE_NUMBER];
            finalAfterBlockList = new List<ICodeBlock>[fileLoader.FILE_NUMBER];
            InitListValueInArray(finalBeforeBlockList);
            InitListValueInArray(finalAfterBlockList);
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
    }
}
