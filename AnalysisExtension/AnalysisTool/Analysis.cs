using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AnalysisExtension.AnalysisTool
{
    public class Analysis
    {
        public string Name { get; set; }
        public RuleSet AnalysisRuleSet { get; set; }
        public string RuleFolderPath { get; set; }
        public List<RuleBlock> RuleList { get; set; }

        AnalysisManager analysisTool = AnalysisManager.GetInstance();
        bool needCheck = false;
        
        public Analysis(RuleSet ruleSet)
        {
            AnalysisRuleSet = ruleSet;
            Name = AnalysisRuleSet.Name;
            LoadRulePath();
        }

        //-----set rule-----
        private void LoadRulePath()
        {
            string path = @"\" + Name;
            RuleFolderPath = StaticValue.GetRuleFolderPath() + path;
        }

        //-----analysis method-----
        public void AnalysisMethod(BackgroundWorker backgroundWorker)
        {
            string[] orgContentList = FileLoader.GetInstance().GetFileContent();

            int count = 0;
            foreach (RuleBlock ruleBlock in RuleList)
            {
                count++;
                for (int fileCount = 0; fileCount < FileLoader.GetInstance().GetFileContent().Length; fileCount++)
                {
                    ruleBlock.InitRuleSetting();
                    List<ICodeBlock>[] result = null;
                    List<ICodeBlock> analysisContent = EscapeTokenSet.SpiltByEscapeToken(orgContentList[fileCount]);

                    if (analysisTool.GetFinalBeforeBlockList(fileCount).Count > 0)
                    {
                        analysisContent = analysisTool.GetFinalBeforeBlockList(fileCount);
                    }

                    if (ruleBlock.IsPureRegex)
                    {
                        result = ComparePureRegex(ruleBlock, StaticValue.GetAllContent(analysisContent));
                    }
                    else
                    {
                        result = CompareToSingleRule(ruleBlock, analysisContent);//0 : beforeResult , 1 : afterResult
                        
                    }
                    if (result != null)
                    {
                        analysisTool.RefreshNotMatchBlock(fileCount, result);
                    }
                }
                backgroundWorker.ReportProgress((int)((float)count / RuleList.Count) * 80);
            }

            while (needCheck)
            {
               MatchNeedCheck();
            }
        }

        public void MatchNeedCheck()
        {
            foreach (RuleBlock ruleBlock in RuleList)
            {
                for (int fileCount = 0; fileCount < FileLoader.GetInstance().GetFileContent().Length; fileCount++)
                {
                    List<ICodeBlock> beforeCodeBlock = analysisTool.GetFinalBeforeBlockList()[fileCount];

                    int changeCount = 0;
                    List<ICodeBlock>[] result = null;
                    if (!ruleBlock.IsPureRegex)
                    {
                        result = CompareToSingleRule(ruleBlock, beforeCodeBlock);//0 : beforeResult , 1 : afterResult
                    }

                    if (result != null)
                    {
                        analysisTool.RefreshNotMatchBlock(fileCount, result);
                        changeCount++;
                    }

                    if (changeCount == 0)
                    {
                        needCheck = false;
                    }
                }
            }
        }

        public string ReplaceComment(string content,RuleBlock commentRule)
        {
            if (EscapeTokenSet.COMMENT_END_TOKEN.Equals("[\\s]+"))
            {
                content = Regex.Replace(content, EscapeTokenSet.COMMENT_START_TOKEN,commentRule.AfterRuleSliceList[0].Content);
            }
            return content;
        }

        //-----------------pure regex compare-----------------
        public List<ICodeBlock>[] ComparePureRegex(RuleBlock ruleBlock, string orgText)
        {
            List<ICodeBlock>[] finalResult = new List<ICodeBlock>[2];
            finalResult[0] = new List<ICodeBlock>();
            finalResult[1] = new List<ICodeBlock>();
            string matchString = orgText;
            int index = 0;

            while (Regex.IsMatch(orgText, ruleBlock.BeforeRuleSliceList[0].Content))
            {
                Match match = Regex.Match(orgText, ruleBlock.BeforeRuleSliceList[0].Content);
                matchString = match.Value;
                index = match.Index;
                if (index > 0)
                {
                    string frontContent = orgText.Substring(0, index);
                    NormalBlock front = new NormalBlock(frontContent);
                    finalResult[0].Add(front);
                    finalResult[1].Add(front.GetCopy());
                }

                NormalBlock before = new NormalBlock(matchString);
                before.IsMatchRule = true;
                before.MatchRule = ruleBlock;

                string afterContent = Regex.Replace(matchString, ruleBlock.BeforeRuleSliceList[0].Content, ruleBlock.AfterRuleSliceList[0].Content);
                int groupCount = GetGroupCountInPattern(ruleBlock.AfterRuleSliceList[0].Content);
                for (int i = 1; i <= groupCount; i++)
                {
                    afterContent = Regex.Replace(afterContent, "\\\\" + i, match.Groups[i].Value);
                }
                NormalBlock after = new NormalBlock(afterContent, before.BlockId);
                after.IsMatchRule = true;
                after.MatchRule = ruleBlock;

                CodeBlock result = new CodeBlock();
                result.BeforeList.Add(before);
                result.AfterList.Add(after);
                result.BlockId = before.BlockId;
                result.IsMatchRule = true;
                result.MatchRule = ruleBlock;

                finalResult[0].Add(result);
                finalResult[1].Add(result);

                if (index + matchString.Length < orgText.Length)
                {
                    orgText = orgText.Substring(index + matchString.Length);
                }
            }

            if (orgText.Length > 0)
            {
                NormalBlock back = new NormalBlock(orgText);
              
                finalResult[0].Add(back);
                finalResult[1].Add(back.GetCopy());
            }

            return finalResult;
        }

        private int GetGroupCountInPattern(string pattern)
        {
            MatchCollection match = Regex.Matches(pattern, @"\\[0-9]");
            return match.Count;
        }

        //------------normal rule compare----------------
        public List<ICodeBlock>[] CompareToSingleRule(RuleBlock ruleBlock, List<ICodeBlock> orgBlockList)//if not match , return null
        {
            List<ICodeBlock> ruleSlice = ruleBlock.BeforeRuleSliceList;
            bool isMatch = true;
            List<ICodeBlock>[] finalResult = new List<ICodeBlock>[2];
            List<ICodeBlock>[] result = new List<ICodeBlock>[2];
            List<ICodeBlock> front = new List<ICodeBlock>();
            ScopeFinder scopeFinder = new ScopeFinder(ruleBlock);
            finalResult[0] = new List<ICodeBlock>();
            finalResult[1] = new List<ICodeBlock>();
            result[0] = new List<ICodeBlock>();
            result[1] = new List<ICodeBlock>();

            int endRuleIndex = ruleBlock.BeforeRuleSliceList.Count - 1;
            int ruleSliceIndex = 0;

            List<ICodeBlock> analysisContentList = orgBlockList;

            while (ruleSliceIndex <= endRuleIndex && StaticValue.GetAllContent(analysisContentList).Length > 0)
            {
                string startToken = null;
                string endToken = null;
                List<ICodeBlock> backList = new List<ICodeBlock>();

                //set start token and end token
                if (ruleSlice[ruleSliceIndex] is ParameterBlock || ruleSlice[ruleSliceIndex] is CodeBlock || ruleSlice[ruleSliceIndex] is IncludeBlock)
                {
                    List<ICodeBlock>[] scope = null;//0-front 1-startToken 2-para 3-endToken 4-back
                    string[] token = scopeFinder.GetStartTokenAndEndToken(ruleSlice, ruleSliceIndex);
                    startToken = token[0];
                    endToken = token[1];
                    //set analysis content
                    if (ruleSliceIndex > 0)
                    {
                        if (analysisContentList[0].MatchRule != null)
                        {
                            scope = new List<ICodeBlock>[5];
                            for (int i = 0; i < scope.Length; i++)
                            {
                                scope[i] = new List<ICodeBlock>();
                            }
                            bool find = false;
                            foreach (ICodeBlock codeBlock in analysisContentList)
                            {
                                if (codeBlock.MatchRule == null && !find)
                                {//start
                                    scope[1].Add(scope[0][scope[0].Count - 1]);
                                    scope[0].RemoveAt(scope[0].Count - 1);
                                    find = true;
                                }
                                else if (codeBlock.MatchRule != null && find)
                                {//end
                                    scope[3].Add(codeBlock);
                                    int index = analysisContentList.IndexOf(codeBlock) + 1;
                                    if (index < analysisContentList.Count)
                                    {
                                        scope[4].AddRange(analysisContentList.GetRange(index, analysisContentList.Count - index));
                                    }
                                    break;
                                }
                                else if (!find)
                                {
                                    scope[0].Add(codeBlock);
                                }

                                if (find)
                                {
                                    scope[2].Add(codeBlock);
                                }
                            }
                        }
                        else
                        {
                            if (startToken != null && (startToken.Equals("[\\n\\r]*") || startToken.Equals("[\\n\\r]+") || startToken.Equals("[\\s]*") || startToken.Equals("[\\s]+")))
                            {
                                int ruleCount = ruleSliceIndex - 2;
                                int count = result[0].Count - 2;
                                while (startToken != null && (startToken.Equals("[\\n\\r]*") || startToken.Equals("[\\n\\r]+") || startToken.Equals("[\\s]+") || startToken.Equals("[\\s]*")) && count >= 0)
                                {
                                    if (result[0][count].Content.Length > 0)
                                    {
                                        startToken = Regex.Escape(result[0][count].Content) + startToken;
                                        analysisContentList.Insert(0, new NormalBlock(result[0][count].Content));
                                        break;
                                    }
                                    count--;
                                }
                            }
                            else if (ruleSlice[ruleSliceIndex - 1] is ParameterBlock || ruleSlice[ruleSliceIndex - 1] is CodeBlock)
                            {
                                //if startToken is parameter or codeBlock, get content from result list
                                startToken = "[ ]+" + Regex.Escape(result[0][result[0].Count - 1].Content);
                                //add content into content need to analysis 
                                analysisContentList.Insert(0, new NormalBlock(" " + result[0][result[0].Count - 1].Content));
                            }
                            else
                            {
                                if (startToken != null)
                                {
                                    analysisContentList.Insert(0, new NormalBlock(result[0][result[0].Count - 1].Content));
                                }
                            }
                            scope = scopeFinder.FindScope(startToken, endToken, analysisContentList);
                        }
                    }

                    //------------------


                    //-----find-----

                    if (scope == null || (scope != null & StaticValue.HasContent(scope[0]) && ruleSliceIndex > 1))
                    {// the rule slice that not first rule slice need to follow front of content (at index 0) , if it has some text before , then not match
                       /* if (scope == null)
                        {*/
                            analysisContentList.RemoveAt(0);
                      //  }
                        if (ruleSliceIndex > 0 && HasTokenInList(analysisContentList, scopeFinder.StartRuleSlice))
                        {
                            foreach (ICodeBlock frontBlock in result[0])
                            {
                                if (front.Count == 0)
                                {
                                    ICodeBlock addBlock = new NormalBlock(frontBlock.Content);
                                    front.Add(addBlock);
                                }
                                else
                                {
                                    front[front.Count - 1].Content += frontBlock.Content;
                                }
                            }
                            result[0] = new List<ICodeBlock>();
                            //reset rule
                            ruleBlock.InitRuleSetting();
                            //change content
                            List<ICodeBlock>[] match = GetListSpiltByToken(analysisContentList, scopeFinder.StartRuleSlice);
                            front.AddRange(match[0]);
                            //reset this compare
                            match[1].AddRange(match[2]);
                            analysisContentList = match[1];
                            ruleSliceIndex = 0;
                            continue;
                        }
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else
                    {
                        //match
                        if (scope[0] != null && scope[0].Count > 0 && ruleSliceIndex == 0)
                        {
                            scope[0].AddRange(scope[1]);
                            front.AddRange(scope[0]);
                        }
                        scope[3].AddRange(scope[4]);
                        backList = scope[3];
                    }

                    if (ruleSlice[ruleSliceIndex] is IncludeBlock)
                    {//is include
                        IncludeBlock ruleInclude = ruleSlice[ruleSliceIndex] as IncludeBlock;
                        int includeListId = ruleInclude.IncludeBlockListIndex;
                        int blockId = ruleInclude.BlockId;
                        int compareRuleId = ruleInclude.CompareRuleId;
                        int fromRuleSetId = ruleInclude.FromRuleSetId;

                        string rulePath = RuleMetadata.GetInstance().GetRulePathById(fromRuleSetId, compareRuleId);
                        RuleBlock findRule = FileLoader.GetInstance().LoadSingleRuleByPath(rulePath);
                        List<ICodeBlock> newScopeList;

                        if (findRule.BeforeRuleSliceList.Count > 1)
                        {
                            List<ICodeBlock>[] scopeNeedFind = scopeFinder.FindScope(findRule.BeforeRuleSliceList[0].Content, findRule.BeforeRuleSliceList[findRule.BeforeRuleSliceList.Count - 1].Content, scope[2]);

                            if (scopeNeedFind == null || scopeNeedFind[0].Count > 0 && StaticValue.GetAllContent(scopeNeedFind[0]).Length > 0)
                            {
                                isMatch = false;
                                break;
                            }
                            else
                            {
                                newScopeList = scopeNeedFind[1];
                                newScopeList.AddRange(scopeNeedFind[2]);
                                newScopeList.AddRange(scopeNeedFind[3]);
                                backList.InsertRange(0, scopeNeedFind[4]);
                            }
                        }
                        else
                        {
                            List<ICodeBlock>[] scopeNeedFind = GetListSpiltByToken(scope[2], findRule.BeforeRuleSliceList[0].Content);
                            if (scopeNeedFind == null || scopeNeedFind[0].Count > 0 && StaticValue.GetAllContent(scopeNeedFind[0]).Length > 0)
                            {
                                isMatch = false;
                                break;
                            }
                            else
                            {
                                newScopeList = scopeNeedFind[1];
                                backList.InsertRange(0, scopeNeedFind[2]);
                            }
                        }

                        List<ICodeBlock>[] findResult = null;
                        if (!findRule.IsPureRegex)
                        {
                            result = CompareToSingleRule(ruleBlock, newScopeList);//0 : beforeResult , 1 : afterResult
                        }

                        if (findResult == null)
                        {
                            isMatch = false;
                            break;
                        }
                        else
                        {
                            IncludeBlock resultIncludeBlock = new IncludeBlock("", blockId, includeListId, compareRuleId, fromRuleSetId);
                            resultIncludeBlock.BeforeList = findResult[0];
                            resultIncludeBlock.AfterList = findResult[1];
                            resultIncludeBlock.IsMatchRule = true;

                            IncludeBlock findBlock = ruleBlock.GetIncludeBlockById(includeListId);
                            if (findBlock != null)
                            {
                                if (!ruleBlock.IsIncludeBlockSame(findBlock, resultIncludeBlock))
                                {//one or more result is not same
                                    if (endToken != null && HasTokenInList(analysisContentList, endToken))
                                    {// if still can find end token , watch back to find if there has block match or not

                                        List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                        front.AddRange(list[0]);
                                        analysisContentList = list[1];
                                        continue;
                                    }
                                    else
                                    {
                                        isMatch = false;
                                        break;
                                    }
                                }
                                else
                                {//is find and match
                                    result[0].AddRange(findBlock.BeforeList);
                                }
                            }
                            else
                            {//add include block
                                ruleBlock.AddIncludeBlock(resultIncludeBlock);
                                result[0].AddRange(resultIncludeBlock.BeforeList);
                            }
                        }
                    }
                    else if (ruleSlice[ruleSliceIndex] is ParameterBlock)
                    {//is parameter

                        int paraListId = (ruleSlice[ruleSliceIndex] as ParameterBlock).ParaListIndex;
                        int blockId = (ruleSlice[ruleSliceIndex] as ParameterBlock).BlockId;
                        string paraContent = StaticValue.GetAllContent(scope[2]);

                        if (Regex.Match(paraContent, @"[\s]+").Success)
                        {
                            analysisContentList.RemoveAt(0);
                            if (ruleSliceIndex > 0 && HasTokenInList(analysisContentList, scopeFinder.StartRuleSlice))
                            {
                                foreach (ICodeBlock frontBlock in result[0])
                                {
                                    if (front.Count == 0)
                                    {
                                        ICodeBlock addBlock = new NormalBlock(frontBlock.Content);
                                        front.Add(addBlock);
                                    }
                                    else
                                    {
                                        front[front.Count - 1].Content += frontBlock.Content;
                                    }
                                }

                                result[0] = new List<ICodeBlock>();
                                //reset rule
                                ruleBlock.InitRuleSetting();
                                //change content
                                List<ICodeBlock>[] match = GetListSpiltByToken(analysisContentList, scopeFinder.StartRuleSlice);
                                front.AddRange(match[0]);
                                //reset this compare
                                match[1].AddRange(match[2]);
                                analysisContentList = match[1];
                                ruleSliceIndex = 0;
                                continue;
                            }
                            else
                            {
                                isMatch = false;
                                break;
                            }
                        }

                        ParameterBlock parameter = new ParameterBlock(paraContent, blockId, paraListId);
                        parameter.IsMatchRule = true;

                        ParameterBlock findBlock = ruleBlock.GetParameterById(parameter.ParaListIndex);
                        if (findBlock != null)
                        {//has this parameter before
                            if (!parameter.Content.Equals(findBlock.Content))
                            {//if parameter is not same in parameter list, then this is not match. Add this into need check list.

                                if (HasTokenInList(analysisContentList, endToken) && ruleSliceIndex == 0)
                                {// if still can find end token , watch back to find if there has parameter match or not

                                    List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                    front.AddRange(list[0]);
                                    analysisContentList = list[1];
                                    continue;
                                }
                                else
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }
                        else
                        {//new parameter
                            //add parameter into parameter list
                            ruleBlock.AddParameter(parameter);
                        }
                        result[0].Add(parameter);
                    }
                    else if (ruleSlice[ruleSliceIndex] is CodeBlock)
                    {//is block next

                        int blockListIndex = (ruleSlice[ruleSliceIndex] as CodeBlock).BlockListIndex;
                        int blockId = (ruleSlice[ruleSliceIndex] as CodeBlock).BlockId;
                        List<ICodeBlock>[] subResult = new List<ICodeBlock>[2];
                        subResult[0] = new List<ICodeBlock>();
                        subResult[1] = new List<ICodeBlock>();

                        string blockContent = StaticValue.GetAllContent(scope[2]);
                        CodeBlock content = new CodeBlock(blockContent, blockId, blockListIndex);
                        content.IsMatchRule = true;

                        if (HasIsMatch(scope[2]))
                        {
                            foreach (ICodeBlock codeBlock in scope[2])
                            {
                                if (codeBlock.IsMatchRule)
                                {
                                    subResult[0].AddRange((codeBlock as CodeBlock).BeforeList);
                                    subResult[1].AddRange((codeBlock as CodeBlock).AfterList);
                                }
                                else
                                {
                                    subResult[0].Add(codeBlock);
                                    subResult[1].Add(codeBlock);
                                }
                            }
                        }
                        else
                        {
                            foreach (RuleBlock subRuleBlock in RuleList)
                            {
                                RuleBlock rule = new RuleBlock(subRuleBlock);
                                if (!subRuleBlock.IsPureRegex)
                                {
                                    subResult = CompareToSingleRule(rule, scope[2]);//0 : beforeResult , 1 : afterResult
                                    if (subResult != null)
                                    {
                                        break;
                                    }
                                }

                                
                            }
                        }

                        if (subResult != null)
                        {
                            content.Content = "";
                            content.BeforeList = subResult[0];
                            content.AfterList = subResult[1];
                        }

                        CodeBlock findBlock = ruleBlock.GetCodeBlockById(content.BlockListIndex);
                        if (findBlock == null)
                        {//new block, add block into block list
                            ruleBlock.AddCodeBlock(content);
                        }
                        else
                        {//has this block before

                            bool isContentSame = false;
                            if (subResult != null)
                            {
                                isContentSame = StaticValue.GetAllContent(content.BeforeList).Equals(findBlock.BeforeList);
                            }
                            else
                            {
                                isContentSame = content.Content.Equals(findBlock.Content);
                            }

                            if (!isContentSame/*content.Content.Equals(findBlock.Content)*/)
                            {//if parameter is not same in block list, then this is not match. Add this into need check list.
                                if (HasTokenInList(analysisContentList, endToken) && ruleSliceIndex == 0)
                                {// if still can find end token , watch back to find if there has block match or not

                                    List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                    front.AddRange(list[0]);
                                    analysisContentList = list[1];
                                    continue;
                                }
                                else
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }
                        result[0].Add(content);
                    }

                    analysisContentList = backList;
                }
                else
                {//is normal ruleSlice
                    List<ICodeBlock>[] match = GetListSpiltByToken(analysisContentList, ruleSlice[ruleSliceIndex].Content);//0-front 1-match 2-back
                    int blockId = ruleSlice[ruleSliceIndex].BlockId;
                    if ((StaticValue.HasContent(match[0]) || HasIsMatch(match[0])) && ruleSliceIndex == 0)
                    {
                        front.AddRange(match[0]);
                    }
                    else if (match[1].Count <= 0 || ((StaticValue.HasContent(match[0]) || HasIsMatch(match[0])) && ruleSliceIndex > 0))//if not first rule, need to start match at index 0
                    {
                        if (ruleSliceIndex > 0 && HasTokenInList(analysisContentList, scopeFinder.StartRuleSlice) && HasTokenInList(analysisContentList, scopeFinder.EndRuleSlice))
                        {                            
                            foreach (ICodeBlock frontBlock in result[0])
                            {
                                if (front.Count == 0)
                                {
                                    ICodeBlock addBlock = new NormalBlock(frontBlock.Content);
                                    front.Add(addBlock);
                                }
                                else
                                {
                                    front[front.Count - 1].Content += frontBlock.Content;
                                }                                
                            }

                            result[0] = new List<ICodeBlock>();
                            //reset rule
                            ruleBlock.InitRuleSetting();
                            //change content
                            match = GetListSpiltByToken(analysisContentList, scopeFinder.StartRuleSlice);
                            front.AddRange(match[0]);
                            //reset this compare
                            match[1].AddRange(match[2]);
                            analysisContentList = match[1];
                            ruleSliceIndex = 0;
                            continue;
                        }
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    string value = StaticValue.GetAllContent(match[1]);

                    NormalBlock codeBlock = new NormalBlock(value, blockId);
                    codeBlock.IsMatchRule = true;
                    result[0].Add(codeBlock);

                    analysisContentList = match[2];
                }
                ruleSliceIndex = ruleSliceIndex + 1;
            }

            if (ruleSliceIndex < endRuleIndex)
            {
                isMatch = false;
            }

            if (isMatch)
            {
                //-----set after rule-----
                string baseIndent = "";
                if (front.Count > 0)
                {
                    baseIndent = GetLastWhitespace(front[front.Count - 1], "");
                }
                string whitespaceBeforeText = baseIndent;


                foreach (ICodeBlock afterBlock in ruleBlock.AfterRuleSliceList)
                {
                    int index = ruleBlock.AfterRuleSliceList.IndexOf(afterBlock);

                    if (afterBlock is NormalBlock)
                    {
                        NormalBlock block = new NormalBlock(afterBlock.Content, afterBlock.BlockId);
                        block.IsMatchRule = true;

                        AddWhitespaceBefore(block, whitespaceBeforeText);
                        result[1].Add(block.GetCopy());
                    }
                    else
                    {
                        Match match = Regex.Match(result[1][result[1].Count - 1].Content, @"[\n]+([ \t]+)");
                        if (match.Success)
                        {
                            whitespaceBeforeText = match.Groups[1].Value;
                        }

                        if (afterBlock is ParameterBlock)
                        {
                            ParameterBlock parameter = ruleBlock.GetParameterById((afterBlock as ParameterBlock).ParaListIndex);
                            if (parameter == null)
                            {
                                return null;
                            }
                            parameter.IsMatchRule = true;
                            result[1].Add(parameter);
                        }
                        else if (afterBlock is CodeBlock)
                        {
                            int blockListIndex = (afterBlock as CodeBlock).BlockListIndex;
                            CodeBlock block = ruleBlock.GetCodeBlockById((afterBlock as CodeBlock).BlockListIndex);
                            if (block == null)
                            {
                                return null;
                            }
                            result[1].AddRange(FormattedAfterCodeBlock(block.GetCopy() as CodeBlock, whitespaceBeforeText));
                        }
                        else if (afterBlock is IncludeBlock)
                        {
                            IncludeBlock includeBlock = ruleBlock.GetIncludeBlockById((afterBlock as IncludeBlock).IncludeBlockListIndex);
                            if (includeBlock == null)
                            {
                                return null;
                            }
                            includeBlock.IsMatchRule = true;
                            result[1].AddRange(FormattedAfterIncludeBlock(includeBlock.GetCopy() as IncludeBlock, whitespaceBeforeText));

                        }
                    }

                    if (index < ruleBlock.AfterRuleSliceList.Count - 1 && !ruleBlock.AfterRuleSliceList[index + 1].GetType().Equals(afterBlock.GetType()))
                    {
                        whitespaceBeforeText = baseIndent;
                    }
                    else if (result[1].Count > 0)
                    {
                        whitespaceBeforeText = GetLastWhitespace(result[1][result[1].Count - 1], baseIndent);
                    }
                }

                CodeBlock finalResultBlock = new CodeBlock();
                finalResultBlock.IsMatchRule = true;
                finalResultBlock.BeforeList = result[0];
                finalResultBlock.AfterList = result[1];
                finalResultBlock.MatchRule = ruleBlock;

                //-----set front-----
                if (front.Count > 0)
                {
                    ResetListBlockId(front, front[0].BlockId);

                    List<ICodeBlock> afterFrontList = new List<ICodeBlock>();
                    foreach (ICodeBlock codeBlock in front)
                    {
                        afterFrontList.Add(codeBlock.GetCopy());
                    }
                    finalResult[0].AddRange(front);
                    finalResult[1].AddRange(afterFrontList);
                    needCheck = true;
                }

                finalResult[0].Add(finalResultBlock);
                finalResult[1].Add(finalResultBlock);

                //----back content----
                if (analysisContentList.Count > 0 && StaticValue.GetAllContent(analysisContentList).Length > 0)
                {
                    List<ICodeBlock>[] backResult = CompareToSingleRule(new RuleBlock(ruleBlock), analysisContentList);
                    if (backResult == null)
                    {
                        finalResult[0].AddRange(analysisContentList);
                        finalResult[1].AddRange(StaticValue.CopyList(analysisContentList));
                    }
                    else
                    {
                        whitespaceBeforeText = GetLastWhitespace(result[1][result[1].Count - 1], baseIndent);
                        finalResult[0].AddRange(backResult[0]);
                        finalResult[1].AddRange(backResult[1]);
                    }

                    needCheck = true;
                }

                return finalResult;
            }
            else
            {
                return null;
            }
        }

        private bool HasIsMatch(List<ICodeBlock> list)
        {
            bool result = false;

            foreach (ICodeBlock codeBlock in list)
            {
                if (codeBlock.IsMatchRule)
                {
                    return true;
                }
            }

            return result;
        }

             //-----list tool-----
        private void ResetListBlockId(List<ICodeBlock> orgList, int blockId)
        {
            foreach (ICodeBlock codeBlock in orgList)
            {
                codeBlock.BlockId = blockId;
            }
        }

        private bool HasTokenInList(List<ICodeBlock> codeBlockList, string token)
        {
            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                if (Regex.Match(codeBlock.Content,token).Success)
                {
                    return true;
                }
            }

            return false;
        }

        private List<ICodeBlock>[] GetListAfterToken(List<ICodeBlock> codeBlockList, string token)
        {
            List<ICodeBlock>[] result = new List<ICodeBlock>[2] { new List<ICodeBlock>(), new List<ICodeBlock>() };//0-front + token , 1 - back
            bool start = false;
            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                if (start)
                {
                    result[1].Add(codeBlock);
                }
                else if (Regex.Match(codeBlock.Content, token).Success)
                {
                    Match match = Regex.Match(codeBlock.Content, token);
                    int nextIndex = match.Index + match.Length;
                    start = true;
                    result[0].Add(new NormalBlock(codeBlock.Content.Substring(0, nextIndex)));
                    result[1].Add(new NormalBlock(codeBlock.Content.Substring(nextIndex)));
                }
                else
                {
                    result[0].Add(codeBlock);
                }
            }
            return result;
        }

        private List<ICodeBlock>[] GetListSpiltByToken(List<ICodeBlock> codeBlockList, string token)
        {
            List<ICodeBlock>[] result = new List<ICodeBlock>[3] { new List<ICodeBlock>(), new List<ICodeBlock>(), new List<ICodeBlock>() };
            bool isFind = false;
            foreach (ICodeBlock codeBlock in codeBlockList)
            {                
                if (isFind)
                {
                    result[2].Add(codeBlock);
                }
                else if (Regex.Match(codeBlock.Content,token).Success)
                {
                    if (codeBlock.IsMatchRule)
                    {
                        if (isFind)
                        {
                            result[2].Add(codeBlock);
                        }
                        else
                        {
                            result[0].Add(codeBlock);
                        }
                    }
                    else
                    {
                        Match match = Regex.Match(codeBlock.Content, token);
                        int nextIndex = match.Index + match.Length;
                        isFind = true;
                        if (match.Index > 0)
                        {
                            NormalBlock front = new NormalBlock(codeBlock.Content.Substring(0, match.Index));
                            front.MatchRule = codeBlock.MatchRule;
                            front.IsMatchRule = codeBlock.IsMatchRule;
                            result[0].Add(front);
                        }
                        NormalBlock normalBlock = new NormalBlock(match.Value);
                        normalBlock.MatchRule = codeBlock.MatchRule;
                        normalBlock.IsMatchRule = codeBlock.IsMatchRule;
                        result[1].Add(normalBlock);
                        NormalBlock back = new NormalBlock(codeBlock.Content.Substring(nextIndex));
                        back.MatchRule = codeBlock.MatchRule;
                        back.IsMatchRule = codeBlock.IsMatchRule;
                        result[2].Add(back);
                    }
                }
                else
                {
                     result[0].Add(codeBlock);
                }
            }
            return result;
        }

        //--------
        private void AddWhitespaceBefore(ICodeBlock codeBlock, string whitespace)
        {
            codeBlock.Content = Regex.Replace(codeBlock.Content, @"\n", "\n" + whitespace);
        }

        private string GetLastWhitespace(ICodeBlock block,string baseIndent)
        {
            string result = baseIndent;

            string text = block.Content;

            Match match = Regex.Match(text,@"([ \t]+)\Z");

            if (match.Success)
            {
                result = match.Groups[1].Value ;
            }

            return result;
        }
        
        private List<ICodeBlock> FormattedAfterCodeBlock(CodeBlock block,string whitespaceBeforeText)
        {
            List<ICodeBlock> result = new List<ICodeBlock>();

            if (block.AfterList != null && block.AfterList.Count > 0)
            {
                for (int i = 0; i < block.AfterList.Count; i++)
                {
                    ICodeBlock codeBlock = block.AfterList[i];

                    if (codeBlock is CodeBlock)
                    {
                        result.AddRange(FormattedAfterCodeBlock(codeBlock as CodeBlock, whitespaceBeforeText));
                    }
                    else
                    {
                        AddWhitespaceBefore(codeBlock, whitespaceBeforeText);
                        result.Add(codeBlock.GetCopy());
                    }
                }
            }
            else
            {
                AddWhitespaceBefore(block, whitespaceBeforeText);
                result.Add(block.GetCopy());
            }

            return result;
        }

        private List<ICodeBlock> FormattedAfterIncludeBlock(IncludeBlock includeBlock, string whitespaceBeforeText)
        {
            List<ICodeBlock> result = new List<ICodeBlock>();

            foreach (ICodeBlock codeBlock in includeBlock.AfterList)
            {
                if (codeBlock is IncludeBlock)
                {
                    result.AddRange(FormattedAfterIncludeBlock(codeBlock as IncludeBlock, whitespaceBeforeText));
                }
                else
                {
                    AddWhitespaceBefore(codeBlock, whitespaceBeforeText);
                    result.Add(codeBlock.GetCopy());
                }
            }

            return result;
        }
    }
}
