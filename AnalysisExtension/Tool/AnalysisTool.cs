using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AnalysisExtension.Model
{
    public class AnalysisTool 
    {
        private static AnalysisTool codeBlockTool = null;
        private static List<ICodeBlock>[] finalBeforeBlockList = null;
        private static List<ICodeBlock>[] finalAfterBlockList = null;

        private static Analysis analysisMode = null;
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

        //--------init analysis mode 
        public void SetAnalysisMode(Analysis analysis)
        {
            analysisMode = analysis;
        }

        public void LoadRuleList()
        {
            analysisMode.RuleList = fileLoader.LoadRuleList(analysisMode.RuleFolderPath);
        }

        public Analysis GetAnalysisMode()
        {
            return analysisMode;
        }

        //-----list method-----
        public void SetFinalList(List<ICodeBlock>[][] list)
        {
            InitListValueInArray(finalBeforeBlockList);
            InitListValueInArray(finalAfterBlockList);
            for (int i = 0; i < list.Length; i++)
            {
                foreach (ICodeBlock codeBlock in list[i][0])
                {
                    ICodeBlock beforeBlock = new CodeBlock(codeBlock.Content,codeBlock.BlockId,-1);
                    finalBeforeBlockList[i].Add(beforeBlock);
                }

                foreach (ICodeBlock codeBlock in list[i][1])
                {
                    ICodeBlock afterBlock = new CodeBlock(codeBlock.Content, codeBlock.BlockId,-1);
                    finalAfterBlockList[i].Add(afterBlock);
                }
            }            
        }

        public void AddIntoBeforeList(ICodeBlock codeBlock,int fileIndex)
        {
            finalBeforeBlockList[fileIndex].Add(codeBlock);                       
        }

        public void AddIntoAfterList(ICodeBlock codeBlock,int fileIndex)
        {
            finalAfterBlockList[fileIndex].Add(codeBlock);
        }

        public void AddListIntoBeforeList(List<ICodeBlock> codeBlockList, int fileIndex)
        {
            finalBeforeBlockList[fileIndex].AddRange(codeBlockList);
        }

        public void AddListIntoAfterList(List<ICodeBlock> codeBlockList, int fileIndex)
        {
            finalAfterBlockList[fileIndex].AddRange(codeBlockList);
        }

        public void InsertIntoBeforeList(ICodeBlock codeBlock, int fileIndex, int insertIndex)
        {
            finalBeforeBlockList[fileIndex].Insert(insertIndex,codeBlock);            
        }

        public void InsertIntoBeforeList(List<ICodeBlock> codeBlockList, int fileIndex, int insertIndex)
        {
            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                InsertIntoBeforeList(codeBlock, fileIndex, insertIndex);
                insertIndex++;
            }            
        }

        public void InsertIntoAfterList(ICodeBlock codeBlock, int fileIndex, int insertIndex)
        {            
            finalAfterBlockList[fileIndex].Insert(insertIndex, codeBlock);            
        }

        public void InsertIntoAfterList(List<ICodeBlock> codeBlockList, int fileIndex, int insertIndex)
        {
            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                InsertIntoAfterList(codeBlock, fileIndex, insertIndex);
                insertIndex++;
            }
        }

        public void RemoveFromBeforeList(int fileIndex, int index)
        {
            finalBeforeBlockList[fileIndex].RemoveAt(index);                      
        }

        public void RemoveRangeFromBeforeList(int fileIndex, int startIndex,int removeLen)
        {
            finalBeforeBlockList[fileIndex].RemoveRange(startIndex, removeLen);            
        }

        public void RemoveFromAfterList(int fileIndex, int index)
        {
            finalAfterBlockList[fileIndex].RemoveAt(index);            
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

        public void RefreshNotMatchBlock(int fileCount,List<ICodeBlock>[] newBlockList)
        {
            finalBeforeBlockList[fileCount] = newBlockList[0];
            finalAfterBlockList[fileCount] = newBlockList[1];
            SetBeforeNotMatchBlock( fileCount, newBlockList[0]);
            SetAfterNotMatchBlock( fileCount, newBlockList[1]);
        }

        private void SetBeforeNotMatchBlock(int fileCount, List<ICodeBlock> newBlockList)
        {
            int blockCount = 0;
            int beforeCont = 0;
            ICodeBlock[] beforeResult = finalBeforeBlockList[fileCount].ToArray();
            while(blockCount < newBlockList.Count && beforeCont < beforeResult.Length)
            {
                ICodeBlock codeBlock = beforeResult[beforeCont];
                if (codeBlock.BlockId != newBlockList[blockCount].BlockId)
                {
                    int index = finalBeforeBlockList[fileCount].IndexOf(codeBlock);
                    RemoveFromBeforeList(fileCount, index);
                    InsertIntoBeforeList(newBlockList[blockCount], fileCount, index);
                }
                blockCount++;
                beforeCont++;
            }
        }

        private void SetAfterNotMatchBlock( int fileCount, List<ICodeBlock> newBlockList)
        {
            int blockCount = 0;
            int afterCount = 0;
            ICodeBlock[] beforeResult = finalAfterBlockList[fileCount].ToArray();
            while (blockCount < newBlockList.Count && afterCount < beforeResult.Length)
            {
                ICodeBlock codeBlock = beforeResult[afterCount];
                if (codeBlock.BlockId != newBlockList[blockCount].BlockId)
                {
                    int index = finalBeforeBlockList[fileCount].IndexOf(codeBlock);
                    RemoveFromBeforeList(fileCount, index);
                    InsertIntoBeforeList(newBlockList[blockCount], fileCount, index);
                }
                blockCount++;
                afterCount++;
            }
        }


        //-----
        public List<ICodeBlock>[][] AnalysisMethod(BackgroundWorker backgroundWorker)
        {
            backgroundWorker.ReportProgress(0);
            analysisMode.AnalysisMethod(backgroundWorker);
            //RemoveEmptyBlock();
            List<ICodeBlock>[][] result = new List<ICodeBlock>[fileLoader.FILE_NUMBER][];
            finalBeforeBlockList = MergeBlock(finalBeforeBlockList);
            finalAfterBlockList = MergeBlock(finalAfterBlockList);

            /*new List<ICodeBlock>[fileLoader.FILE_NUMBER][];*/
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>[2];
                 List<ICodeBlock> beforeCodeBlockList = finalBeforeBlockList[i];
                 List<ICodeBlock> afterCodeBlockList = finalAfterBlockList[i];/**/
                result[i][0] = beforeCodeBlockList;
                result[i][1] = afterCodeBlockList;
            }
            backgroundWorker.ReportProgress(100);

            return result;
        }

        private List<ICodeBlock> SpiltByLine(List<ICodeBlock> codeBlockList)
        {
            List<ICodeBlock> result = new List<ICodeBlock>();

            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                if (codeBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                {
                    result.Add(codeBlock);
                }
                else
                {
                    while (codeBlock.Content.Length > 0)
                    {
                        Match match = Regex.Match(codeBlock.Content, "[\r\n]+");
                        int blockId = codeBlock.BlockId;
                        int blockListId = (codeBlock as CodeBlock).BlockListIndex;

                        if (match.Success)
                        {
                            int index = match.Index + match.Length;
                            result.Add(new CodeBlock(codeBlock.Content.Substring(0, index), blockId, blockListId));//add with \n\r
                            codeBlock.Content = codeBlock.Content.Substring(index);
                        }
                        else
                        {
                            result.Add(codeBlock);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private void RemoveEmptyBlock()
        {
            for (int fileIndex = 0; fileIndex <fileLoader.FILE_NUMBER; fileIndex++)
            {
                foreach (ICodeBlock codeBlock in finalBeforeBlockList[fileIndex].ToArray())
                {
                    if (!(codeBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME)) && !(codeBlock.Content.Contains("<block")))
                    {
                        Match match = Regex.Match(codeBlock.Content, @"[\S]");
                        if (!match.Success || codeBlock.Content.Length == 0)
                        {
                            finalBeforeBlockList[fileIndex].Remove(codeBlock);
                        }
                    }
                }

                foreach (ICodeBlock codeBlock in finalAfterBlockList[fileIndex].ToArray())
                {
                    if (!(codeBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME)) && !(codeBlock.Content.Contains("<block")))
                    {
                        Match match = Regex.Match(codeBlock.Content, @"[\S]");
                        if (!match.Success || codeBlock.Content.Length == 0)
                        {
                            finalBeforeBlockList[fileIndex].Remove(codeBlock);
                        }
                    }
                }
            }
        }
        private List<ICodeBlock>[] MergeBlock(List<ICodeBlock>[] finalBlockList)
        {
            List<ICodeBlock>[] result = new List<ICodeBlock>[fileLoader.FILE_NUMBER];
            for (int fileIndex = 0; fileIndex < fileLoader.FILE_NUMBER; fileIndex++)
            {
                result[fileIndex] = new List<ICodeBlock>();
                List<ICodeBlock> codeBlockList = finalBlockList[fileIndex];
                int mergeStartIndex = -1;
                int mergeRange = 0;
                int i = 0;
                while (i < codeBlockList.Count)
                {
                    if (codeBlockList[i].IsMatchRule)
                    {
                        if (mergeStartIndex != -1)
                        {
                            mergeRange = i - mergeStartIndex;

                            MergeBlockFromRange(mergeStartIndex, mergeRange, fileIndex, codeBlockList, result[fileIndex]);
                            result[fileIndex].Add(codeBlockList[i]);

                            mergeStartIndex = -1;
                            mergeRange = 0;
                        }
                        else
                        {
                            result[fileIndex].Add(codeBlockList[i]);
                        }
                    }
                    else if (i == codeBlockList.Count - 1 && mergeStartIndex != -1)
                    {
                        //from mergeStartIndex to final index need to merge
                        mergeRange = i - mergeStartIndex;
                        MergeBlockFromRange(mergeStartIndex, mergeRange, fileIndex, codeBlockList, result[fileIndex]);

                        break;
                    }
                    else
                    {//not match rule
                        if (mergeStartIndex == -1)
                        {
                            mergeStartIndex = i;
                        }
                    }
                    i++;
                }
            }
            return result;
        }

        private void MergeBlockFromRange(int mergeStartIndex, int mergeRange, int fileIndex, List<ICodeBlock> orgCodeBlockList, List<ICodeBlock> resultCodeBlock)
        {
            string mergeText = "";
            int blockId = -1;
            for (int j = 0; j < mergeRange; j++)
            {
                int index = mergeStartIndex + j;
                mergeText += orgCodeBlockList[index].Content;
                blockId = orgCodeBlockList[index].BlockId;
            }

            CodeBlock mergeBlock = new CodeBlock(mergeText,blockId,-1);
            resultCodeBlock.Add(mergeBlock);
        }
    }
}
