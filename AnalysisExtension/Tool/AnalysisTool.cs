using AnalysisExtension.Tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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

        //--------
        public void SetAnalysisMode(Analysis analysis)
        {
            analysisMode = analysis;
        }

        public Analysis GetAnalysisMode()
        {
            return analysisMode;
        }

        public List<ICodeBlock>[][] AnalysisMethod(BackgroundWorker backgroundWorker)
        {
            backgroundWorker.ReportProgress(0);
            analysisMode.AnalysisMethod();
            backgroundWorker.ReportProgress(50);
            List<ICodeBlock>[][] result =// MergeBlock(backgroundWorker);
                new List<ICodeBlock>[fileLoader.FILE_NUMBER][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>[2];
                // result[i][0] = new List<ICodeBlock>();
                List<ICodeBlock> beforeCodeBlockList = finalBeforeBlockList[i];
                List<ICodeBlock> afterCodeBlockList = finalAfterBlockList[i];
                result[i][0] = beforeCodeBlockList;
                result[i][1] = afterCodeBlockList;
            }
            backgroundWorker.ReportProgress(100);

            return result;
        }


        private List<ICodeBlock>[][] MergeBlock(BackgroundWorker backgroundWorker)
        {//merge those blocks that not match rule to a block
            List<ICodeBlock>[][] result = new List<ICodeBlock>[fileLoader.FILE_NUMBER][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>[2];
               // result[i][0] = new List<ICodeBlock>();
                List<ICodeBlock> beforeCodeBlockList = finalBeforeBlockList[i];
                List<ICodeBlock> afterCodeBlockList = finalAfterBlockList[i];
                result[i][0] = beforeCodeBlockList;
                result[i][1] = afterCodeBlockList;
            }
            
            float completePersent = 0;
            /*  for (int fileIndex = 0; fileIndex < finalBeforeBlockList.Length; fileIndex++)
              {
                  List<ICodeBlock> codeBlockList = finalBeforeBlockList[fileIndex];
                  int mergeStartIndex = -1;
                  int mergeRange = 0;
                  int i = 0;
                  while (i < codeBlockList.Count)
                  {
                      if (codeBlockList[i].IsMatchRule)
                      {
                          if (mergeStartIndex != -1)
                          {
                              MergeBeforeBlockFromRange(mergeStartIndex, mergeRange, fileIndex, codeBlockList, result[fileIndex][0]);

                              //continue check the block after mergeBlock(will add to next index by 'i++') , reset mergeStartIndex
                              i = mergeStartIndex;
                              mergeStartIndex = -1;
                              mergeRange = 0;
                          }
                          else
                          {
                              result[fileIndex][0].Add(codeBlockList[i]);
                          }
                      }
                      else if (i == codeBlockList.Count - 1 && mergeStartIndex != -1)
                      {
                          //from mergeStartIndex to final index need to merge
                          mergeRange++;
                          MergeBeforeBlockFromRange(mergeStartIndex, mergeRange, fileIndex, codeBlockList, result[fileIndex][0]);

                          break;
                      }
                      else
                      {//not match rule
                          if (mergeStartIndex == -1)
                          {
                              mergeStartIndex = i;
                          }
                          else
                          {
                              mergeRange++;
                          }
                      }
                      i++;
                  }

                  //refresh progress bar                
                  completePersent = ((float)(fileIndex + 1) / finalBeforeBlockList.Length) * 50 + 50;
                  if (completePersent <= 100)
                  {
                      backgroundWorker.ReportProgress((int)completePersent);
                  }
              }*/
            return result;
        }

        private void MergeBeforeBlockFromRange(int mergeStartIndex, int mergeRange, int fileIndex, List<ICodeBlock> orgCodeBlockList, List<ICodeBlock> resultCodeBlock)
        {
            string mergeText = "";

            for (int j = 0; j < mergeRange; j++)
            {
                int index = mergeStartIndex + j;
                mergeText += orgCodeBlockList[index].Content + "\n";
            }

            CodeBlock mergeBlock = new CodeBlock(mergeText);
            resultCodeBlock.Add(mergeBlock);
        }
    }
}
