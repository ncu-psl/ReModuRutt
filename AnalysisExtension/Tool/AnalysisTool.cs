﻿using AnalysisExtension.Tool;
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
                    if (codeBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                    {
                        ParameterBlock parameterBlock = new ParameterBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as ParameterBlock).ParaListIndex);
                        finalBeforeBlockList[i].Add(parameterBlock);
                    }
                    else if (codeBlock.TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                    {
                        int includeBlockId = (codeBlock as IncludeBlock).IncludeBlockListIndex;
                        int compareRuleId = (codeBlock as IncludeBlock).CompareRuleId;
                        int fromRuleSetId = (codeBlock as IncludeBlock).FromRuleSetId;
                        IncludeBlock includeBlock = new IncludeBlock(codeBlock.Content, codeBlock.BlockId, includeBlockId, compareRuleId, fromRuleSetId);
                        finalBeforeBlockList[i].Add(includeBlock);
                    }
                    else if (codeBlock.TypeName.Equals(StaticValue.CODE_BLOCK_TYPE_NAME))
                    {
                        CodeBlock beforeBlock = new CodeBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as CodeBlock).BlockListIndex);
                        finalBeforeBlockList[i].Add(beforeBlock);

                    }
                    else
                    {
                        NormalBlock beforeBlock = new NormalBlock(codeBlock.Content, codeBlock.BlockId);
                        finalBeforeBlockList[i].Add(beforeBlock);
                    }
                }

                foreach (ICodeBlock codeBlock in list[i][1])
                {
                    if (codeBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                    {
                        ParameterBlock parameterBlock = new ParameterBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as ParameterBlock).ParaListIndex);
                        finalAfterBlockList[i].Add(parameterBlock);
                    }
                    else if (codeBlock.TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                    {
                        int includeBlockId = (codeBlock as IncludeBlock).IncludeBlockListIndex;
                        int compareRuleId = (codeBlock as IncludeBlock).CompareRuleId;
                        int fromRuleSetId = (codeBlock as IncludeBlock).FromRuleSetId;
                        IncludeBlock includeBlock = new IncludeBlock(codeBlock.Content, codeBlock.BlockId, includeBlockId, compareRuleId, fromRuleSetId);
                        finalAfterBlockList[i].Add(includeBlock);
                    }
                    else if (codeBlock.TypeName.Equals(StaticValue.CODE_BLOCK_TYPE_NAME))
                    {
                        CodeBlock beforeBlock = new CodeBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as CodeBlock).BlockListIndex);
                        finalAfterBlockList[i].Add(beforeBlock);

                    }
                    else
                    {
                        NormalBlock beforeBlock = new NormalBlock(codeBlock.Content, codeBlock.BlockId);
                        finalAfterBlockList[i].Add(beforeBlock);
                    }
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
            List<ICodeBlock>[][] result = new List<ICodeBlock>[fileLoader.FILE_NUMBER][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>[2];
                result[i][0] = SpiltByLine(finalBeforeBlockList[i]); 
                result[i][1] = SpiltByLine(finalAfterBlockList[i]); 
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

                        if (match.Success)
                        {
                            int index = match.Index + match.Length;

                            if (codeBlock.TypeName.Equals(StaticValue.CODE_BLOCK_TYPE_NAME))
                            {
                                CodeBlock copyBlock = codeBlock.GetCopy() as CodeBlock;
                                copyBlock.Content = codeBlock.Content.Substring(0, match.Index);
                                result.Add(copyBlock);
                                CodeBlock line = codeBlock.GetCopy() as CodeBlock;
                                line.Content = match.Value;
                                result.Add(line);
                            }
                            else if (codeBlock.TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                            {
                                IncludeBlock copyBlock = codeBlock.GetCopy() as IncludeBlock;
                                copyBlock.Content = codeBlock.Content.Substring(0, match.Index);
                                result.Add(copyBlock);
                                IncludeBlock line = codeBlock.GetCopy() as IncludeBlock;
                                line.Content = match.Value;
                                result.Add(line);
                            }
                            else 
                            {
                                result.Add(new NormalBlock(codeBlock.Content.Substring(0, match.Index), blockId));
                                result.Add(new NormalBlock(match.Value, blockId));
                            }
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

    }
}
