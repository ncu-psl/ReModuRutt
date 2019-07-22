using System;
using System.Collections.Generic;
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

           /*  show rule content
            string text = "";
            var list = new List<ICodeBlock>(AfterRuleSliceList);
            foreach (ICodeBlock codeBlock in list.ToArray())
            {
                text += codeBlock.GetPrintInfo() + "\n";
                
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
            SplitParameterBlockFromList(ruleSliceList , ruleName + "/");
            SplitCodeBlockFromList(ruleSliceList, ruleName + "/");
        }

        private void SplitByLine(string ruleText, List<ICodeBlock> list)
        {
            string content = ruleText;

            if (content.IndexOf("\r\n") > -1 || content.IndexOf("\n") > -1 || content.IndexOf("\r") > -1)
            {
                string[] splitList = null;

                if (content.IndexOf("\r\n") > -1)
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    splitList = content.Split(stringSeparators, StringSplitOptions.None);
                }
                else if (content.IndexOf("\n") > -1)
                {
                    string[] stringSeparators = new string[] { "\n" };
                    splitList = content.Split(stringSeparators, StringSplitOptions.None);
                }
                else if (content.IndexOf("\r") > -1)
                {
                    string[] stringSeparators = new string[] { "\r" };
                    splitList = content.Split(stringSeparators, StringSplitOptions.None);
                }

                for(int i = 0; i < splitList.Length - 1; i++)
                {
                    list.Add(new CodeBlock(splitList[i]));
                }               

                content = splitList[splitList.Length - 1];
            }
            list.Add(new CodeBlock(content));//add remaining content to list
        }

        private void SplitCodeBlockFromList(List<ICodeBlock> ruleList, string layer)
        {
            var list = new List<ICodeBlock>(ruleList);
            foreach (ICodeBlock ruleCodeBlock in list.ToArray())
            {
                string content = ruleCodeBlock.GetPrintInfo();
                int blockCount = 1;// index/number of <block> in <layer>           
                int insertIndex = ruleList.IndexOf(ruleCodeBlock);

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
                        codeBlockList.Add(codeBlock);

                        ruleList.Insert(insertIndex, new CodeBlock(stringBefore));
                        insertIndex++;
                        ruleList.Insert(insertIndex, codeBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new CodeBlock(content));//add remaining content to list
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
                        paraList.Add(parameterBlock);

                        ruleList.Insert(insertIndex, new CodeBlock(stringBefore));
                        insertIndex++;
                        ruleList.Insert(insertIndex, parameterBlock);
                        insertIndex++;
                    }
                }
                ruleList.Insert(insertIndex, new CodeBlock(content));                
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

        public ParameterBlock GetParameterFromIndex(int i)
        {
            if (i < paraList.Count)
            {
                return paraList[i];
            }
            else
            {
                //throw exception?
                return null;
            }
        }

        //-----code block-----
        public void AddCodeBlock(CodeBlock codeBlock)
        {
            codeBlockList.Add(codeBlock);
        }

        public CodeBlock GetCodeBlockFromIndex(int i)
        {
            if (i < codeBlockList.Count)
            {
                return codeBlockList[i];
            }
            else
            {
                //throw exception?
                return null;
            }
        }
    }
}
