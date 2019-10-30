using AnalysisExtension.Tool;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace AnalysisExtension.Model
{
    public class RuleBlock
    {
        public int RuleId{ get; set; }
        public bool CanSpaceIgnore { get; set; }

        public List<ICodeBlock> BeforeRuleSliceList { get; set; }
        public List<ICodeBlock> AfterRuleSliceList { get; set; }

        private List<ParameterBlock> paraList;
        private List<CodeBlock> codeBlockList;

        private XmlDocument xmlDocument = new XmlDocument();

        public int ruleBlockId = StaticValue.GetNextBlockId();

        public RuleBlock(string rule)
        {
            BeforeRuleSliceList = new List<ICodeBlock>();
            AfterRuleSliceList = new List<ICodeBlock>();
            paraList = new List<ParameterBlock>();
            codeBlockList = new List<CodeBlock>();
            CanSpaceIgnore = true;

            InitRule(rule);
        }

        public RuleBlock(string rule,bool canSpaceIgnore)
        {
            BeforeRuleSliceList = new List<ICodeBlock>();
            AfterRuleSliceList = new List<ICodeBlock>();
            paraList = new List<ParameterBlock>();
            codeBlockList = new List<CodeBlock>();
            CanSpaceIgnore = canSpaceIgnore;

            InitRule(rule);
        }

        //-----ruleList-----
        private void InitRule(string rule)
        {
            xmlDocument.LoadXml(rule);

            SetRuleId();
            LoadRule("before" , BeforeRuleSliceList);
            LoadRule("after" , AfterRuleSliceList);
            ReplaceWhitespaceToRegexToken(BeforeRuleSliceList);

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
        
        private void SetRuleId()
        {
            XmlElement element = xmlDocument.DocumentElement;
            RuleId = int.Parse(GetAttributeInElement(element, "id"));
        }

        private void LoadRule(string ruleName, List<ICodeBlock> ruleSliceList)
        {
            int index = 1;
            XmlElement node = FindElementByTag(index, ruleName, "");

            SplitByLine(node.InnerXml , ruleSliceList);
           // SplitPairTokenFromList(ruleSliceList);
            SplitParameterBlockFromList(ruleSliceList , ruleName + "/");
            SplitCodeBlockFromList(ruleSliceList, ruleName + "/");            
            RemoveEmptyRuleSlice(ruleSliceList);
        }

        private void SplitByLine(string ruleText, List<ICodeBlock> list)
        {
            string content = ruleText;

            while (content.Length > 0)
            {
                Match match = Regex.Match(content, "[\r\n]+");
                if (match.Success)
                {
                    int index = match.Index + match.Length;
                    list.Add(new CodeBlock(content.Substring(0, index),ruleBlockId,-1));//add with \n\r
                    content = content.Substring(index);
                }
                else
                {
                    list.Add(new CodeBlock(content, ruleBlockId, -1));
                    break;
                }
            }
        }

        private void SplitCodeBlockFromList(List<ICodeBlock> ruleList, string layer)
        {
            var list = new List<ICodeBlock>(ruleList);
            int blockCount = 1;// index/number of <block> in <layer>      

            foreach (ICodeBlock ruleCodeBlock in list.ToArray())
            {
                string content = ruleCodeBlock.GetPrintInfo();     
                int insertIndex = ruleList.IndexOf(ruleCodeBlock);

                if (ruleCodeBlock.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                {
                    continue;
                }
                ruleList.Remove(ruleCodeBlock);//remove from list
                while (content.IndexOf("<block") > -1)
                {
                    int startIndex = content.IndexOf("<block");
                    int endIndex = content.IndexOf("/>");
                    int endTokenLen = 2;

                    string stringBefore = content.Substring(0, startIndex);
                    XmlElement blockElement = FindElementByTag(blockCount, "block", layer);
                    blockCount++;

                    string codeBlockString = content.Substring(startIndex, endIndex - startIndex + endTokenLen);
                    content = content.Substring(endIndex + endTokenLen);

                    if (blockElement == null)
                    {
                        break;
                    }
                    else
                    {
                        int codeBlockId = int.Parse(GetAttributeInElement(blockElement, "id"));
                        CodeBlock codeBlock = new CodeBlock(codeBlockString, codeBlockId);

                        ruleList.Insert(insertIndex, new CodeBlock(stringBefore, ruleBlockId, -1));
                        insertIndex++;
                        ruleList.Insert(insertIndex, codeBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new CodeBlock(content, ruleBlockId, -1));//add remaining content to list
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

                    XmlElement paraElement = FindElementByTag(paraCount, "para", layer);
                    paraCount++;

                    content = content.Substring(endIndex + endTokenLen);
                    if (paraElement == null)
                    {
                        break;
                    }
                    else
                    {
                        int paraId = int.Parse(GetAttributeInElement(paraElement, "id"));                        
                        ParameterBlock parameterBlock = new ParameterBlock("", paraId);

                        ruleList.Insert(insertIndex, new CodeBlock(stringBefore, ruleBlockId, -1));
                        insertIndex++;
                        ruleList.Insert(insertIndex, parameterBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new CodeBlock(content, ruleBlockId, -1));                
            }
        }

        private void SplitPairTokenFromList(List<ICodeBlock> ruleList)
        {
            var list = new List<ICodeBlock>(ruleList);

            foreach (ICodeBlock ruleCodeBlock in list.ToArray())
            {
                string content = ruleCodeBlock.GetPrintInfo();
                int insertIndex = ruleList.IndexOf(ruleCodeBlock);

                ruleList.Remove(ruleCodeBlock);//remove from list
                                
                if (EscapeTokenSet.GetPairTokenIndex(content) > -1)
                {
                    int startIndex = EscapeTokenSet.GetPairTokenIndex(content);
                    string token = EscapeTokenSet.GetToken(content);

                    string stringBefore = content.Substring(0, startIndex);
                    content = content.Substring(startIndex + token.Length);
                    
                    ruleList.Insert(insertIndex, new CodeBlock(stringBefore, ruleBlockId, -1));
                    insertIndex++;
                    ruleList.Insert(insertIndex, new CodeBlock(token, ruleBlockId, -1));
                    insertIndex++;                    
                }
                ruleList.Insert(insertIndex, new CodeBlock(content, ruleBlockId, -1));
            }
        }

        private void RemoveEmptyRuleSlice(List<ICodeBlock> list)
        {
            foreach (ICodeBlock ruleSlice in list.ToArray())
            {
                if (!(ruleSlice.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME)) && !(ruleSlice.Content.Contains("<block")))
                {
                    Match match = Regex.Match(ruleSlice.Content, @"[\S]");
                    if (!match.Success || ruleSlice.Content.Length == 0)
                    {
                        list.Remove(ruleSlice);
                    }
                }
            }
        }

        private void ReplaceWhitespaceToRegexToken(List<ICodeBlock> list)
        {///replace \n, \r, \t, and " " to regex simbol , and escape those content not include  in \n, \r, \t, \f, and " " 
            string whitespacePattern = @"[ \t]+";
            string linePattern = @"[\n\r]+";
            for(int i = 0; i < list.Count;i++)//foreach (ICodeBlock ruleSlice in list)
            {
                ICodeBlock ruleSlice = list[i];
                if (!(ruleSlice.TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME)) && !(ruleSlice.Content.Contains("<block")))
                {
                    MatchCollection matches = Regex.Matches(ruleSlice.Content, @"[\S]");//not include 
                    foreach (Match match in matches)
                    {
                        ruleSlice.Content = ruleSlice.Content.Replace(match.Value, Regex.Escape(match.Value));
                    }

                    MatchCollection whitespaceMatches = Regex.Matches(ruleSlice.Content, whitespacePattern);
                    int indexShift = 0;
                    foreach (Match match in whitespaceMatches)
                    {
                        string changePattern = "";
                        int actualIndex = match.Index + indexShift;
                        if (actualIndex == 0)
                        {//whitespace at first
                            if (i == 0)
                            {//not have front ruleSlice
                                changePattern = @"\b[ \t]*";
                            }
                            else if (list[i - 1].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                            {
                                if (Regex.Match(list[i].Content[actualIndex + match.Length].ToString(), @"\w").Success)
                                {
                                    changePattern = @"[ \t]+";
                                }
                                else
                                {
                                    changePattern = @"[ \t]*";
                                }
                            }
                            else if (Regex.Match(list[i - 1].Content, @"(\w)\Z").Success)
                            {
                                changePattern = @"[ \t]+";
                            }
                            else if (Regex.Match(list[i - 1].Content, @"(\W)\Z").Success || list[i - 1].Content.Contains("<block"))
                            {
                                changePattern = @"[ \t]*";
                            }
                        }
                        else if (actualIndex == ruleSlice.Content.Length - 1)
                        {//whitespace at end
                            if (i == list.Count - 1)
                            {//is last ruleSlice
                                changePattern = @"[ \t]*\b";
                            }
                            else if (list[i + 1].TypeName.Equals(StaticValue.PARAMETER_BLOCK_TYPE_NAME))
                            {
                                int contentLen = list[i].Content.Length;
                                if (Regex.Match(list[i].Content[actualIndex - 1].ToString(), @"\w").Success)
                                {
                                    changePattern = @"[ \t]+";
                                }
                                else
                                {
                                    changePattern = @"[ \t]*";
                                }
                            }
                            else if (Regex.Match(list[i + 1].Content, @"\A(\w)").Success)
                            {
                                changePattern = @"[ \t]+";
                            }
                            else if (Regex.Match(list[i + 1].Content, @"\A(\W)").Success || list[i + 1].Content.Contains("<block"))
                            {
                                changePattern = @"[ \t]*";
                            }
                        }
                        else
                        {//whitespace in the middle
                            if (actualIndex > 0 && actualIndex < ruleSlice.Content.Length - 1)
                            {
                                if ((Regex.Match(ruleSlice.Content[actualIndex - 1].ToString(), @"[\w]").Success || Regex.Match(ruleSlice.Content[actualIndex + 1].ToString(), @"[\w]").Success) &&
                                    !(Regex.Match(ruleSlice.Content[actualIndex - 1].ToString(), @"[\W]").Success || Regex.Match(ruleSlice.Content[actualIndex + 1].ToString(), @"[\W]").Success ))
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
                    ruleSlice.Content = Regex.Replace(ruleSlice.Content, linePattern, @"[\n\r]*");//+ or * ?
                }
            }
        }
               
        //-----xml tool-----
        private XmlElement FindElementByTag(int index,string tag,string layer)
        {
            return (XmlElement)xmlDocument.DocumentElement.SelectSingleNode(layer+tag+"["+index+"]");
        }

        private string GetAttributeInElement(XmlElement element, string attributeName)
        {
            if (element.HasAttribute(attributeName))
            {
                return element.GetAttribute(attributeName);
            }
            return null;
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
        
        public CodeBlock GetCodeBlockrById(int id)
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

        //-----init-----

        public void InitRuleSetting()
        {
            InitCodeBlockList();
            InitParameterList();
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

    }
}
