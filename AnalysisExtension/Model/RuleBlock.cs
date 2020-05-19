using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace AnalysisExtension.Model
{
    public class RuleBlock
    {
        public string RuleName { get; set; }
        public int RuleId{ get; set; }
        public bool CanSpaceIgnore { get; set; }

        public List<ICodeBlock> BeforeRuleSliceList { get; set; }
        public List<ICodeBlock> AfterRuleSliceList { get; set; }

        private List<ParameterBlock> paraList;
        private List<CodeBlock> codeBlockList;
        private List<IncludeBlock> includeBlockList;

        private XmlDocument xmlDocument = new XmlDocument();

        public int ruleBlockId = StaticValue.GetNextBlockId();

        public RuleBlock(RuleBlock copy)
        {
            RuleName = copy.RuleName;
            RuleId = copy.RuleId;
            CanSpaceIgnore = copy.CanSpaceIgnore;

            BeforeRuleSliceList = new List<ICodeBlock>();
            foreach (ICodeBlock codeBlock in copy.BeforeRuleSliceList)
            {
                BeforeRuleSliceList.Add(codeBlock.GetCopy());
            }

            AfterRuleSliceList = new List<ICodeBlock>();
            foreach (ICodeBlock codeBlock in copy.AfterRuleSliceList)
            {
                AfterRuleSliceList.Add(codeBlock.GetCopy());
            }

            InitRuleSetting();
        }

        public RuleBlock(string rule)
        {
            BeforeRuleSliceList = new List<ICodeBlock>();
            AfterRuleSliceList = new List<ICodeBlock>();
            paraList = new List<ParameterBlock>();
            codeBlockList = new List<CodeBlock>();
            includeBlockList = new List<IncludeBlock>();
            CanSpaceIgnore = false;

            InitRule(rule);
        }

        public RuleBlock(string rule,bool canSpaceIgnore)
        {
            BeforeRuleSliceList = new List<ICodeBlock>();
            AfterRuleSliceList = new List<ICodeBlock>();
            paraList = new List<ParameterBlock>();
            codeBlockList = new List<CodeBlock>();
            includeBlockList = new List<IncludeBlock>();
            CanSpaceIgnore = canSpaceIgnore;

            InitRule(rule);
        }
        //-----get text----
        public string GetOrgText(string tag)
        {
            string orgText = StaticValue.GetXmlTextByTag(xmlDocument, tag);

            if (orgText.StartsWith("\r\n"))
            {
                orgText = orgText.Remove(0, 2);
            }
            else if (orgText.StartsWith("\n"))
            {
                orgText = orgText.Remove(0, 1);
            }

            if (orgText.EndsWith("\r\n"))
            {
                int len = orgText.Length;
                orgText = orgText.Remove(orgText.Length - 2, 2);
            }
            else if (orgText.EndsWith("\n"))
            {
                int len = orgText.Length;
                orgText = orgText.Remove(orgText.Length - 1, 1);
            }
           
            return orgText;
        }

        //-----ruleList-----
        private void InitRule(string rule)
        {
            xmlDocument.LoadXml(rule);
            SetRuleInfo();
            LoadRule("before" , BeforeRuleSliceList);
            LoadRule("after" , AfterRuleSliceList);
            ReplaceTokenToRegex(BeforeRuleSliceList);

            /*  show rule content
             string text = "";
             var list = new List<ICodeBlock>(BeforeRuleSliceList);
             foreach (ICodeBlock codeBlock in list.ToArray())
             {
                 text += codeBlock.BlockId + " : " + codeBlock.GetPrintInfo() + "\n";
             }
             MessageBox.Show(text);
            */
        }

        private void SetRuleInfo()
        {
            XmlElement element = xmlDocument.DocumentElement;
            RuleId = int.Parse(StaticValue.GetAttributeInElement(element, "id"));
            RuleName = StaticValue.GetAttributeInElement(element, "name");
            CanSpaceIgnore = bool.Parse(StaticValue.GetAttributeInElement(element, "canWhitespaceIgnore"));
        }

        private void LoadRule(string ruleName, List<ICodeBlock> ruleSliceList)
        {
            SpiltByEscapeToken(GetOrgText(ruleName),ruleSliceList);
            SplitParameterBlockFromList(ruleSliceList , ruleName + "/");
            SplitCodeBlockFromList(ruleSliceList, ruleName + "/");
            SplitIncludeBlockFromList(ruleSliceList, ruleName + "/");
            RemoveEmptyRuleSlice(ruleSliceList);
            if (ruleSliceList.Count > 0 && !(ruleSliceList[ruleSliceList.Count - 1] is NormalBlock))
            {
                ruleSliceList.Add(new NormalBlock("\n",ruleBlockId));
            }

            foreach (ICodeBlock codeBlock in ruleSliceList)
            {
                codeBlock.Content = StaticValue.ReplaceXmlToken(codeBlock.Content);
            }
        }

        private void SpiltByEscapeToken(string ruleText, List<ICodeBlock> ruleSliceList)
        {
            string content = ruleText;
            ruleSliceList.AddRange(EscapeTokenSet.SpiltByEscapeToken(content));
        }

        private void SplitCodeBlockFromList(List<ICodeBlock> ruleList, string layer)
        {
            //codeBlockIdPairList = new List<Dictionary<int, string>>();
            var list = new List<ICodeBlock>(ruleList);
            int blockCount = 1;// index/number of <block> in <layer>      

            foreach (ICodeBlock ruleCodeBlock in list.ToArray())
            {
                string content = ruleCodeBlock.GetPrintInfo();     
                int insertIndex = ruleList.IndexOf(ruleCodeBlock);

                if (ruleCodeBlock is ParameterBlock)
                {
                    continue;
                }
                ruleList.Remove(ruleCodeBlock);//remove from list
                while (content.IndexOf("<block") > -1)
                {
                    int startIndex = content.IndexOf("<block");
                    int endIndex = content.Substring(startIndex).IndexOf("/>") + startIndex;
                    int endTokenLen = 2;

                    string stringBefore = content.Substring(0, startIndex);
                    XmlElement blockElement = StaticValue.FindElementByTag(xmlDocument,blockCount, "block", layer);
                    blockCount++;

                    string codeBlockString = content.Substring(startIndex, endIndex - startIndex + endTokenLen);
                    content = content.Substring(endIndex + endTokenLen);

                    if (blockElement == null)
                    {
                        break;
                    }
                    else
                    {
                        if (stringBefore.Length > 0)
                        {
                            ruleList.Insert(insertIndex, new NormalBlock(stringBefore, ruleBlockId));
                            insertIndex++;
                        }

                        int codeBlockId = int.Parse(StaticValue.GetAttributeInElement(blockElement, "id"));
                        CodeBlock codeBlock = new CodeBlock(codeBlockString, codeBlockId);
                        
                        ruleList.Insert(insertIndex, codeBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new NormalBlock(content, ruleBlockId));//add remaining content to list
            }
        }

        private void SplitParameterBlockFromList(List<ICodeBlock> ruleList, string layer)
        {
            var list = new List<ICodeBlock>(ruleList);
            int paraCount = 1;// index/number of <para> in <layer>

            foreach (ICodeBlock ruleCodeBlock in list.ToArray())
            {
                string content = ruleCodeBlock.GetPrintInfo();
                int insertIndex = ruleList.IndexOf(ruleCodeBlock);

                ruleList.Remove(ruleCodeBlock);//remove from list

                while (content.IndexOf("<para") > -1)
                {
                    int startIndex = content.IndexOf("<para");
                    int endIndex = content.IndexOf("/>");
                    int endTokenLen = 2;

                    string stringBefore = content.Substring(0, startIndex);

                    XmlElement paraElement = StaticValue.FindElementByTag(xmlDocument,paraCount, "para", layer);
                    paraCount++;

                    content = content.Substring(endIndex + endTokenLen);
                    if (paraElement == null)
                    {
                        break;
                    }
                    else
                    {
                        int paraId = int.Parse(StaticValue.GetAttributeInElement(paraElement, "id"));                        
                        ParameterBlock parameterBlock = new ParameterBlock("", paraId);

                        ruleList.Insert(insertIndex, new NormalBlock(stringBefore, ruleBlockId));
                        insertIndex++;
                        ruleList.Insert(insertIndex, parameterBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new NormalBlock(content, ruleBlockId));                
            }
        }

        private void SplitIncludeBlockFromList(List<ICodeBlock> ruleList, string layer)
        {
            var list = new List<ICodeBlock>(ruleList);
            int includeBlockCount = 1;// index/number of <para> in <layer>

            foreach (ICodeBlock ruleCodeBlock in list.ToArray())
            {
                string content = ruleCodeBlock.GetPrintInfo();
                int insertIndex = ruleList.IndexOf(ruleCodeBlock);

                if (ruleCodeBlock is ParameterBlock || ruleCodeBlock.Content.Contains("<block"))
                {
                    continue;
                }
                ruleList.Remove(ruleCodeBlock);//remove from list
                while (content.IndexOf("<include") > -1)
                {
                    int startIndex = content.IndexOf("<include");
                    int endIndex = content.Substring(startIndex).IndexOf("/>") + startIndex;
                    int endTokenLen = 2;

                    string stringBefore = content.Substring(0, startIndex);
                    XmlElement blockElement = StaticValue.FindElementByTag(xmlDocument, includeBlockCount, "include", layer);
                    includeBlockCount++;

                    string codeBlockString = content.Substring(startIndex, endIndex - startIndex + endTokenLen);
                    content = content.Substring(endIndex + endTokenLen);

                    if (blockElement == null)
                    {
                        break;
                    }
                    else
                    {
                        if (stringBefore.Length > 0)
                        {
                            ruleList.Insert(insertIndex, new NormalBlock(stringBefore, ruleBlockId));
                            insertIndex++;
                        }
                        int codeBlockId = int.Parse(StaticValue.GetAttributeInElement(blockElement, "id"));
                        int compareRuleId = int.Parse(StaticValue.GetAttributeInElement(blockElement, "compareRuleId"));
                        int fromRuleSetId= int.Parse(StaticValue.GetAttributeInElement(blockElement, "fromRuleSetId"));

                        IncludeBlock codeBlock = new IncludeBlock(codeBlockString, codeBlockId, compareRuleId, fromRuleSetId);
                        ruleList.Insert(insertIndex, codeBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new NormalBlock(content, ruleBlockId));//add remaining content to list
            }
        }
        
        private void RemoveEmptyRuleSlice(List<ICodeBlock> list)
        {
            foreach (ICodeBlock ruleSlice in list.ToArray())
            {
                if (ruleSlice is NormalBlock)
                {
                    if (ruleSlice.Content.Length == 0)
                    {
                        list.Remove(ruleSlice);
                    }
                }
            }
        }

        private void ReplaceTokenToRegex(List<ICodeBlock> list)
        {
            for (int i = 0; i < list.Count;i++)
            {
                ICodeBlock ruleSlice = list[i];                

                if (ruleSlice is NormalBlock)
                {
                    MatchCollection matches = Regex.Matches(ruleSlice.Content, @"[\S]");//not include 
                    foreach (Match match in matches)
                    {//escape those content not include  in \n, \r, \t, \f, and " " 
                        ruleSlice.Content = ruleSlice.Content.Replace(match.Value, Regex.Escape(match.Value));
                    }

                    if (CanSpaceIgnore)
                    {//replace \n, \r, \t, and " " to regex simbol
                        ReplaceWhitespaceToRegex(i,list);
                    }
                }
            }
        }

        private void ReplaceWhitespaceToRegex(int ruleSliceCount, List<ICodeBlock> list)
        {
            ICodeBlock ruleSlice = list[ruleSliceCount];
            string whitespacePattern = @"[ \t]+";
            string linePattern = @"[\n\r]+";
            MatchCollection whitespaceMatches = Regex.Matches(ruleSlice.Content, whitespacePattern);
            int indexShift = 0;
            foreach (Match match in whitespaceMatches)
            {
                string changePattern = "";
                int actualIndex = match.Index + indexShift;
                if (actualIndex == 0)
                {//whitespace at first
                    if (ruleSliceCount == 0)
                    {//not have front ruleSlice
                        changePattern = "";//@"\b[ \t]*";
                    }
                    else if (list[ruleSliceCount - 1] is ParameterBlock)
                    {
                        if (Regex.Match(list[ruleSliceCount].Content[actualIndex + match.Length].ToString(), @"\w").Success)
                        {
                            changePattern = @"[ \t]+";
                        }
                        else
                        {
                            changePattern = @"[ \t]*";
                        }
                    }
                    else if (Regex.Match(list[ruleSliceCount - 1].Content, @"(\w)\Z").Success)
                    {
                        changePattern = @"[ \t]+";
                    }
                    else if (Regex.Match(list[ruleSliceCount - 1].Content, @"(\W)\Z").Success || list[ruleSliceCount - 1] is CodeBlock)
                    {
                        changePattern = @"[ \t]*";
                    }
                }
                else if (actualIndex == ruleSlice.Content.Length - 1)
                {//whitespace at end
                    if (ruleSliceCount == list.Count - 1)
                    {//is last ruleSlice
                        changePattern = @"[ \t]*\b";
                    }
                    else if (list[ruleSliceCount + 1] is ParameterBlock)
                    {
                        int contentLen = list[ruleSliceCount].Content.Length;
                        if (Regex.Match(list[ruleSliceCount].Content[actualIndex - 1].ToString(), @"\w").Success)
                        {
                            changePattern = @"[ \t]+";
                        }
                        else
                        {
                            changePattern = @"[ \t]*";
                        }
                    }
                    else if (Regex.Match(list[ruleSliceCount + 1].Content, @"\A(\w)").Success)
                    {
                        changePattern = @"[ \t]+";
                    }
                    else if (Regex.Match(list[ruleSliceCount + 1].Content, @"\A(\W)").Success || list[ruleSliceCount + 1].Content.Contains("<block"))
                    {
                        changePattern = @"[ \t]*";
                    }
                }
                else
                {//whitespace in the middle
                    if (actualIndex > 0 && actualIndex < ruleSlice.Content.Length - 1)
                    {
                        if ((Regex.Match(ruleSlice.Content[actualIndex - 1].ToString(), @"[\w]").Success || Regex.Match(ruleSlice.Content[actualIndex + 1].ToString(), @"[\w]").Success) &&
                            !(Regex.Match(ruleSlice.Content[actualIndex - 1].ToString(), @"[\W]").Success || Regex.Match(ruleSlice.Content[actualIndex + 1].ToString(), @"[\W]").Success))
                        {
                            changePattern = @"[ \t]+";
                        }
                        else
                        {
                            changePattern = @"[ \t]*";
                        }
                    }
                    else if (list.IndexOf(ruleSlice) == 0 && actualIndex == 0)
                    {//start at whitespace
                        changePattern = @"\b[ \t]*";
                    }
                    else
                    {
                        changePattern = @"[ \t]*";
                    }
                }

                ruleSlice.Content = ruleSlice.Content.Remove(actualIndex, match.Length);
                ruleSlice.Content = ruleSlice.Content.Insert(actualIndex, changePattern);
                indexShift = indexShift - match.Length + changePattern.Length;
            }

             ruleSlice.Content = Regex.Replace(ruleSlice.Content, linePattern, @"[\s]*");//@"[\n\r]*"+ or * ?
        }

        
        //-----para block-----
        public void AddParameter(ParameterBlock parameterBlock)
        {
            paraList.Add(parameterBlock);
        }      
        
        public ParameterBlock GetParameterById(int id)
        {            
            foreach (ParameterBlock parameter in paraList)
            {
                if (parameter.ParaListIndex == id)
                {
                    return parameter;
                }
            }

            return null;
        }

        //-----code block-----
        public void AddCodeBlock(CodeBlock codeBlock)
        {
            codeBlockList.Add(codeBlock);
        }

        public CodeBlock GetCodeBlockById(int id)
        {
            foreach (CodeBlock codeBlock in codeBlockList)
            {
                if (codeBlock.BlockListIndex == id)
                {
                    return codeBlock;
                }
            }
            return null;
        }
        
        //-----include block-----
        public void AddIncludeBlock(IncludeBlock includeBlock)
        {
            includeBlockList.Add(includeBlock);
        }

        public IncludeBlock GetIncludeBlockById(int id)
        {
            foreach (IncludeBlock includeBlock in includeBlockList)
            {
                if (includeBlock.IncludeBlockListIndex == id)
                {
                    return includeBlock;
                }
            }
            return null;
        }

        public bool IsIncludeBlockSame(IncludeBlock org,IncludeBlock compare)
        {
            return StaticValue.IsListSame(org.BeforeList, compare.BeforeList,CanSpaceIgnore) && StaticValue.IsListSame(org.AfterList, compare.AfterList,CanSpaceIgnore);

        }

        public bool IsCodeBlockSame(CodeBlock org, CodeBlock compare)
        {
            return StaticValue.IsListSame(org.BeforeList, compare.BeforeList, CanSpaceIgnore) && StaticValue.IsListSame(org.AfterList, compare.AfterList, CanSpaceIgnore);

        }
        //-----init-----

        public void InitRuleSetting()
        {
            InitCodeBlockList();
            InitParameterList();
            InitIncludeList();

            int maxId = -1;
            int needToAdd = StaticValue.GetNextBlockId() - BeforeRuleSliceList[0].BlockId;

            foreach (ICodeBlock codeBlock in BeforeRuleSliceList)
            {
                codeBlock.BlockId = codeBlock.BlockId + needToAdd;
                if (maxId < codeBlock.BlockId)
                {
                    maxId = codeBlock.BlockId;
                }
            }
            foreach (ICodeBlock codeBlock in AfterRuleSliceList)
            {
                codeBlock.BlockId = codeBlock.BlockId + needToAdd;
                if (maxId < codeBlock.BlockId)
                {
                    maxId = codeBlock.BlockId;
                }
            }

            StaticValue.CODE_BLOCK_ID_COUNT = maxId;
        }

        private void InitCodeBlockList()
        {
            codeBlockList = new List<CodeBlock>();
        }

        private void InitParameterList()
        {
            paraList = new List<ParameterBlock>();
        }

        private void InitIncludeList()
        {
            includeBlockList = new List<IncludeBlock>();
        }

        public ICodeBlock GetFirstNormalBlock()
        {
            ICodeBlock normalBlock = BeforeRuleSliceList[0];

            while (!(normalBlock is NormalBlock))
            {
                if (normalBlock is IncludeBlock)
                {
                    string rulePath = RuleMetadata.GetInstance().GetRulePathById((normalBlock as IncludeBlock).FromRuleSetId, (normalBlock as IncludeBlock).CompareRuleId);
                    RuleBlock findRule = FileLoader.GetInstance().LoadSingleRuleByPath(rulePath);
                    normalBlock = findRule.GetFirstNormalBlock();
                }
                else
                {
                    normalBlock = new NormalBlock("\n");
                }
            }

            return normalBlock;
        }
    }
}
