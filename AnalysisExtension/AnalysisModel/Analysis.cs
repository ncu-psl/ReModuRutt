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

        List<List<ICodeBlock>[]> AllResult = new List<List<ICodeBlock>[]>();

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
                    CodeBlock codeBlock = new CodeBlock(orgContentList[fileCount]);
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
            List<ICodeBlock>[] matchResult = new List<ICodeBlock>[] { new List<ICodeBlock>(), new List<ICodeBlock>()};

            result[0] = new List<ICodeBlock>();
            result[1] = new List<ICodeBlock>();

            /*string orgContent = analysisContent;*/
            int endRuleIndex = ruleBlock.BeforeRuleSliceList.Count - 1;
            int ruleSlicIndex = 0;
            List<List<ICodeBlock>>[] afterBlockList = new List<List<ICodeBlock>>[2];//0-front 1-block
            for (int i = 0; i < afterBlockList.Length; i++)
            {
                afterBlockList[i] = new List<List<ICodeBlock>>();
            }

            ruleBlock.InitRuleSetting();
            List<ICodeBlock> analysisContentList = orgBlockList;

            while (ruleSlicIndex <= endRuleIndex)
            {
                string startToken;
                string endToken;

                //set start token and end token
                if (ruleSlice[ruleSlicIndex].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME) || ruleSlice[ruleSlicIndex].Content.Contains("<block"))
                {
                    if (ruleSlicIndex == 0)
                    {
                        startToken = null;
                        endToken = ruleSlice[ruleSlicIndex + 1].Content;
                    }
                    else
                    {
                        if (ruleSlicIndex == endRuleIndex)
                        {
                            startToken = ruleSlice[ruleSlicIndex - 1].Content;
                            endToken = null;
                        }
                        else
                        {
                            startToken = ruleSlice[ruleSlicIndex - 1].Content;
                            endToken = ruleSlice[ruleSlicIndex + 1].Content;
                        }

                        //set analysis content
                        if (ruleSlice[ruleSlicIndex - 1].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME) || ruleSlice[ruleSlicIndex - 1].Content.Contains("<block"))
                        {
                            //if startToken is parameter or codeBlock, get content from result list
                            startToken = "[ ]+" + Regex.Escape(result[0][result[0].Count - 1].Content);
                            //add content into content need to analysis 
                            analysisContentList.Insert(0, new CodeBlock(" "  + result[0][result[0].Count - 1].Content));
                        }
                        else
                        {
                            analysisContentList.Insert(0, new CodeBlock(result[0][result[0].Count - 1].Content));
                        }

                    }

                    /*  if (EscapeTokenSet.IsPairToken(startToken))
                        {//TODO : fix

                            startToken = Regex.Escape(result[0][result[0].Count - 2].Content) + startToken;
                            analysisContent = result[0][result[0].Count - 2].Content + analysisContent;
                        }*/

                    //-----find-----
                    List<ICodeBlock>[] scope = FindScope(startToken, endToken, analysisContentList);//0-front 1-startToken 2-para 3-endToken 4-back

                    if (scope == null || (scope != null & StaticValue.GetAllContent(scope[0]).Length > 0 && ruleSlicIndex > 1))
                    {// the rule slice that not first rule slice need to follow front of content (at index 0) , if it has some text before , then not match
                        isMatch = false;
                        break;
                    }
                    else
                    {
                        //match
                        if (scope[0] != null && scope[0].Count > 0 && ruleSlicIndex == 0)
                        {
                            scope[0].AddRange(scope[1]);
                            result[0].AddRange(scope[0]);
                            afterBlockList[0].Add(scope[0]);//add after's front

                            scope[3].AddRange(scope[4]);
                            analysisContentList = scope[3];
                        }
                    }

                    if (ruleSlice[ruleSlicIndex].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                    {//is parameter

                        int paraListId = (ruleSlice[ruleSlicIndex] as ParameterBlock).ParaListIndex;
                        int blockId = (ruleSlice[ruleSlicIndex] as ParameterBlock).BlockId;
                        string paraContent = StaticValue.GetAllContent(scope[2]);

                        ParameterBlock parameter = new ParameterBlock(paraContent, blockId, paraListId);
                        parameter.IsMatchRule = true;

                        ParameterBlock findBlock = ruleBlock.GetParameterById(parameter.ParaListIndex);
                        if (findBlock != null)
                        {//has this parameter before
                            if (!parameter.Content.Equals(findBlock.Content))
                            {//if parameter is not same in parameter list, then this is not match. Add this into need check list.

                                if (HasTokenInList(analysisContentList,endToken) && ruleSlicIndex == 0)
                                {// if still can find end token , watch back to find if there has parameter match or not

                                    List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                    result[0].AddRange(list[0]);
                                    afterBlockList[0].Add(list[0]);//add after's front
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
                        matchResult[0].Add(parameter);
                    }
                    else if (ruleSlice[ruleSlicIndex].Content.Contains("<block"))
                    {//is block next

                        int blockListIndex = (ruleSlice[ruleSlicIndex] as CodeBlock).BlockListIndex;
                        int blockId = (ruleSlice[ruleSlicIndex] as CodeBlock).BlockId;
                        string blockContent = StaticValue.GetAllContent(scope[2]);
                        CodeBlock content = new CodeBlock(blockContent, blockId, blockListIndex);
                        content.IsMatchRule = false;
                        needCheck = true;

                        if (HasIsMatch(scope[2]))
                        {
                            content.IsMatchRule = true;
                        }              

                        CodeBlock findBlock = ruleBlock.GetCodeBlockById(content.BlockListIndex);
                        if (findBlock != null)
                        {//has this parameter before
                            if (!content.Content.Equals(findBlock.Content))
                            {//if parameter is not same in parameter list, then this is not match. Add this into need check list.
                                if (HasTokenInList(analysisContentList, endToken) && ruleSlicIndex == 0)
                                {// if still can find end token , watch back to find if there has parameter match or not

                                    List<ICodeBlock>[] list = GetListAfterToken(analysisContentList, endToken);

                                    result[0].AddRange(list[0]);
                                    afterBlockList[0].Add(list[0]);//add after's front
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
                        {//new block
                            //add block into block list
                             ruleBlock.AddCodeBlock(content);
                        }

                        if (HasIsMatch(scope[2]))
                        {
                            result[0].AddRange(scope[2]);
                            matchResult[0].AddRange(scope[2]);
                            afterBlockList[1].Add(scope[2]);//add after's block
                        }
                        else
                        {
                            result[0].Add(content);
                            List<ICodeBlock> afterList = new List<ICodeBlock>();
                            afterList.Add(content);
                            matchResult[0].Add(content);
                            afterBlockList[1].Add(afterList);//add after's block
                        }
                    }
                    if (scope[3] != null)
                    {
                        if (scope[4] != null)
                        {
                            scope[3].AddRange(scope[4]);
                        }
                        analysisContentList = scope[3];
                    }
                }
                else
                {//is normal ruleSlice
                    List<ICodeBlock>[] match = GetListSpiltByToken(analysisContentList, ruleSlice[ruleSlicIndex].Content);//0-front 1-match 2-back
                    int blockId = ruleSlice[ruleSlicIndex].BlockId;

                    if (match[1].Count <= 0 || (match[0].Count > 0 && StaticValue.GetAllContent(match[0]).Length > 0 && ruleSlicIndex > 0))//if not first rule, need to start match at index 0
                    {
                        isMatch = false;
                        break;
                    }
                    else if (match[0].Count > 0 && ruleSlicIndex == 0)
                    {
                        result[0].AddRange(match[0]);
                        afterBlockList[0].Add(match[0]);//add after's front
                    }

                    string value = StaticValue.GetAllContent(match[1]);

                    CodeBlock codeBlock = new CodeBlock(value, blockId, -1);
                    codeBlock.IsMatchRule = true;
                    result[0].Add(codeBlock);
                    matchResult[0].Add(codeBlock);

                    analysisContentList = match[2];
                }                
                ruleSlicIndex = ruleSlicIndex + 1;
            }

            if (isMatch)
            {
                if (afterBlockList[0].Count > 0)
                {
                    foreach (List<ICodeBlock> codeBlock in afterBlockList[0])
                    {
                        result[1].AddRange(codeBlock);
                    }
                }

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
                        matchResult[1].Add(parameter);
                    }
                    else if (afterBlock.Content.Contains("<block"))
                    {
                        int blockListIndex = (afterBlock as CodeBlock).BlockListIndex;
                        List<ICodeBlock> blockContent = afterBlockList[1][blockListIndex-1];

                        if (HasIsMatch(blockContent))
                        {
                            List<ICodeBlock> match = FindResultAlreadyMatch(blockContent);
                            result[1].AddRange(match);
                            matchResult[1].AddRange(match);
                        }
                        else
                        {
                            CodeBlock block = ruleBlock.GetCodeBlockById((afterBlock as CodeBlock).BlockListIndex);
                            if (block == null)
                            {
                                return null;
                            }
                            block.IsMatchRule = false;
                            result[1].Add(block);
                            matchResult[1].Add(block);
                        }
                    }
                    else
                    {
                        int blockListIndex = (afterBlock as CodeBlock).BlockListIndex;
                        CodeBlock block = new CodeBlock(afterBlock.Content, afterBlock.BlockId, blockListIndex);
                        block.IsMatchRule = true;
                        result[1].Add(block);
                        matchResult[1].Add(block);
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
                    List<ICodeBlock>[] backResult = CompareToSingleRule(ruleBlock, back);
                    if (backResult != null && result[0].Count > 0 && result[1].Count > 0)
                    {
                        result[0].AddRange(backResult[0]);
                        result[1].AddRange(backResult[1]);
                    }
                    else
                    {
                        CodeBlock backCodeBlock = new CodeBlock(backContent);
                        backCodeBlock.IsMatchRule = false;
                        result[0].Add(backCodeBlock);
                        result[1].Add(backCodeBlock);
                    }
                }

                AllResult.Add(matchResult);
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

            if (Regex.Match(startToken, "<block").Success)
            {
                startToken = "\n";
            }

            if (Regex.Match(endToken, "<block").Success)
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
                   /* if (orgContentList[i].IsMatchRule)
                    {*/
                        result[4].Add(orgContentList[i]);
               /*     }
                    else
                    {
                        back += orgContentList[i].Content;
                    }*/
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
                                int nextIndex = escapeMatch.Index + escapeMatch.Length;
                                orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                            }
                            else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > stringQuotationMatch.Index)
                            {
                                orgContentList[i].Content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList[i].Content)[4];
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
                                    result[1].Add(new CodeBlock(startTokenContent));
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
                                    result[3].Add(new CodeBlock(endTokenContent));
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
                               /* if (orgContentList[i].IsMatchRule)
                                {*/
                                    result[0].Add(orgContentList[i]);
                           /*     }
                                else
                                {
                                    front += orgContentList[i].Content;
                                }*/
                            }
                            else
                            {
                              /*  if (orgContentList[i].IsMatchRule)
                                {*/
                                    result[2].Add(orgContentList[i]);
                            /*    }
                                else
                                {
                                    matchContent += orgContentList[i].Content;
                                }*/
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (pairCount == 0)
                        {//is content before start token
                           /* if (orgContentList[i].IsMatchRule)
                            {*/
                                result[0].Add(orgContentList[i]);
                         /*   }
                            else
                            {
                                front += orgContentList[i].Content;
                            }*/
                        }
                        else
                        {//is content between start token and end token
                           /* if (orgContentList[i].IsMatchRule)
                            {*/
                                result[2].Add(orgContentList[i]);
                           /* }
                            else
                            {
                                matchContent += orgContentList[i].Content;
                            }*/
                        }
                        break;
                    }
                }
            }

            if (isMatch)
            {
                /*//result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
                if (front.Length > 0)
                {
                    result[0].AddRange(EscapeTokenSet.SpiltByEscapeToken(front));
                }
                //result[1] = new List<ICodeBlock>();
                result[1].Add(new CodeBlock(startTokenContent));
                if (matchContent.Length > 0)
                {
                    result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                }
               // result[3] = new List<ICodeBlock>();
                result[3].Add(new CodeBlock(endTokenContent));
                if (back.Length > 0)
                {
                    result[4].AddRange(EscapeTokenSet.SpiltByEscapeToken(back));
                }*/
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
        {
            List<ICodeBlock>[] result = null;//front string , start token string  ,match string , end token string , back string

            //stack to find
            int indexShift = 0;

            string matchContent = "";
            string startTokenContent = "";
            string front = "";

            bool isMatch = false;

            if (Regex.Match(startToken, "<block").Success)
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
                    matchContent += orgContentList[i].Content;
                    continue;
                }

                while (orgContentList[i].Content.Length > 0)
                {
                    if (orgContentList[i].IsMatchRule)
                    {
                        break;
                    }

                    if (Regex.Match(orgContentList[i].Content, startToken).Success)
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + startToken;
                        Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);

                        if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > escapeMatch.Index)
                        {
                            int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > stringQuotationMatch.Index)
                        {
                            orgContentList[i].Content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList[i].Content)[4];
                        }
                        else
                        {
                            Match match = Regex.Match(orgContentList[i].Content, startToken);
                            startTokenContent = match.Value;
                            front = orgContent.Substring(0, match.Index);

                            int backIndex = match.Index + match.Length + indexShift;
                            matchContent = orgContent.Substring(backIndex);
                            isMatch = true;
                            break;
                        }
                        indexShift = orgContent.Length - orgContentList[i].Content.Length;

                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (isMatch)
            {
                result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
                if (front.Length > 0)
                {
                    result[0] = EscapeTokenSet.SpiltByEscapeToken(front);
                }
                result[1] = new List<ICodeBlock>();
                if (startTokenContent.Length > 0)
                {
                    result[1].Add(new CodeBlock(startTokenContent));
                }
                result[2] = EscapeTokenSet.SpiltByEscapeToken(matchContent);
                result[3] = new List<ICodeBlock>();
                result[4] = new List<ICodeBlock>();
            }

            return result;
        }

        private List<ICodeBlock>[] FindSingleEndTokenScope(string endToken, List<ICodeBlock> orgContentList)
        {
            List<ICodeBlock>[] result = null;//front string , start token string  ,match string , end token string , back string

            //stack to find
            int indexShift = 0;

            string matchContent = "";
            string endTokenContent = "";
            string back = "";

            bool isMatch = false;

            if (Regex.Match(endToken, "<block").Success)
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

                while (orgContentList[i].Content.Length > 0)
                {
                    if (orgContentList[i].IsMatchRule)
                    {
                        break;
                    }

                    if (Regex.Match(orgContentList[i].Content, endToken).Success)
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
                            Match match = Regex.Match(orgContentList[i].Content, endToken);
                            endTokenContent = match.Value;
                            int backIndex = match.Index + match.Length + indexShift;
                            orgContentList[i].Content = orgContent.Substring(backIndex);
                            matchContent += orgContent.Substring(0, match.Index);
                            back = orgContentList[i].Content;
                            isMatch = true;
                            break;
                        }
                        indexShift = orgContent.Length - orgContentList[i].Content.Length;

                    }
                    else
                    {
                        matchContent += orgContentList[i].Content;
                        break;
                    }
                }
            }

            if (isMatch)
            {
                result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
                result[0] = new List<ICodeBlock>();
                result[1] = new List<ICodeBlock>();
                if (matchContent.Length > 0)
                {
                    result[2] = EscapeTokenSet.SpiltByEscapeToken(matchContent);
                }
                result[3] = new List<ICodeBlock>();
                result[3].Add(new CodeBlock(endTokenContent));
                if (back.Length > 0)
                {
                    result[4] = EscapeTokenSet.SpiltByEscapeToken(back); ;
                }
            }

            return result;         
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
            List<ICodeBlock>[] result = new List<ICodeBlock>[2] { new List<ICodeBlock>(), new List<ICodeBlock>() };
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
                    result[0].Add(new CodeBlock(codeBlock.Content.Substring(0, nextIndex)));
                    result[1].Add(new CodeBlock(codeBlock.Content.Substring(nextIndex)));
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
                        result[0].Add(new CodeBlock(codeBlock.Content.Substring(0,match.Index)));
                    }
                    result[1].Add(new CodeBlock(match.Value));
                    result[2].Add(new CodeBlock(codeBlock.Content.Substring(nextIndex)));
                }
                else
                {
                     result[0].Add(codeBlock);
                }
            }
            return result;
        }

        private List<ICodeBlock> FindResultAlreadyMatch(List<ICodeBlock> beforeList)
        {
            foreach (List<ICodeBlock>[] list in AllResult)
            {
                if (list != null && StaticValue.IsListSame(list[0], beforeList))
                {
                    return list[1];
                }
            }
            return null;
        }
    }
}
