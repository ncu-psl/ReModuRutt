﻿using System;
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
            SplitByParameter(node.InnerXml, ruleSliceList, ruleName + "/");
            SplitCodeBlockFromList(ruleSliceList, ruleName + "/");
        }
        
        private void SplitCodeBlockFromList(List<ICodeBlock> ruleList,string layer)
        {
            var list = new List<ICodeBlock>(ruleList);
            foreach (ICodeBlock codeBlock in list.ToArray())
            {
                SplitByCodeBlock(codeBlock, ruleList, layer);
            }

            list = new List<ICodeBlock>(ruleList);
            foreach (ICodeBlock codeBlock in list.ToArray())
            {
                SplitByLine(codeBlock, ruleList);
            }
            //ruleList = list;
        }

        private void SplitByParameter(string rule, List<ICodeBlock> list, string layer)
        {
            string content = rule;
            int count = 1;// index/number of <para> in <layer>

            while (content.IndexOf("<para") > -1)
            {
                int startIndex = content.IndexOf("<para");
                int endIndex = content.IndexOf("/>");
                int endTokenLen = 2;

                string stringBefore = content.Substring(0,startIndex);

                XmlElement paraElement = FindElementByTag(count,"para",layer);
                count++;

                content = content.Substring(endIndex + endTokenLen);
                if (paraElement == null)
                {
                    break;
                }
                else
                {
                    int paraId = int.Parse(GetAttributeInElement(paraElement, "id"));
                    ParameterBlock parameterBlock = new ParameterBlock("",paraId);
                    paraList.Add(parameterBlock);

                    list.Add(new CodeBlock(stringBefore));
                    list.Add(parameterBlock);
                }                
            }
            list.Add(new CodeBlock(content));
        }

        private void SplitByCodeBlock(ICodeBlock ruleCodeBlock, List<ICodeBlock> list, string layer)
        {
            string content = ruleCodeBlock.GetPrintInfo();
            int count = 1;// index/number of <block> in <layer>           
            int insertIndex = list.IndexOf(ruleCodeBlock);

            list.Remove(ruleCodeBlock);//remove from list
            while (content.IndexOf("<block") > -1)
            {
                int startIndex = content.IndexOf("<block");
                int endIndex = content.IndexOf("/>");
                int endTokenLen = 2;

                string stringBefore = content.Substring(0, startIndex);
                XmlElement blockElement = FindElementByTag(count, "block", layer);
                count++;

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

                    list.Insert(insertIndex, new CodeBlock(stringBefore));
                    insertIndex++;
                    list.Insert(insertIndex, codeBlock);
                    insertIndex++;
                }
            }
            list.Insert(insertIndex, new CodeBlock(content));//add remaining content to list
        }

        private void SplitByLine(ICodeBlock ruleCodeBlock, List<ICodeBlock> list)
        {
            string content = ruleCodeBlock.GetPrintInfo(); 
            int insertIndex = list.IndexOf(ruleCodeBlock);

            list.Remove(ruleCodeBlock);//remove from list
            while (content.IndexOf("\r\n") > -1 || content.IndexOf("\n") > -1 || content.IndexOf("\r") > -1)
            {                
                string[] splitList = null;

                if (content.IndexOf("\r\n") > -1)
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    splitList = content.Split(stringSeparators,StringSplitOptions.None);
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
                    list.Insert(insertIndex, new CodeBlock(splitList[i]));
                    insertIndex++;
                }
                content = splitList[splitList.Length - 1];
            }
            list.Insert(insertIndex, new CodeBlock(content));//add remaining content to list
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
