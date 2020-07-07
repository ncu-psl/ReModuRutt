using AnalysisExtension.Tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AnalysisExtension.Model
{
    public class AnalysisManager 
    {
        private static AnalysisManager codeBlockTool = null;
        private static List<ICodeBlock>[] finalBeforeBlockList = null;
        private static List<ICodeBlock>[] finalAfterBlockList = null;

        private static Analysis analysisMode = null;
        private static FileLoader fileLoader = FileLoader.GetInstance();

        private AnalysisManager()
        {
            InitBlockList();
        }

        public static AnalysisManager GetInstance()
        {
            if (codeBlockTool == null)
            {
                codeBlockTool = new AnalysisManager();
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
                finalBeforeBlockList[i] = new List<ICodeBlock>();
                finalAfterBlockList[i] = new List<ICodeBlock>();
                foreach (ICodeBlock codeBlock in list[i][0])
                {
                    if (codeBlock is ParameterBlock)
                    {
                        ParameterBlock parameterBlock = new ParameterBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as ParameterBlock).ParaListIndex);
                        parameterBlock.IsMatchRule = codeBlock.IsMatchRule;
                        parameterBlock.MatchRule = codeBlock.MatchRule;
                        finalBeforeBlockList[i].Add(parameterBlock);
                    }
                    else if (codeBlock is IncludeBlock)
                    {
                        int includeBlockId = (codeBlock as IncludeBlock).IncludeBlockListIndex;
                        int compareRuleId = (codeBlock as IncludeBlock).CompareRuleId;
                        int fromRuleSetId = (codeBlock as IncludeBlock).FromRuleSetId;
                        IncludeBlock includeBlock = new IncludeBlock(codeBlock.Content, codeBlock.BlockId, includeBlockId, compareRuleId, fromRuleSetId);

                        includeBlock.IsMatchRule = codeBlock.IsMatchRule;
                        includeBlock.MatchRule = codeBlock.MatchRule;
                        finalBeforeBlockList[i].Add(includeBlock);
                    }
                    else if (codeBlock is CodeBlock)
                    {
                        CodeBlock beforeBlock = new CodeBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as CodeBlock).BlockListIndex);
                        beforeBlock.IsMatchRule = codeBlock.IsMatchRule;
                        beforeBlock.MatchRule = codeBlock.MatchRule;
                        finalBeforeBlockList[i].Add(beforeBlock);
                    }
                    else
                    {
                        NormalBlock beforeBlock = new NormalBlock(codeBlock.Content, codeBlock.BlockId);
                        beforeBlock.IsMatchRule = codeBlock.IsMatchRule;
                        beforeBlock.MatchRule = codeBlock.MatchRule;
                        finalBeforeBlockList[i].Add(beforeBlock);
                    }
                }

                foreach (ICodeBlock codeBlock in list[i][1])
                {
                    if (codeBlock is ParameterBlock)
                    {
                        ParameterBlock parameterBlock = new ParameterBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as ParameterBlock).ParaListIndex);
                        parameterBlock.IsMatchRule = codeBlock.IsMatchRule;
                        parameterBlock.MatchRule = codeBlock.MatchRule;
                        finalAfterBlockList[i].Add(parameterBlock);
                    }
                    else if (codeBlock is IncludeBlock)
                    {
                        int includeBlockId = (codeBlock as IncludeBlock).IncludeBlockListIndex;
                        int compareRuleId = (codeBlock as IncludeBlock).CompareRuleId;
                        int fromRuleSetId = (codeBlock as IncludeBlock).FromRuleSetId;
                        IncludeBlock includeBlock = new IncludeBlock(codeBlock.Content, codeBlock.BlockId, includeBlockId, compareRuleId, fromRuleSetId);
                        includeBlock.IsMatchRule = codeBlock.IsMatchRule;
                        includeBlock.MatchRule = codeBlock.MatchRule;
                        finalAfterBlockList[i].Add(includeBlock);
                    }
                    else if (codeBlock is CodeBlock)
                    {
                        CodeBlock beforeBlock = new CodeBlock(codeBlock.Content, codeBlock.BlockId, (codeBlock as CodeBlock).BlockListIndex);

                        beforeBlock.IsMatchRule = codeBlock.IsMatchRule;
                        beforeBlock.MatchRule = codeBlock.MatchRule;
                        finalAfterBlockList[i].Add(beforeBlock);

                    }
                    else
                    {
                        NormalBlock beforeBlock = new NormalBlock(codeBlock.Content, codeBlock.BlockId);
                        beforeBlock.IsMatchRule = codeBlock.IsMatchRule;
                        beforeBlock.MatchRule = codeBlock.MatchRule;
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
        public void RemoveRangeFromAfterList(int fileIndex, int startIndex, int removeLen)
        {
            finalAfterBlockList[fileIndex].RemoveRange(startIndex, removeLen);
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
        }

        //-----
        public List<ICodeBlock>[][] AnalysisMethod(BackgroundWorker backgroundWorker)
        {
            backgroundWorker.ReportProgress(0);
            analysisMode.AnalysisMethod(backgroundWorker);
            ExpandAllList();
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

        public bool AnalysisSingleRule(RuleBlock ruleBlock,List<ICodeBlock> content,int fileIndex,int startIndex,int len)
        {
            string contentString = StaticValue.GetAllContent(content);
            int afterStartIndex = -1;
            int afterInseretIndex = -1;

            List<ICodeBlock>[] result = analysisMode.CompareToSingleRule(ruleBlock, content);
            if(result == null)
            {
                return false;
            }

            //find text in before 
            RemoveRangeFromBeforeList(fileIndex, startIndex, len);
            //find text in after
            string afterContent = StaticValue.GetAllContent(finalAfterBlockList[fileIndex]);            
            ICodeBlock lastBlock = null;
            foreach (ICodeBlock codeBlock in finalAfterBlockList[fileIndex].ToArray())
            {
                if (lastBlock != null && /*codeBlock is NormalBlock && lastBlock is NormalBlock && */codeBlock.BlockId == lastBlock.BlockId)
                {
                    lastBlock.Content += codeBlock.Content;
                    int i = finalAfterBlockList[fileIndex].IndexOf(codeBlock);
                    RemoveFromAfterList(fileIndex, i);
                }
                else if (codeBlock.IsMatchRule && codeBlock is CodeBlock)
                {
                    if ((codeBlock as CodeBlock).AfterList.Count > 0)
                    {
                        int i = finalAfterBlockList[fileIndex].IndexOf(codeBlock);
                        RemoveFromAfterList(fileIndex, i);
                        finalAfterBlockList[fileIndex].InsertRange(i, (codeBlock as CodeBlock).ExpandAfterList());
                    }
                    lastBlock = codeBlock;
                }
                else
                {
                    lastBlock = codeBlock;
                }
            }
            foreach (ICodeBlock codeBlock in finalAfterBlockList[fileIndex].ToArray())
            {
                if (codeBlock.BlockId == content[0].BlockId)
                {
                    string compare = contentString;
                    int matchIndex = -1;
                    int matchLen = -1;

                    if (codeBlock is CodeBlock)
                    {
                        contentString = Regex.Replace(Regex.Escape(contentString), @"\\t", @"[\t]*");
                        Match match = Regex.Match(codeBlock.Content, contentString);
                        if (match.Success)
                        {
                            matchIndex = match.Index;
                            matchLen = match.Length;
                        }
                    }
                    else
                    {
                        if (codeBlock.Content.Contains(contentString))
                        {
                            matchIndex = codeBlock.Content.IndexOf(contentString);
                            matchLen = contentString.Length;
                        }
                    }

                    if (matchLen > -1 && matchIndex > -1)
                    {
                        afterStartIndex = finalAfterBlockList[fileIndex].IndexOf(codeBlock);
                        NormalBlock front = new NormalBlock(codeBlock.Content.Substring(0, matchIndex));
                        front.BlockId = codeBlock.BlockId;

                        NormalBlock back = new NormalBlock(codeBlock.Content.Substring(matchIndex + matchLen));
                        back.BlockId = codeBlock.BlockId;

                        finalAfterBlockList[fileIndex].Remove(codeBlock);
                        finalAfterBlockList[fileIndex].Insert(afterStartIndex, back);
                        finalAfterBlockList[fileIndex].Insert(afterStartIndex, front);
                        afterInseretIndex = finalAfterBlockList[fileIndex].IndexOf(back);
                        break;
                    }                    
                }
            }

            //insert result
            finalBeforeBlockList[fileIndex].InsertRange(startIndex,result[0]);
            finalAfterBlockList[fileIndex].InsertRange(afterInseretIndex, result[1]);

            //expand
            ExpandAllList();
            List<ICodeBlock>[][] finalResult = new List<ICodeBlock>[fileLoader.FILE_NUMBER][];
            for (int i = 0; i < finalResult.Length; i++)
            {
                finalResult[i] = new List<ICodeBlock>[2];
                finalResult[i][0] = SpiltByLine(finalBeforeBlockList[i]);
                finalResult[i][1] = SpiltByLine(finalAfterBlockList[i]);
            }
            SetFinalList(finalResult);
            return true;
        }

        private void ExpandAllList()
        {
            for (int i = 0; i < fileLoader.FILE_NUMBER; i++)
            {
                ICodeBlock lastBlock = null;
                foreach (ICodeBlock codeBlock in finalBeforeBlockList[i].ToArray())
                {
                    if (codeBlock.IsMatchRule && codeBlock is CodeBlock)
                    {
                        if ((codeBlock as CodeBlock).BeforeList.Count > 0)
                        {
                            int index = finalBeforeBlockList[i].IndexOf(codeBlock);
                            RemoveFromBeforeList(i, index);
                            finalBeforeBlockList[i].InsertRange(index, (codeBlock as CodeBlock).ExpandBeforeList());
                        }
                        lastBlock = codeBlock;
                    }
                    else if (lastBlock != null && codeBlock is NormalBlock && lastBlock is NormalBlock && codeBlock.BlockId == lastBlock.BlockId && codeBlock.MatchRule == lastBlock.MatchRule
                        && codeBlock.IsMatchRule == lastBlock.IsMatchRule)
                    {
                        lastBlock.Content += codeBlock.Content;
                        int index = finalBeforeBlockList[i].IndexOf(codeBlock);
                        RemoveFromBeforeList(i, index);
                    }
                    else
                    {
                        lastBlock = codeBlock;
                    }
                }

                lastBlock = null;
                foreach (ICodeBlock codeBlock in finalAfterBlockList[i].ToArray())
                {
                    if (codeBlock.IsMatchRule && codeBlock is CodeBlock)
                    {
                        if ((codeBlock as CodeBlock).AfterList.Count > 0)
                        {
                            int index = finalAfterBlockList[i].IndexOf(codeBlock);
                            RemoveFromAfterList(i, index);
                            finalAfterBlockList[i].InsertRange(index, (codeBlock as CodeBlock).ExpandAfterList());
                        }
                        lastBlock = codeBlock;
                    }
                    else if (lastBlock != null && codeBlock is NormalBlock && lastBlock is NormalBlock && codeBlock.BlockId == lastBlock.BlockId && codeBlock.MatchRule == lastBlock.MatchRule
                        && codeBlock.IsMatchRule == lastBlock.IsMatchRule)
                    {
                        lastBlock.Content += codeBlock.Content;
                        int index = finalAfterBlockList[i].IndexOf(codeBlock);
                        RemoveFromAfterList(i, index);
                    }
                    else
                    {
                        lastBlock = codeBlock;
                    }
                }
            }
        }

        private List<ICodeBlock> SpiltByLine(List<ICodeBlock> codeBlockList)
        {
            List<ICodeBlock> result = new List<ICodeBlock>();

            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                if (codeBlock is ParameterBlock)
                {
                    result.Add(codeBlock.GetCopy());
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

                            if (codeBlock is CodeBlock)
                            {
                                CodeBlock copyBlock = codeBlock.GetCopy() as CodeBlock;
                                copyBlock.Content = codeBlock.Content.Substring(0, match.Index);
                                result.Add(copyBlock);
                                CodeBlock line = codeBlock.GetCopy() as CodeBlock;
                                line.Content = match.Value;
                                result.Add(line);
                            }
                            else if (codeBlock is IncludeBlock)
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
