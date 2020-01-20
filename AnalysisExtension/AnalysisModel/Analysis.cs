using AnalysisExtension.Tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace AnalysisExtension.Model
{
    public class Analysis
    {
        public string Name { get; set; }
        public RuleSet AnalysisRuleSet { get; set; }
        public string RuleFolderPath { get; set; }
        public List<RuleBlock> RuleList { get; set; }

        AnalysisTool analysisTool = AnalysisTool.GetInstance();
        bool needCheck = false;

        //List<List<ICodeBlock>[]> AllResult = new List<List<ICodeBlock>[]>();

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
            RuleFolderPath = StaticValue.RULE_FOLDER_PATH + path;
        }
        
        //-----analysis method-----
        public void AnalysisMethod(BackgroundWorker backgroundWorker)
        {
            string[] orgContentList = FileLoader.GetInstance().GetFileContent();
            
            for (int fileCount = 0; fileCount < orgContentList.Length; fileCount++)
            {
                bool isMatch = false;

                backgroundWorker.ReportProgress((int)((float)(fileCount + 1) / orgContentList.Length) * 80);
                foreach (RuleBlock ruleBlock in RuleList)
                {
                    isMatch = MatchRule(ruleBlock, fileCount, orgContentList[fileCount]);
                    if (isMatch)
                    {
                        break;
                    }                    
                }

                if (!isMatch)
                {
                    NormalBlock codeBlock = new NormalBlock(orgContentList[fileCount]);
                    codeBlock.IsMatchRule = false;
                    analysisTool.AddIntoBeforeList(codeBlock, fileCount);                    
                    analysisTool.AddIntoAfterList(codeBlock, fileCount);
                }
            }

        /*    while (needCheck)
            {
                foreach (RuleBlock ruleBlock in RuleList)
                {
                    for (int fileCount = 0; fileCount < orgContentList.Length; fileCount++)
                    {
                        MatchNeedCheck(ruleBlock, fileCount);
                    }
                }
            }*/
            
        }

        private void MatchNeedCheck(RuleBlock ruleBlock, int fileCount)
        {
            List<ICodeBlock> beforeCodeBlock = analysisTool.GetFinalBeforeBlockList()[fileCount];
            int changeCount = 0;
            List<ICodeBlock>[] result = CompareToSingleRule(ruleBlock, beforeCodeBlock);//0 : beforeResult , 1 : afterResult
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

        private bool MatchRule(RuleBlock ruleBlock, int fileCount, string orgContent)
        {//orgContent will pass by value , so it is a copy of org data 

            bool isMatch = false;
            //List<ICodeBlock> beforeCodeBlock = analysisTool.GetFinalBeforeBlockList()[fileCount];

            ruleBlock.InitRuleSetting();
            List<ICodeBlock>[] result = CompareToSingleRule(ruleBlock,  EscapeTokenSet.SpiltByEscapeToken(orgContent));//0 : beforeResult , 1 : afterResult

            if (result != null && result[0].Count > 0 && result[1].Count > 0)
            {
                //save result

                analysisTool.AddListIntoBeforeList(result[0],fileCount);
                analysisTool.AddListIntoAfterList(result[1], fileCount);
                isMatch = true;
            }


            return isMatch;                       
        }

        public List<ICodeBlock>[] CompareToSingleRule(RuleBlock ruleBlock, List<ICodeBlock> orgBlockList)//if not match , return null
        { 
            List<ICodeBlock> ruleSlice = ruleBlock.BeforeRuleSliceList;
            bool isMatch = true;
            List<ICodeBlock>[] result = new List<ICodeBlock>[2];

            result[0] = new List<ICodeBlock>();
            result[1] = new List<ICodeBlock>();

            /*string orgContent = analysisContent;*/
            int endRuleIndex = ruleBlock.BeforeRuleSliceList.Count - 1;
            int ruleSliceIndex = 0;

            List<ICodeBlock> analysisContentList = orgBlockList;

            while (ruleSliceIndex <= endRuleIndex)
            {
                string startToken = null;
                string endToken = null;
                List<ICodeBlock> backList = new List<ICodeBlock>();

                //set start token and end token
                if (ruleSlice[ruleSliceIndex].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME) || ruleSlice[ruleSliceIndex].Content.Contains("<block") || ruleSlice[ruleSliceIndex].TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                {
                    string[] token = GetStartTokenAndEndToken(ruleSlice, ruleSliceIndex);
                    startToken = token[0];
                    endToken = token[1];
                    //set analysis content
                    if (ruleSliceIndex > 0)
                    {
                        int count = result[0].Count - 1;
                        if (startToken != null && (startToken.Equals("[\\n\\r]*") || startToken.Equals("[\\n\\r]+")))
                        {
                            while (startToken != null && count >= 0 && (startToken.Equals("[\\n\\r]*") || startToken.Equals("[\\n\\r]+")))
                            {
                                startToken = Regex.Escape(result[0][count].Content) + startToken;
                                analysisContentList.Insert(0, new NormalBlock(result[0][count].Content));
                                count--;
                            }
                        }
                        else if ((ruleSlice[ruleSliceIndex - 1].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME) || ruleSlice[ruleSliceIndex - 1].TypeName.Equals(StaticValue.CODE_BLOCK_TYPE_NAME)))
                        {
                            //if startToken is parameter or codeBlock, get content from result list
                            startToken = "[ ]+" + Regex.Escape(result[0][result[0].Count - 1].Content);
                            //add content into content need to analysis 
                            analysisContentList.Insert(0, new NormalBlock(" " + result[0][result[0].Count - 1].Content));
                        }
                        else
                        {
                            analysisContentList.Insert(0, new NormalBlock(result[0][result[0].Count - 1].Content));
                        }
                    }
                
                    //-----find-----
                    List<ICodeBlock>[] scope = FindScope(startToken, endToken, analysisContentList);//0-front 1-startToken 2-para 3-endToken 4-back

                    if (scope == null || (scope != null & StaticValue.GetAllContent(scope[0]).Length > 0 && ruleSliceIndex > 1))
                    {// the rule slice that not first rule slice need to follow front of content (at index 0) , if it has some text before , then not match
                        isMatch = false;
                        break;
                    }
                    else
                    {
                        //match
                        if (scope[0] != null && scope[0].Count > 0 && ruleSliceIndex == 0)
                        {
                            scope[0].AddRange(scope[1]);
                            result[0].AddRange(scope[0]);
                            result[1].AddRange(scope[0]);
                         //   afterBlockList[0].Add(scope[0]);//add after's front
                        }
                        scope[3].AddRange(scope[4]);
                        backList = scope[3];
                    }

                    if (ruleSlice[ruleSliceIndex].TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                    {//is include
                        IncludeBlock ruleInclude = ruleSlice[ruleSliceIndex] as IncludeBlock;
                        int includeListId = ruleInclude.IncludeBlockListIndex;
                        int blockId = ruleInclude.BlockId;
                        int compareRuleId = ruleInclude.CompareRuleId;
                        int fromRuleSetId = ruleInclude.FromRuleSetId;

                        string rulePath = RuleMetadata.GetInstance().GetRulePathById(fromRuleSetId,compareRuleId);
                        RuleBlock findRule = FileLoader.GetInstance().LoadSingleRuleByPath(rulePath);
                        List<ICodeBlock> newScopeList;

                        if (findRule.BeforeRuleSliceList.Count > 1)
                        {
                            List<ICodeBlock>[] scopeNeedFind = FindScope(findRule.BeforeRuleSliceList[0].Content, findRule.BeforeRuleSliceList[findRule.BeforeRuleSliceList.Count - 1].Content, scope[2]);

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
                            List<ICodeBlock>[] scopeNeedFind = GetListSpiltByToken(scope[2],findRule.BeforeRuleSliceList[0].Content);
                            if(scopeNeedFind == null || scopeNeedFind[0].Count > 0 && StaticValue.GetAllContent(scopeNeedFind[0]).Length > 0)
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

                        List<ICodeBlock>[] findResult = CompareToSingleRule(findRule, newScopeList);

                        if (findResult == null)
                        {
                            isMatch = false;
                            break;
                        }
                        else
                        {                           
                            IncludeBlock resultIncludeBlock = new IncludeBlock("",blockId,includeListId,compareRuleId,fromRuleSetId);
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

                                        result[0].AddRange(list[0]);
                                        result[1].AddRange(list[0]);
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
                    else if (ruleSlice[ruleSliceIndex].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                    {//is parameter

                        int paraListId = (ruleSlice[ruleSliceIndex] as ParameterBlock).ParaListIndex;
                        int blockId = (ruleSlice[ruleSliceIndex] as ParameterBlock).BlockId;
                        string paraContent = StaticValue.GetAllContent(scope[2]);

                        ParameterBlock parameter = new ParameterBlock(paraContent, blockId, paraListId);
                        parameter.IsMatchRule = true;

                        ParameterBlock findBlock = ruleBlock.GetParameterById(parameter.ParaListIndex);
                        if (findBlock != null)
                        {//has this parameter before
                            if (!parameter.Content.Equals(findBlock.Content))
                            {//if parameter is not same in parameter list, then this is not match. Add this into need check list.

                                if (HasTokenInList(analysisContentList,endToken) && ruleSliceIndex == 0)
                                {// if still can find end token , watch back to find if there has parameter match or not

                                    List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                    result[0].AddRange(list[0]);
                                    result[1].AddRange(list[0]);
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
                    else if (ruleSlice[ruleSliceIndex].TypeName.Equals(StaticValue.CODE_BLOCK_TYPE_NAME))
                    {//is block next

                        int blockListIndex = (ruleSlice[ruleSliceIndex] as CodeBlock).BlockListIndex;
                        int blockId = (ruleSlice[ruleSliceIndex] as CodeBlock).BlockId;
                        string blockContent = StaticValue.GetAllContent(scope[2]);
                        CodeBlock content = new CodeBlock(blockContent, blockId, blockListIndex);
                        List<ICodeBlock>[] subResult = null;
                        bool isBlockFind = false;
                        CodeBlock findBlock = ruleBlock.GetCodeBlockById(content.BlockListIndex);

                        foreach (RuleBlock subRuleBlock in RuleList)
                        {
                            RuleBlock rule = new RuleBlock(subRuleBlock);
                            subResult = CompareToSingleRule(subRuleBlock, scope[2]);
                            if (subResult != null)
                            {
                                break;
                            }
                        }
                       
                        if (subResult != null)
                        {
                            content.BeforeList = subResult[0];
                            content.AfterList = subResult[1];
                        }
                        else
                        {
                            content.IsMatchRule = false;
                            needCheck = true;
                        }

                        if (findBlock == null)
                        {//new block
                         //add block into block list
                            ruleBlock.AddCodeBlock(content);
                        }
                        else
                        {//has this block before
                            if (subResult != null)
                            {
                                isBlockFind = ruleBlock.IsCodeBlockSame(findBlock, content);
                            }
                            else
                            {
                                isBlockFind = content.Content.Equals(findBlock.Content);
                            }

                            if (!isBlockFind)
                            {//if parameter is not same in block list, then this is not match. Add this into need check list.
                                if (HasTokenInList(analysisContentList, endToken) && ruleSliceIndex == 0)
                                {// if still can find end token , watch back to find if there has block match or not

                                    List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                    result[0].AddRange(list[0]);
                                    result[1].AddRange(list[0]);
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


                        if (subResult != null)
                        {
                            result[0].AddRange(content.BeforeList);
                        }
                        else
                        {
                            result[0].Add(content);
                        }
                    }

                    if (backList.Count > 0)
                    {
                        analysisContentList = backList;
                    }
                }
                else
                {//is normal ruleSlice
                    List<ICodeBlock>[] match = GetListSpiltByToken(analysisContentList, ruleSlice[ruleSliceIndex].Content);//0-front 1-match 2-back
                    int blockId = ruleSlice[ruleSliceIndex].BlockId;

                    if (match[1].Count <= 0 || (match[0].Count > 0 && StaticValue.GetAllContent(match[0]).Length > 0 && ruleSliceIndex > 0))//if not first rule, need to start match at index 0
                    {
                        isMatch = false;
                        break;
                    }
                    else if (match[0].Count > 0 && ruleSliceIndex == 0)
                    {
                        result[0].AddRange(match[0]);
                        result[1].AddRange(match[0]);
                    }

                    string value = StaticValue.GetAllContent(match[1]);

                    NormalBlock codeBlock = new NormalBlock(value, blockId);
                    codeBlock.IsMatchRule = true;
                    result[0].Add(codeBlock);

                    analysisContentList = match[2];
                }                
                ruleSliceIndex = ruleSliceIndex + 1;
            }

            if (isMatch)
            {
                //-----set after rule-----
                foreach (ICodeBlock afterBlock in ruleBlock.AfterRuleSliceList)
                {
                    if (afterBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                    {
                        ParameterBlock parameter = ruleBlock.GetParameterById((afterBlock as ParameterBlock).ParaListIndex);
                        if (parameter == null)
                        {
                            return null;
                        }
                        parameter.IsMatchRule = true;
                        result[1].Add(parameter);
                    }
                    else if (afterBlock.TypeName.Equals(StaticValue.CODE_BLOCK_TYPE_NAME))
                    {
                        int blockListIndex = (afterBlock as CodeBlock).BlockListIndex;
                        CodeBlock block = ruleBlock.GetCodeBlockById((afterBlock as CodeBlock).BlockListIndex).GetCopy() as CodeBlock;

                        if (block == null)
                        {
                            return null;
                        }

                        if (block.BeforeList != null && block.BeforeList.Count > 0)
                        {
                            result[1].AddRange(block.AfterList);
                        }
                        else
                        {
                            result[1].Add(block);
                        }
                    }
                    else if (afterBlock.TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                    {
                        IncludeBlock includeBlock = ruleBlock.GetIncludeBlockById((afterBlock as IncludeBlock).IncludeBlockListIndex).GetCopy() as IncludeBlock;
                        if (includeBlock == null)
                        {
                            return null;
                        }
                        includeBlock.IsMatchRule = true;
                        result[1].AddRange(includeBlock.AfterList);
                    
                    }
                    else
                    {
                        NormalBlock block = new NormalBlock(afterBlock.Content, afterBlock.BlockId);
                        block.IsMatchRule = true;
                        result[1].Add(block);
                    }
                }

                //----back content----
                string backContent = "";
                if (analysisContentList.Count > 0)
                {
                    backContent += StaticValue.GetAllContent(analysisContentList);
                }

                if (backContent.Length > 0)
                {
                    List<ICodeBlock> back = EscapeTokenSet.SpiltByEscapeToken(backContent);
                    List<ICodeBlock>[] backResult = null;

                    foreach (RuleBlock subRuleBlock in RuleList)
                    {
                        RuleBlock rule = new RuleBlock(subRuleBlock);
                        backResult = CompareToSingleRule(subRuleBlock, back);
                        if (backResult != null)
                        {
                            break;
                        }
                    }

                    if (backResult != null && result[0].Count > 0 && result[1].Count > 0)
                    {
                        result[0].AddRange(backResult[0]);
                        result[1].AddRange(backResult[1]);
                    }
                    else
                    {
                        NormalBlock backCodeBlock = new NormalBlock(backContent);
                        backCodeBlock.IsMatchRule = false;
                        result[0].Add(backCodeBlock.GetCopy() as NormalBlock);
                        result[1].Add(backCodeBlock.GetCopy() as NormalBlock);
                    }
                }

              //  AllResult.Add(matchResult);
                return result;
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

        private List<ICodeBlock>[]  FindScope(string startToken, string endToken, List<ICodeBlock> orgContentList)//if cannot find , return null
        {
            List<ICodeBlock>[] result = null;//front string , start token string  ,match string , end token string , back string

            //stack to find
            // int indexShift = 0;
            bool isMatch = false;
            string front = "";
            string startTokenContent = "";
            string matchContent = "";
            string endTokenContent = "";
            string back = "";
            
            int pairCount = 0;
            //int indexAfterStartToken = -1;
            bool isPairTokenStart = false;
            bool isPairTokenEnd = false;
            string[] pairToken = new string[2];

            result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
            for (int i = 0; i < result.Length;i++)
            {
                result[i] = new List<ICodeBlock>();
            }              


            if (startToken == null)
            {
                return FindSingleEndTokenScope(endToken, orgContentList);
            }

            if (endToken == null)
            {
                return FindSingleStartTokenScope(startToken, orgContentList);
            }

            if (Regex.Match(startToken, "<block").Success || Regex.Match(startToken, "<include").Success)
            {
                startToken = "\n";
            }            

            if (Regex.Match(endToken, "<block").Success || Regex.Match(endToken, "<include").Success)
            {
                endToken = "\n";
            }

            if (EscapeTokenSet.IsPairToken(startToken) && !EscapeTokenSet.IsPairToken(endToken))
            {
                isPairTokenStart = true;
                pairToken = EscapeTokenSet.GetPairToken(startToken);
            }
            else if (!EscapeTokenSet.IsPairToken(startToken) && EscapeTokenSet.IsPairToken(endToken))
            {
                isPairTokenEnd = true;
                pairToken = EscapeTokenSet.GetPairToken(endToken);
            }


           if (startToken.Equals(endToken))
            {
                //return FindSamePairScope(startToken,orgContent);
            }

            if (startToken == "\n")
            {
                startToken = @"[\n\r]+";
            }
            if (endToken == "\n")
            {
                endToken = @"[\n\r]+";
            }

            for (int i = 0; i < orgContentList.Count; i++)
            {
                string orgContent = orgContentList[i].Content;
                if (isMatch)
                {
                    result[4].Add(orgContentList[i]);
                    continue;
                }

                while (orgContentList[i].Content.Length > 0)
                {                   
                    string pattern = startToken + "|" + endToken;
                    if (Regex.Match(orgContentList[i].Content, pattern).Success)
                    {
                        if (Regex.Match(orgContentList[i].Content, startToken).Success &&
                            (!Regex.Match(orgContentList[i].Content, endToken).Success || Regex.Match(orgContentList[i].Content, startToken).Index < Regex.Match(orgContentList[i].Content, endToken).Index))
                        {
                            string escapePattern = EscapeTokenSet.BACKSLASH + startToken;
                            Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                            Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);

                            if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > escapeMatch.Index)
                            {
                                /*int nextIndex = escapeMatch.Index + escapeMatch.Length;
                                orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);*/
                            }
                            else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > stringQuotationMatch.Index)
                            {
                                //orgContentList[i].Content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList[i].Content)[4];
                            }
                            else
                            {
                                Match match = Regex.Match(orgContentList[i].Content, startToken);
                                int nextIndex = match.Index + match.Length;

                                if (pairCount == 0)
                                {
                                    front = orgContentList[i].Content.Substring(0, match.Index);
                                    result[0].AddRange(EscapeTokenSet.SpiltByEscapeToken(front));
                                    startTokenContent = match.Value;
                                    result[1].Add(new NormalBlock(startTokenContent));
                                }
                                else
                                {
                                    matchContent = orgContentList[i].Content.Substring(0, nextIndex);
                                    result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                }
                                pairCount++;           
                                orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                            }
                        }
                        else if (Regex.Match(orgContentList[i].Content, endToken).Success 
                            && (!Regex.Match(orgContentList[i].Content, startToken).Success || Regex.Match(orgContentList[i].Content, endToken).Index <= Regex.Match(orgContentList[i].Content, startToken).Index))
                        {
                            string escapePattern = EscapeTokenSet.BACKSLASH + endToken;
                            Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                            Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);

                            if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, endToken).Index > escapeMatch.Index)
                            {
                                int nextIndex = escapeMatch.Index + escapeMatch.Length;
                                orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                            }
                            else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, endToken).Index > stringQuotationMatch.Index)
                            {
                                orgContentList[i].Content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList[i].Content)[4];
                            }
                            else
                            {
                                pairCount--;
                                Match match = Regex.Match(orgContentList[i].Content, endToken);
                                int nextIndex = match.Index + match.Length;
                                matchContent = orgContentList[i].Content.Substring(0, match.Index);
                                result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                if (pairCount == 0)
                                {
                                    endTokenContent = match.Value;
                                    result[3].Add(new NormalBlock(endTokenContent));
                                    back = orgContentList[i].Content.Substring(nextIndex);
                                    result[4].AddRange(EscapeTokenSet.SpiltByEscapeToken(back));
                                    isMatch = true;
                                    break;
                                    /*if (isPairTokenEnd && orgContentList[i].Content.Length > 1)
                                    {//need to check
                                        string[] returnContent = FindScope(Regex.Escape(pairToken[0]), Regex.Escape(pairToken[1]), matchContent);
                                        if (returnContent != null && (match.Index == orgContentList.IndexOf(returnContent[2]) - match.Length || match.Index == orgContentList.IndexOf(returnContent[4]) - match.Length))
                                        {//this token is not the token want to find
                                            pairCount++;
                                            nextIndex = orgContentList.IndexOf(returnContent[4]);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }*/

                                }
                                else if (pairCount < 0)
                                {
                                    break;
                                }
                                else
                                {
                                    orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                                }
                            }
                        }
                        else
                        {
                            if (pairCount == 0)
                            {
                                result[0].Add(orgContentList[i]);
                            }
                            else
                            {
                                result[2].Add(orgContentList[i]);
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (pairCount == 0)
                        {//is content before start token
                            result[0].Add(orgContentList[i]);
                        }
                        else
                        { 
                            result[2].Add(orgContentList[i]);
                        }
                        break;
                    }
                }
            }

            if (isMatch)
            {
                return result;
            }
            else
            {
                return null;
            }
      
        }

        private string[] FindSamePairScope(string token,string content)
        {
            string[] result = null;//front string , start token string  ,match string , end token string , back string

            int indexInOrgContent = 0;
            string orgContent = content;
            string front = "";
            string matchContent = "";
            string back = "";

            int pairCount = 0;
            int indexAfterStartToken = -1;

            if (!Regex.Match(orgContent, token).Success || !Regex.Match(orgContent, token).Success)
            {//cannot find , not match
                return result;
            }
            else
            {
                Match match = Regex.Match(content, token);
                indexAfterStartToken = match.Index + match.Length;
                
                front = content.Substring(0, match.Index);
                content = content.Substring(match.Index);
                indexInOrgContent = orgContent.IndexOf(content);
            }

            while (content.Length > 0)
            {
                string pattern = token;
                if (Regex.Match(content, pattern).Success)
                {
                    string escapePattern = EscapeTokenSet.BACKSLASH + token;
                    Match escapeMatch = Regex.Match(content, escapePattern);
                    Match stringQuotationMatch = Regex.Match(content, EscapeTokenSet.DOUBLE_QUOTATION);

                    if (escapeMatch.Success)
                    {
                        int nextIndex = escapeMatch.Index + escapeMatch.Length;
                        content = content.Substring(nextIndex);
                    }
                    else if (stringQuotationMatch.Success && !token.Equals(EscapeTokenSet.DOUBLE_QUOTATION))
                    {
                        content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, content)[4];
                    }
                    else
                    {
                        pairCount++;
                        Match match = Regex.Match(content, pattern);
                        int nextIndex = match.Index + match.Length;

                        if (pairCount % 2 == 0)
                        {
                            int matchStringLen = indexInOrgContent + match.Index - indexAfterStartToken;
                            matchContent = orgContent.Substring(indexAfterStartToken, matchStringLen);
                            back = orgContent.Substring(indexInOrgContent + nextIndex);
                            break;
                        }
                        content = content.Substring(nextIndex);
                    }
                    indexInOrgContent = orgContent.IndexOf(content);
                }
            }

            if (pairCount % 2 == 0)
            {
                result = new string[5];//front string , start token string  ,match string , end token string , back string
                result[0] = front;
                result[1] = token;
                result[2] = matchContent;
                result[3] = token;
                result[4] = back;
            }


            return result;
        }

        private List<ICodeBlock>[] FindSingleStartTokenScope(string startToken, List<ICodeBlock> orgContentList)
        {//2+3
            List<ICodeBlock>[] result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>();
            }

          //  int indexShift = 0;            
            bool isMatch = false;

            if (Regex.Match(startToken, "<block").Success || Regex.Match(startToken, "<include").Success)
            {
                startToken = "\n";
            }

            if (startToken == "\n")
            {
                startToken = @"[\n\r]+";
            }

            for (int i = 0; i < orgContentList.Count; i++)
            {
                string orgContent = orgContentList[i].Content;
                if (isMatch)
                {
                    result[2].Add(orgContentList[i]);
                    continue;
                }

                while (orgContentList[i].Content.Length > 0)
                {
                    if (Regex.Match(orgContentList[i].Content, startToken).Success)
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + startToken;
                        Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);

                        if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > escapeMatch.Index)
                        {
                            /*int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);*/
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > stringQuotationMatch.Index)
                        {
                          //  orgContentList[i].Content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList[i].Content)[4];
                        }
                        else
                        {

                            Match match = Regex.Match(orgContentList[i].Content, startToken);
                            int backIndex = match.Index + match.Length /*+ indexShift*/;
                            string front = orgContent.Substring(0, match.Index);
                            result[0].AddRange(EscapeTokenSet.SpiltByEscapeTokenWithBlockId(front, orgContentList[i].BlockId));

                            string startTokenContent = match.Value;
                            result[1].Add(new NormalBlock(startTokenContent));
                            string matchContent = orgContent.Substring(backIndex);
                            result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));

                            isMatch = true;
                            break;
                        }
                        /*indexShift = orgContent.Length - orgContentList[i].Content.Length;*/

                    }
                    else
                    {
                        result[0].Add(orgContentList[i]);
                        break;
                    }
                }
            }

            if (isMatch)
            {
                return result;
            }

            return result;
        }

        private List<ICodeBlock>[] FindSingleEndTokenScope(string endToken, List<ICodeBlock> orgContentList)
        {//1+2
            List<ICodeBlock>[] result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>();
            }

            //int indexShift = 0;
            
            bool isMatch = false;
            
            if (Regex.Match(endToken, "<block").Success || Regex.Match(endToken, "<include").Success)
            {
                endToken = "\n";
            }

            if (endToken == "\n")
            {
                endToken = @"[\n\r]+";
            }

            for (int i = 0; i < orgContentList.Count; i++)
            {
                string orgContent = orgContentList[i].Content;

                if (isMatch)
                {
                    result[4].Add(orgContentList[i]);
                    continue;
                }

                while (orgContentList[i].Content.Length > 0)
                {
                    if (Regex.Match(orgContentList[i].Content, endToken).Success)
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + endToken;
                        Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);

                        if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, endToken).Index > escapeMatch.Index)
                        {
                            /*int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);*/
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, endToken).Index > stringQuotationMatch.Index)
                        {
                            //orgContentList[i].Content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList[i].Content)[4];
                        }
                        else
                        {
                            Match match = Regex.Match(orgContentList[i].Content, endToken);
                            int backIndex = match.Index + match.Length/* + indexShift*/;
                            string matchContent = orgContent.Substring(0, match.Index);
                            result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));

                            string endTokenContent = match.Value;
                            result[3].Add(new NormalBlock(endTokenContent));
                            string back = orgContent.Substring(backIndex);
                            result[4].AddRange(EscapeTokenSet.SpiltByEscapeTokenWithBlockId(back, orgContentList[i].BlockId));
                            isMatch = true;
                            break;
                        }
                        //indexShift = orgContent.Length - orgContentList[i].Content.Length;

                    }
                    else
                    {
                        result[2].Add(orgContentList[i]);
                        break;
                    }
                }
            }

            if (isMatch)
            {
                return result;
            }

            return result;         
        }

        private string[] GetStartTokenAndEndToken(List<ICodeBlock> ruleSlice, int ruleSliceIndex)
        {
            string startToken = null, endToken = null;
            int endRuleIndex = ruleSlice.Count - 1;

            if (ruleSliceIndex == 0)
            {
                startToken = null;
                endToken = ruleSlice[ruleSliceIndex + 1].Content;
            }
            else
            {
                if (ruleSliceIndex == endRuleIndex)
                {
                    startToken = ruleSlice[ruleSliceIndex - 1].Content;
                    endToken = null;
                    return new string[] { startToken, endToken };
                }
                else
                {
                    startToken = ruleSlice[ruleSliceIndex - 1].Content;
                    if (ruleSlice[ruleSliceIndex + 1].TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                    {
                        string rulePath = RuleMetadata.GetInstance().GetRulePathById((ruleSlice[ruleSliceIndex + 1] as IncludeBlock).FromRuleSetId, (ruleSlice[ruleSliceIndex + 1] as IncludeBlock).CompareRuleId);
                        RuleBlock findRule = FileLoader.GetInstance().LoadSingleRuleByPath(rulePath);
                        endToken = findRule.GetFirstNormalBlock().Content;
                    }
                    else
                    {
                        endToken = ruleSlice[ruleSliceIndex + 1].Content;
                    }
                }
            }

            if (startToken != null && (startToken.Equals("[\\n\\r]*") || startToken.Equals("[\\n\\r]+")))
            {
                startToken = null;
            }

            while (endToken!=null && (endToken.Equals("[\\n\\r]*") || endToken.Equals("[\\n\\r]+")))
            {
                ruleSliceIndex ++;
                string[] find = GetStartTokenAndEndToken(ruleSlice,ruleSliceIndex);
               // startToken = find[0];
                endToken += find[1];
            }
            
            return new string[] { startToken, endToken };
        }
        //-----list tool-----

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
                        result[2].Add(codeBlock);
                        continue;
                    }
                    Match match = Regex.Match(codeBlock.Content, token);
                    int nextIndex = match.Index + match.Length;
                    isFind = true;
                    if (match.Index > 0)
                    {
                        result[0].Add(new NormalBlock(codeBlock.Content.Substring(0,match.Index)));
                    }
                    result[1].Add(new NormalBlock(match.Value));
                    result[2].Add(new NormalBlock(codeBlock.Content.Substring(nextIndex)));
                }
                else
                {
                     result[0].Add(codeBlock);
                }
            }
            return result;
        }


    }
}
