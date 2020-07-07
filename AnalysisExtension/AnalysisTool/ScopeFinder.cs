using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnalysisExtension.AnalysisTool
{
    public class ScopeFinder
    {
        public string StartRuleSlice { get; set; }
        public string EndRuleSlice { get; set; }

        public ScopeFinder(RuleBlock ruleBlockUseNow)
        {
            int endRuleIndex = ruleBlockUseNow.BeforeRuleSliceList.Count - 1;
            StartRuleSlice = ruleBlockUseNow.BeforeRuleSliceList[0].Content;
            EndRuleSlice = ruleBlockUseNow.BeforeRuleSliceList[endRuleIndex].Content;
        }

        public List<ICodeBlock>[] FindScope(string startToken, string endToken, List<ICodeBlock> orgList)//if cannot find , return null
        {
            List<ICodeBlock> contentList = StaticValue.CopyList(orgList);
            List<ICodeBlock>[] result = null;//front string , start token string  ,match string , end token string , back string

            bool isMatch = false;
            string front = "";
            string startTokenContent = "";
            string matchContent = "";
            string endTokenContent = "";
            string back = "";

            int pairCount = 0;
            bool isPairTokenStart = false;
            bool isPairTokenEnd = false;
            string[] pairToken = new string[2];

            result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>();
            }


            if (startToken == null)
            {
                return FindSingleEndTokenScope(endToken, contentList);
            }

            if (endToken == null)
            {
                return FindSingleStartTokenScope(startToken, contentList);
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
                return FindSamePairScope(startToken, orgList);
            }

            startToken = Regex.Replace(startToken, @"[\n\r]+", "[\\n\\r]+");
            endToken = Regex.Replace(endToken, @"[\n\r]+", "[\\n\\r]+");
            if (startToken == "[ \\t]*[\\n\\r]*" || startToken == "[\\n\\r]*[ \\t]*" || startToken == "[\\n\\r]*" || startToken == "[\\s]*")
            {
                startToken = "[\\n\\r]+";
            }

            if (endToken == "[ \\t]*[\\n\\r]*" || endToken == "[\\n\\r]*[ \\t]*" || endToken == "[\\n\\r]*" || endToken == "[\\s]*")
            {
                endToken = "[\\n\\r]+";
            }

            for (int i = 0; i < contentList.Count; i++)
            {
                string orgContent = contentList[i].Content;
                if (isMatch)
                {
                    result[4].Add(contentList[i]);
                    continue;
                }

                string pattern = startToken + "|" + endToken;
                if (contentList[i].Content.Length > 0 && Regex.Match(contentList[i].Content, pattern).Success)
                {
                    while (contentList[i].Content.Length > 0)
                    {
                        if (!Regex.Match(StartRuleSlice, startToken).Success && !startToken.Equals(StartRuleSlice) && Regex.Match(contentList[i].Content, StartRuleSlice).Success && Regex.Match(contentList[i].Content, StartRuleSlice).Index == 0)
                        {
                            List<ICodeBlock>[] blockSkip = FindScope(StartRuleSlice, EndRuleSlice, contentList);
                            if (blockSkip != null)
                            {
                                for (int j = 1; j < 4; j++)
                                {
                                    blockSkip[0].AddRange(blockSkip[j]);
                                }

                                if (pairCount == 0)
                                {
                                    result[0].AddRange(blockSkip[0]);
                                }
                                else
                                {
                                    result[2].AddRange(blockSkip[0]);
                                }

                                contentList = blockSkip[4];
                                i = -1;
                                break;
                            }
                        }
                        else if (Regex.Match(contentList[i].Content, startToken).Success &&
                            (!Regex.Match(contentList[i].Content, endToken).Success || Regex.Match(contentList[i].Content, startToken).Index < Regex.Match(contentList[i].Content, endToken).Index))
                        {//startToken
                            string escapePattern = EscapeTokenSet.BACKSLASH + startToken;
                            Match escapeMatch = Regex.Match(contentList[i].Content, escapePattern);
                            Match stringQuotationMatch = Regex.Match(contentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);
                            Match commentMatch = Regex.Match(contentList[i].Content, EscapeTokenSet.COMMENT_START_TOKEN);

                            if (escapeMatch.Success && Regex.Match(contentList[i].Content, startToken).Index > escapeMatch.Index)
                            {
                                int nextIndex = escapeMatch.Index + escapeMatch.Length;
                                contentList[i].Content = contentList[i].Content.Substring(nextIndex);
                            }
                            else if (stringQuotationMatch.Success && Regex.Match(contentList[i].Content, startToken).Index > stringQuotationMatch.Index)
                            {
                                List<ICodeBlock>[] innerResult = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, contentList.GetRange(i, contentList.Count - 1 - i));
                                if (pairCount == 0)
                                {
                                    for (int innerIndex = 0; innerIndex < 4; innerIndex++)
                                    {
                                        result[0].AddRange(innerResult[innerIndex]);
                                    }
                                    contentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                                }
                                else if (isMatch)
                                {
                                    for (int innerIndex = 0; innerIndex < 5; innerIndex++)
                                    {
                                        result[4].AddRange(innerResult[innerIndex]);
                                    }
                                    break;
                                }
                                else
                                {
                                    for (int innerIndex = 0; innerIndex < 5; innerIndex++)
                                    {
                                        result[2].AddRange(innerResult[innerIndex]);
                                    }
                                    break;
                                }
                            }
                            else if (commentMatch.Value.Length > 0 && !startToken.Equals(EscapeTokenSet.COMMENT_START_TOKEN) && Regex.Match(contentList[i].Content, startToken).Index > commentMatch.Index)
                            {
                                List<ICodeBlock>[] innerResult = FindScope(EscapeTokenSet.COMMENT_START_TOKEN, EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));//FindSingleEndTokenScope(EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));

                                result[0].AddRange(innerResult[0]);
                                result[0].AddRange(innerResult[1]);
                                result[0].AddRange(innerResult[2]);
                                result[0].AddRange(innerResult[3]);
                                contentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                            }
                            else
                            {
                                Match match = Regex.Match(contentList[i].Content, startToken);
                                int nextIndex = match.Index + match.Length;

                                if (pairCount == 0)
                                {
                                    front = contentList[i].Content.Substring(0, match.Index);
                                    result[0].AddRange(EscapeTokenSet.SpiltByEscapeToken(front));
                                    startTokenContent = match.Value;
                                    result[1].Add(new NormalBlock(startTokenContent));
                                }
                                else
                                {
                                    matchContent = contentList[i].Content.Substring(0, nextIndex);
                                    result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                }
                                pairCount++;
                                contentList[i].Content = contentList[i].Content.Substring(nextIndex);

                            }
                        }
                        else if (Regex.Match(contentList[i].Content, endToken).Success
                            && (!Regex.Match(contentList[i].Content, startToken).Success || Regex.Match(contentList[i].Content, endToken).Index <= Regex.Match(contentList[i].Content, startToken).Index))
                        {
                            string escapePattern = EscapeTokenSet.BACKSLASH + endToken;
                            Match escapeMatch = Regex.Match(contentList[i].Content, escapePattern);
                            Match stringQuotationMatch = Regex.Match(contentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);
                            Match commentMatch = Regex.Match(contentList[i].Content, EscapeTokenSet.COMMENT_START_TOKEN);
                            if (escapeMatch.Success && Regex.Match(contentList[i].Content, endToken).Index > escapeMatch.Index)
                            {
                                int nextIndex = escapeMatch.Index + escapeMatch.Length;
                                contentList[i].Content = contentList[i].Content.Substring(nextIndex);
                            }
                            else if (stringQuotationMatch.Success && Regex.Match(contentList[i].Content, endToken).Index > stringQuotationMatch.Index)
                            {
                                List<ICodeBlock>[] innerResult = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, contentList.GetRange(i, contentList.Count - 1 - i));
                                if (pairCount == 0)
                                {
                                    for (int innerIndex = 0; innerIndex < 4; innerIndex++)
                                    {
                                        result[0].AddRange(innerResult[innerIndex]);
                                    }
                                    contentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                                }
                                else if (isMatch)
                                {
                                    for (int innerIndex = 0; innerIndex < 5; innerIndex++)
                                    {
                                        result[4].AddRange(innerResult[innerIndex]);
                                    }
                                    break;
                                }
                                else
                                {
                                    for (int innerIndex = 0; innerIndex < 5; innerIndex++)
                                    {
                                        result[2].AddRange(innerResult[innerIndex]);
                                    }
                                    break;
                                }
                            }
                            else if (commentMatch.Value.Length > 0 && !startToken.Equals(EscapeTokenSet.COMMENT_START_TOKEN) && Regex.Match(contentList[i].Content, endToken).Index > commentMatch.Index)
                            {
                                List<ICodeBlock>[] innerResult = FindScope(EscapeTokenSet.COMMENT_START_TOKEN, EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));//FindSingleEndTokenScope(EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));

                                result[0].AddRange(innerResult[0]);
                                result[0].AddRange(innerResult[1]);
                                result[0].AddRange(innerResult[2]);
                                result[0].AddRange(innerResult[3]);
                                contentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                            }
                            else
                            {
                                pairCount--;
                                Match match = Regex.Match(contentList[i].Content, endToken);
                                int nextIndex = match.Index + match.Length;
                                matchContent = contentList[i].Content.Substring(0, match.Index);
                                if (pairCount == 0)
                                {
                                    result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                    endTokenContent = match.Value;
                                    result[3].Add(new NormalBlock(endTokenContent));
                                    back = contentList[i].Content.Substring(nextIndex);
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
                                    result[0].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                    endTokenContent = match.Value;
                                    result[0].Add(new NormalBlock(endTokenContent));
                                    contentList[i].Content = contentList[i].Content.Substring(nextIndex);
                                    pairCount = 0;
                                }
                                else
                                {
                                    result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                    result[2].Add(new NormalBlock(match.Value));
                                    contentList[i].Content = contentList[i].Content.Substring(nextIndex);
                                }
                            }
                        }
                        else
                        {
                            if (pairCount == 0)
                            {
                                result[0].Add(contentList[i]);
                            }
                            else
                            {
                                result[2].Add(contentList[i]);
                            }
                            break;
                        }
                    }

                }
                else
                {
                    if (pairCount == 0)
                    {//is content before start token
                        result[0].Add(contentList[i]);
                    }
                    else
                    {
                        result[2].Add(contentList[i]);
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

        public List<ICodeBlock>[] FindSamePairScope(string token, List<ICodeBlock> orgContentList)
        {
            List<ICodeBlock>[] result = new List<ICodeBlock>[5];//front string , start token string  ,match string , end token string , back string
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>();
            }
            string front = "";
            string matchContent = "";
            string back = "";

            int pairCount = 0;
            bool isMatch = false;

            if (!Regex.Match(StaticValue.GetAllContent(orgContentList), token).Success)
            {//cannot find , not match
                return null;
            }

            //find pair
            for (int i = 0; i < orgContentList.Count; i++)
            {
                // string orgContent = orgContentList[i].Content;
                if (isMatch)
                {
                    result[4].Add(orgContentList[i]);
                    continue;
                }

                if (orgContentList[i].Content.Length > 0 && Regex.Match(orgContentList[i].Content, token).Success)
                {
                    while (orgContentList[i].Content.Length > 0)
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + token;
                        Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);
                        Match commentMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.COMMENT_START_TOKEN);

                        if (escapeMatch.Success)
                        {
                            int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                        }
                        else if (stringQuotationMatch.Success && !token.Equals(EscapeTokenSet.DOUBLE_QUOTATION))
                        {
                            List<ICodeBlock>[] innerResult = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList.GetRange(i, orgContentList.Count - 1 - i));
                            if (pairCount == 0)
                            {
                                for (int innerIndex = 0; innerIndex < 4; innerIndex++)
                                {
                                    result[0].AddRange(innerResult[innerIndex]);
                                }
                                orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                            }
                            else if (isMatch)
                            {
                                for (int innerIndex = 0; innerIndex < 5; innerIndex++)
                                {
                                    result[4].AddRange(innerResult[innerIndex]);
                                }
                                break;
                            }
                            else
                            {
                                for (int innerIndex = 0; innerIndex < 5; innerIndex++)
                                {
                                    result[2].AddRange(innerResult[innerIndex]);
                                }
                                break;
                            }
                        }
                        else if (commentMatch.Value.Length > 0 && !token.Equals(EscapeTokenSet.COMMENT_START_TOKEN) && Regex.Match(orgContentList[i].Content, token).Index > commentMatch.Index)
                        {
                            List<ICodeBlock>[] innerResult = FindScope(EscapeTokenSet.COMMENT_START_TOKEN, EscapeTokenSet.COMMENT_END_TOKEN, orgContentList.GetRange(i, orgContentList.Count - 1 - i));//FindSingleEndTokenScope(EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));

                            result[0].AddRange(innerResult[0]);
                            result[0].AddRange(innerResult[1]);
                            result[0].AddRange(innerResult[2]);
                            result[0].AddRange(innerResult[3]);
                            orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                        }
                        else if (Regex.Match(orgContentList[i].Content, token).Success)
                        {
                            Match match = Regex.Match(orgContentList[i].Content, token);
                            int nextIndex = match.Index + match.Length;
                            pairCount++;
                            if (pairCount == 1)
                            {
                                front = orgContentList[i].Content.Substring(0, match.Index);
                                result[0].AddRange(EscapeTokenSet.SpiltByEscapeToken(front));
                                result[1].Add(new NormalBlock(token));
                            }
                            else if (pairCount == 2)
                            {
                                matchContent = orgContentList[i].Content.Substring(0, match.Index);
                                result[2].AddRange(EscapeTokenSet.SpiltByEscapeToken(matchContent));
                                back = orgContentList[i].Content.Substring(nextIndex);
                                result[3].Add(new NormalBlock(token));
                                result[4].AddRange(EscapeTokenSet.SpiltByEscapeToken(back));
                                isMatch = true;
                                break;

                            }
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
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
                }

            }
            return result;
        }

        public List<ICodeBlock>[] FindSingleStartTokenScope(string startToken, List<ICodeBlock> orgContentList)
        {//2+3
            List<ICodeBlock>[] result = new List<ICodeBlock>[5];//front string , start token string  ,match string , X, X
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>();
            }

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
                    if (!startToken.Equals(StartRuleSlice) && (!Regex.Match(orgContentList[i].Content, startToken).Success ||
                        (Regex.Match(orgContentList[i].Content, startToken).Success && Regex.Match(orgContentList[i].Content, StartRuleSlice).Index < Regex.Match(orgContentList[i].Content, startToken).Index)))
                    {
                        List<ICodeBlock>[] blockSkip = FindScope(StartRuleSlice, EndRuleSlice, orgContentList);
                        if (blockSkip != null)
                        {
                            for (int j = 1; j < 4; j++)
                            {
                                blockSkip[0].AddRange(blockSkip[j]);
                            }
                            result[0].AddRange(blockSkip[0]);
                            orgContentList = blockSkip[4];
                            i = -1;
                        }
                        else
                        {
                            result[0].Add(orgContentList[i]);
                        }
                        break;
                    }
                    else if (Regex.Match(orgContentList[i].Content, startToken).Success)
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + startToken;
                        Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);
                        Match commentMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.COMMENT_START_TOKEN);

                        if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > escapeMatch.Index)
                        {
                            int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, startToken).Index > stringQuotationMatch.Index)
                        {
                            List<ICodeBlock>[] innerResult = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList.GetRange(i, orgContentList.Count - 1 - i));
                            int insertIndex = 0;
                            if (isMatch)
                            {
                                insertIndex = 2;
                            }
                            result[insertIndex].AddRange(innerResult[0]);
                            result[insertIndex].AddRange(innerResult[1]);
                            result[insertIndex].AddRange(innerResult[2]);
                            result[insertIndex].AddRange(innerResult[3]);
                            orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                        }
                        else if (commentMatch.Value.Length > 0 && !startToken.Equals(EscapeTokenSet.COMMENT_START_TOKEN) && Regex.Match(orgContentList[i].Content, startToken).Index > commentMatch.Index)
                        {
                            int insertIndex = 0;
                            if (isMatch)
                            {
                                insertIndex = 2;
                            }
                            List<ICodeBlock>[] innerResult = FindScope(EscapeTokenSet.COMMENT_START_TOKEN, EscapeTokenSet.COMMENT_END_TOKEN, orgContentList.GetRange(i, orgContentList.Count - 1 - i));//FindSingleEndTokenScope(EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));

                            orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                            result[insertIndex].AddRange(innerResult[0]);
                            result[insertIndex].AddRange(innerResult[1]);
                            result[insertIndex].AddRange(innerResult[2]);
                            result[insertIndex].AddRange(innerResult[3]);
                            orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
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

        public List<ICodeBlock>[] FindSingleEndTokenScope(string endToken, List<ICodeBlock> orgContentList)
        {//1+2
            List<ICodeBlock>[] result = new List<ICodeBlock>[5];//X, X, match string , end token string , back string
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<ICodeBlock>();
            }
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
                //string orgContent = orgContentList[i].Content;

                if (isMatch)
                {
                    result[4].Add(orgContentList[i]);
                    continue;
                }

                while (orgContentList[i].Content.Length > 0)
                {
                    if (Regex.Match(orgContentList[i].Content, StartRuleSlice).Success && (!Regex.Match(orgContentList[i].Content, endToken).Success ||
                        (Regex.Match(orgContentList[i].Content, endToken).Success && Regex.Match(orgContentList[i].Content, StartRuleSlice).Index < Regex.Match(orgContentList[i].Content, endToken).Index)))
                    {
                        List<ICodeBlock>[] blockSkip = FindScope(StartRuleSlice, EndRuleSlice, orgContentList);
                        if (blockSkip != null)
                        {
                            for (int j = 1; j < 4; j++)
                            {
                                blockSkip[0].AddRange(blockSkip[j]);
                            }
                            result[2].AddRange(blockSkip[0]);
                            orgContentList = blockSkip[4];
                            i = -1;
                        }
                        else
                        {
                            result[2].Add(orgContentList[i]);
                        }
                        break;
                    }
                    else if (Regex.Match(orgContentList[i].Content, endToken).Success)
                    {
                        string escapePattern = EscapeTokenSet.BACKSLASH + endToken;
                        Match escapeMatch = Regex.Match(orgContentList[i].Content, escapePattern);
                        Match stringQuotationMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.DOUBLE_QUOTATION);
                        Match commentMatch = Regex.Match(orgContentList[i].Content, EscapeTokenSet.COMMENT_START_TOKEN);

                        if (escapeMatch.Success && Regex.Match(orgContentList[i].Content, endToken).Index > escapeMatch.Index)
                        {
                            int nextIndex = escapeMatch.Index + escapeMatch.Length;
                            orgContentList[i].Content = orgContentList[i].Content.Substring(nextIndex);
                        }
                        else if (stringQuotationMatch.Success && Regex.Match(orgContentList[i].Content, endToken).Index > stringQuotationMatch.Index)
                        {
                            List<ICodeBlock>[] innerResult = FindSamePairScope(EscapeTokenSet.DOUBLE_QUOTATION, orgContentList.GetRange(i, orgContentList.Count - 1 - i));
                            int insertIndex = 2;
                            if (isMatch)
                            {
                                insertIndex = 4;
                            }
                            result[insertIndex].AddRange(innerResult[0]);
                            result[insertIndex].AddRange(innerResult[1]);
                            result[insertIndex].AddRange(innerResult[2]);
                            result[insertIndex].AddRange(innerResult[3]);
                            orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                        }
                        else if (commentMatch.Value.Length > 0 && !endToken.Equals(EscapeTokenSet.COMMENT_START_TOKEN) && Regex.Match(orgContentList[i].Content, endToken).Index > commentMatch.Index)
                        {
                            int insertIndex = 2;
                            if (isMatch)
                            {
                                insertIndex = 4;
                            }
                            List<ICodeBlock>[] innerResult = FindScope(EscapeTokenSet.COMMENT_START_TOKEN, EscapeTokenSet.COMMENT_END_TOKEN, orgContentList.GetRange(i, orgContentList.Count - 1 - i));//FindSingleEndTokenScope(EscapeTokenSet.COMMENT_END_TOKEN, contentList.GetRange(i, contentList.Count - 1 - i));

                            result[insertIndex].AddRange(innerResult[0]);
                            result[insertIndex].AddRange(innerResult[1]);
                            result[insertIndex].AddRange(innerResult[2]);
                            result[insertIndex].AddRange(innerResult[3]);
                            orgContentList[i].Content = StaticValue.GetAllContent(innerResult[4]);
                        }
                        else
                        {
                            Match match = Regex.Match(orgContentList[i].Content, endToken);
                            int backIndex = match.Index + match.Length/* + indexShift*/;
                            string matchContent = orgContentList[i].Content.Substring(0, match.Index);
                            result[2].Add(new NormalBlock(matchContent));

                            result[3].Add(new NormalBlock(match.Value));
                            orgContentList[i].Content = orgContentList[i].Content.Substring(backIndex);
                            result[4].Add(orgContentList[i]);
                            isMatch = true;
                            break;
                        }
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

        public string[] GetStartTokenAndEndToken(List<ICodeBlock> ruleSlice, int ruleSliceIndex)
        {
            string startToken = null, endToken = null;
            int endRuleIndex = ruleSlice.Count - 1;
            int orgIndex = ruleSliceIndex;

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
                    if (ruleSlice[ruleSliceIndex + 1] is IncludeBlock)
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

            ruleSliceIndex = orgIndex;
            if (ruleSliceIndex == ruleSlice.Count - 2 && (endToken.Equals("[\\s]+") || endToken.Equals("[\\s]*")))
            {//if endToken is last rule slice
                endToken = "[\\n\\r]*";
            }

            while (endToken != null && (endToken.Equals("[\\n\\r]*") || endToken.Equals("[\\n\\r]+") || endToken.Equals("[\\s]+") || endToken.Equals("[\\s]*")) && ruleSliceIndex < ruleSlice.Count - 1)
            {
                ruleSliceIndex++;
                string[] find = GetStartTokenAndEndToken(ruleSlice, ruleSliceIndex);
                endToken += find[1];
            }

            return new string[] { startToken, endToken };
        }

    }
}
