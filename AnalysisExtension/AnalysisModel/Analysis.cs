using AnalysisExtension.Tool;
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

            while (needCheck)
            {
                foreach (RuleBlock ruleBlock in RuleList)
                {
                    for (int fileCount = 0; fileCount < orgContentList.Length; fileCount++)
                    {
                        MatchNeedCheck(ruleBlock, fileCount);
                    }
                }
            }
            
        }

        private void MatchNeedCheck(RuleBlock ruleBlock, int fileCount)
        {
            List<ICodeBlock> beforeCodeBlock = analysisTool.GetFinalBeforeBlockList()[fileCount];

            int i = 0;
            int changeCount = 0;
            while(i < beforeCodeBlock.Count)//for(int i = 0; i < analysisTool.GetFinalBeforeBlockList()[fileCount].Count; i++)//(ICodeBlock codeBlock in analysisTool.GetFinalBeforeBlockList()[fileCount].ToArray())
            {
                if (!beforeCodeBlock[i].IsMatchRule)
                {
                    int blockId = beforeCodeBlock[i].BlockId;
                    List<ICodeBlock>[] result = CompareToSingleRule(ruleBlock, beforeCodeBlock[i].Content);//0 : beforeResult , 1 : afterResult
                    if (result != null)
                    {
                        analysisTool.RefreshNotMatchBlock(blockId, result);
                        changeCount++;
                    }                   
                }
                i++;            
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
            List<ICodeBlock>[] result = CompareToSingleRule(ruleBlock, orgContent);//0 : beforeResult , 1 : afterResult

            if (result != null && result[0].Count > 0 && result[1].Count > 0)
            {
                //save result

                analysisTool.AddListIntoBeforeList(result[0],fileCount);
                analysisTool.AddListIntoAfterList(result[1], fileCount);
                isMatch = true;
            }


            return isMatch;                       
        }

        public List<ICodeBlock>[] CompareToSingleRule(RuleBlock ruleBlock, string analysisContent)//if not match , return null
        { 
            List<ICodeBlock> ruleSlice = ruleBlock.BeforeRuleSliceList;
            bool isMatch = true;
            List<ICodeBlock>[] result = new List<ICodeBlock>[2];
            result[0] = new List<ICodeBlock>();
            result[1] = new List<ICodeBlock>();

            string orgContent = analysisContent;
            int startRuleIndex = 0;
            int endRuleIndex = ruleBlock.BeforeRuleSliceList.Count - 1;
            int ruleSlicIndex = startRuleIndex;// compare from the second ruleSlice to the penultimate ruleSlice
            bool hasFront = false;

            ruleBlock.InitRuleSetting();

            while (analysisContent.Length > 0 && ruleSlicIndex < endRuleIndex)
            {
                if (ruleSlice[ruleSlicIndex].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                {//is parameter
                    string startToken = ruleSlice[ruleSlicIndex - 1].Content;
                    string endToken = ruleSlice[ruleSlicIndex + 1].Content;
                    string[] scope = FindScope(startToken, endToken, result[0][result[0].Count - 1].Content + analysisContent);//0-front 1-startToken 2-para 3-endToken 4-back

                    if ((scope == null || scope[0].Length > 0) && ruleSlicIndex > 0)// the rule slice that not first rule slice need to follow front of content (at index 0) , if it has some text before , then not match
                    {
                        if (Regex.Match(analysisContent, startToken).Success)
                        {
                            analysisContent = analysisContent.Substring(Regex.Match(analysisContent, startToken).Index + Regex.Match(analysisContent, startToken).Length);
                            continue;
                        }
                        else if (Regex.Match(analysisContent, endToken).Success)
                        {
                            analysisContent = analysisContent.Substring(Regex.Match(analysisContent, endToken).Index + Regex.Match(analysisContent, endToken).Length);
                            continue;
                        }
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (scope[0].Length > 0 && ruleSlicIndex == 0)
                    {
                        hasFront = true;
                        CodeBlock front = new CodeBlock(scope[0] + scope[1]);
                        result[0].Add(front);
                    }

                    int paraListId = (ruleSlice[ruleSlicIndex] as ParameterBlock).ParaListIndex;
                    int blockId = (ruleSlice[ruleSlicIndex] as ParameterBlock).BlockId;
                    ParameterBlock parameter = new ParameterBlock(scope[2], blockId, paraListId);
                    parameter.IsMatchRule = true;

                    ParameterBlock findBlock = ruleBlock.GetParameterById(parameter.ParaListIndex);
                    if (findBlock != null)
                    {//has this parameter before
                        if (!parameter.Content.Equals(findBlock.Content))
                        {//if parameter is not same in parameter list, then this is not match. Add this into need check list.

                            if (Regex.Match(analysisContent, endToken).Success)
                            {
                                analysisContent = analysisContent.Substring(Regex.Match(analysisContent, endToken).Index + Regex.Match(analysisContent, endToken).Length);
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

                    ruleSlicIndex = ruleSlicIndex + 1;
                    analysisContent = scope[3] + scope[4];
                }
                else if (ruleSlice[ruleSlicIndex].Content.Contains("<block"))
                {//is block next
                    string startToken = "";
                    string inputContent;
                    if (ruleSlice[ruleSlicIndex - 1].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                    {
                        startToken = "[ ]+" + Regex.Escape(result[0][result[0].Count - 1].Content);
                        inputContent = " " + result[0][result[0].Count - 1].Content + analysisContent;
                    }
                    else
                    {
                        startToken = ruleSlice[ruleSlicIndex - 1].Content;
                        inputContent = result[0][result[0].Count - 1].Content + analysisContent;
                    }
                    string endToken = ruleSlice[ruleSlicIndex + 1].Content;

                    if (EscapeTokenSet.IsPairToken(startToken))
                    {
                        startToken = Regex.Escape(result[0][result[0].Count - 2].Content) + startToken;
                        inputContent = result[0][result[0].Count - 2].Content + inputContent;
                    }
                    string[] scope = FindScope(startToken, endToken,  inputContent);//0-front 1-startToken 2-block 3-endToken 4-back

                    if ((scope == null || scope[0].Length > 0) && ruleSlicIndex > 0)// the rule slice that not first rule slice need to follow front of content (at index 0) , if it has some text before , then not match
                    {
                        if (Regex.Match(analysisContent, startToken).Success)
                        {
                            analysisContent = analysisContent.Substring(Regex.Match(analysisContent, startToken).Index + Regex.Match(analysisContent, startToken).Length);
                            continue;
                        }
                        else if (Regex.Match(analysisContent, endToken).Success)
                        {
                            analysisContent = analysisContent.Substring(Regex.Match(analysisContent, endToken).Index + Regex.Match(analysisContent, endToken).Length);
                            continue;
                        }
                        else
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (scope[0].Length > 0 && ruleSlicIndex == 0)
                    {
                        hasFront = true;
                        CodeBlock front = new CodeBlock(scope[0] + scope[1]);
                        result[0].Add(front);
                    }

                    int blockListIndex = (ruleSlice[ruleSlicIndex] as CodeBlock).BlockListIndex;
                    int blockId = (ruleSlice[ruleSlicIndex] as CodeBlock).BlockId;
                    CodeBlock content = new CodeBlock(scope[2],blockId,blockListIndex);
                    content.IsMatchRule = false;
                    needCheck = true;

                    CodeBlock findBlock = ruleBlock.GetCodeBlockrById(content.BlockListIndex);
                    if (findBlock != null)
                    {//has this parameter before
                        if (!content.Content.Equals(findBlock.Content))
                        {//if parameter is not same in parameter list, then this is not match. Add this into need check list.
                            if (Regex.Match(analysisContent, endToken).Success)
                            {
                                analysisContent = analysisContent.Substring(Regex.Match(analysisContent, endToken).Index + Regex.Match(analysisContent, endToken).Length);
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
                        ruleBlock.AddCodeBlock(content);
                    }
                    result[0].Add(content);

                    ruleSlicIndex = ruleSlicIndex + 1;
                    analysisContent = scope[3] + scope[4];
                }
                else
                {//is normal ruleSlice

                    Match match = Regex.Match(analysisContent, ruleSlice[ruleSlicIndex].Content);
                    int blockId = ruleSlice[ruleSlicIndex].BlockId;

                    if (!match.Success || (match.Index > 0 && ruleSlicIndex > 0))//if not first rule, need to start match at index 0
                    {
                        isMatch = false;
                        break;
                    }
                    else if (match.Index > 0 && ruleSlicIndex == 0)
                    {
                        CodeBlock beforeContent = new CodeBlock(analysisContent.Substring(0,match.Index));
                        beforeContent.IsMatchRule = false;
                        result[0].Add(beforeContent);
                        hasFront = true;
                    }

                    CodeBlock codeBlock = new CodeBlock(match.Value,blockId,-1);
                    codeBlock.IsMatchRule = true;
                    result[0].Add(codeBlock);


                    analysisContent = analysisContent.Substring(match.Index + match.Length);
                    ruleSlicIndex = ruleSlicIndex + 1;
                }
            }//end while

            if (isMatch)
            {
                Match match = Regex.Match(analysisContent, ruleSlice[ruleSlicIndex].Content);
                int blockId = ruleSlice[ruleSlicIndex].BlockId;

                if (!match.Success || match.Index > 0)//need to start match at index 0
                {
                    return null;
                }

                CodeBlock codeBlock = new CodeBlock(match.Value,blockId,-1);
                codeBlock.IsMatchRule = true;
                result[0].Add(codeBlock);
                analysisContent = analysisContent.Substring(match.Index + match.Length);

                //-----set after rule-----
                if (hasFront)
                {
                    result[1].Add(result[0][0]);
                }
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
                    else if (afterBlock.Content.Contains("<block"))
                    {
                        int blockListIndex = (afterBlock as CodeBlock).BlockListIndex;
                        CodeBlock block = ruleBlock.GetCodeBlockrById((afterBlock as CodeBlock).BlockListIndex);
                        if (block == null)
                        {
                            return null;
                        }
                        block.IsMatchRule = false;
                        result[1].Add(block);
                    }
                    else
                    {
                        int blockListIndex = (afterBlock as CodeBlock).BlockListIndex;
                        CodeBlock block = new CodeBlock(afterBlock.Content,afterBlock.BlockId,blockListIndex);
                        block.IsMatchRule = true;
                        result[1].Add(block);
                    }
                }

                if (analysisContent.Length > 0)
                {                   
                    List<ICodeBlock>[]  back = CompareToSingleRule(ruleBlock, analysisContent);
                    if (back != null && result[0].Count > 0 && result[1].Count > 0)
                    {
                        result[0].AddRange(back[0]);
                        result[1].AddRange(back[1]);
                    }
                    else
                    {
                        CodeBlock backCodeBlock = new CodeBlock(analysisContent);
                        backCodeBlock.IsMatchRule = false;
                        result[0].Add(backCodeBlock);
                        result[1].Add(backCodeBlock);
                    }                    
                }
            }
            else
            {//front ruleSlice and back ruleSlice is match , but content between this two slice have something not match
                return null;
            }
            return result;
        }

        private string[]  FindScope(string startToken, string endToken, string content)//if cannot find , return null
        {
            string[] result = null;//front string , start token string  ,match string , end token string , back string

            //stack to find
            int indexInOrgContent = 0;

            string orgContent = content;

            string front = "";
            string startTokenContent = "";
            string matchContent = "";
            string endTokenContent = "";
            string back = "";
            
            int pairCount = 0;
            int indexAfterStartToken = -1;
            bool isPairTokenStart = false;
            bool isPairTokenEnd = false;
            string[] pairToken = new string[2];

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


            if (!Regex.Match(orgContent, startToken).Success || !Regex.Match(orgContent, endToken).Success)
            {//cannot find , not match
                return result;
            }
            else if (startToken.Equals(endToken))
            {
                return FindSamePairScope(startToken,orgContent);
            }
            else
            {
                Match match = Regex.Match(content, startToken);
                indexAfterStartToken = match.Index + match.Length;
                startTokenContent = match.Value;
                front = content.Substring(0, match.Index);
                content = content.Substring(match.Index);
                indexInOrgContent = orgContent.IndexOf(content);
            }

            if (startToken == "\n")
            {
                startToken = @"[\n\r]+";
            }
            if (endToken == "\n")
            {
                endToken = @"[\n\r]+";
            }

            while (content.Length > 0)
            {
                string pattern = startToken + "|" + endToken;
                if (Regex.Match(content, pattern).Success)
                {
                    if (Regex.Match(content, startToken).Success && (!Regex.Match(content, endToken).Success || Regex.Match(content, startToken).Index < Regex.Match(content, endToken).Index))
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + startToken;
                        Match escapeMatch = Regex.Match(content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(content, EscapeTokenSet.DOUBLE_QUOTATION);

                        if (escapeMatch.Success && Regex.Match(content, startToken).Index > escapeMatch.Index)
                        {
                            int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            content = content.Substring(nextIndex);
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(content, startToken).Index > stringQuotationMatch.Index)
                        {
                            content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, content)[4];
                        }
                        else
                        {
                            pairCount++;
                            Match match = Regex.Match(content, startToken);
                            int nextIndex = match.Index + match.Length;

                            /* if (isPairTokenStart && content.Length > 1)
                             {//need to check
                                 string[] returnContent = FindScope(Regex.Escape(pairToken[0]), Regex.Escape(pairToken[1]), content.Substring(match.Index));
                                 if (returnContent != null )
                                 {                                    
                                     nextIndex = content.IndexOf(returnContent[4]);
                                     if (nextIndex == match.Index + match.Length)
                                     {
                                         pairCount--;
                                     }
                                 }
                             }*/
                            content = content.Substring(nextIndex);
                        }
                        indexInOrgContent = orgContent.IndexOf(content);
                    }
                    else if (Regex.Match(content, endToken).Success && (!Regex.Match(content, startToken).Success || Regex.Match(content, endToken).Index <= Regex.Match(content, startToken).Index))
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + endToken;
                        Match escapeMatch = Regex.Match(content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(content, EscapeTokenSet.DOUBLE_QUOTATION);

                        if (escapeMatch.Success && Regex.Match(content, endToken).Index > escapeMatch.Index)
                        {
                            int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            content = content.Substring(nextIndex);
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(content, endToken).Index > stringQuotationMatch.Index)
                        {
                            content = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, content)[4];
                        }
                        else
                        {
                            pairCount--;
                            Match match = Regex.Match(content, endToken);
                            int nextIndex = match.Index + match.Length;
                            if (pairCount == 0)
                            {
                                int matchStringLen = indexInOrgContent + match.Index - indexAfterStartToken;
                                matchContent = orgContent.Substring(indexAfterStartToken, matchStringLen);
                                endTokenContent = match.Value;
                                back = orgContent.Substring(indexInOrgContent + nextIndex);

                                if (isPairTokenEnd && content.Length > 1)
                                {//need to check
                                    string[] returnContent = FindScope(Regex.Escape(pairToken[0]), Regex.Escape(pairToken[1]), matchContent);
                                    if (returnContent != null && (match.Index == content.IndexOf(returnContent[2]) - match.Length || match.Index == content.IndexOf(returnContent[4]) - match.Length))
                                    {//this token is not the token want to find
                                        pairCount++;
                                        nextIndex = content.IndexOf(returnContent[4]);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else if (pairCount < 0)
                            {
                                break;
                            }
                            content = content.Substring(nextIndex);
                        }
                        indexInOrgContent = orgContent.IndexOf(content);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    return null;
                }
            }

            if (pairCount == 0)
            {
                result = new string[5];//front string , start token string  ,match string , end token string , back string
                result[0] = front;
                result[1] = startTokenContent;
                result[2] = matchContent;
                result[3] = endTokenContent;
                result[4] = back;
            }
            else
            {
                return null;
            }

            return result;        
        }

        private string[] FindSamePairScope(string token, string content)
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

    }
}
