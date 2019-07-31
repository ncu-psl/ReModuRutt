using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AnalysisExtension.Model
{
    public class Analysis
    {
        public string Name { get; set; }
        public bool IsChoose { get; set; }
        public List<string> Type { get; set; }
        public string RuleFolderPath { get; set; }
        public static List<RuleBlock> ruleList = null;

        public Analysis(string name)
        {
            this.Name = name;
            this.IsChoose = false;
            Type = new List<string>();
            ruleList = new List<RuleBlock>();

            LoadRulePath();
        }

        public Analysis(string name,string type)
        {
            this.Name = "name";
            this.IsChoose = false;
            Type = new List<string>();
            Type.Add(type);
        }

        //-----set rule-----
        private void LoadRulePath()
        {
            RuleFolderPath = Path.Combine(@"..\..\Rule\", Name);
        }

        public void SetRuleList()
        {
            string[] rulePath = Directory.GetFiles(RuleFolderPath, "*.xml");

            for (int i = 0; i < rulePath.Length; i++)
            {
                string ruleText = File.ReadAllText(rulePath[i]);
                RuleBlock ruleBlock = new RuleBlock(ruleText);
                ruleList.Add(ruleBlock);
            }
        }

        //-----analysis method-----
        public virtual void AnalysisMethod()
        {//TODO : add analysis template 
            /*
             IgnoreSpace();
             GetRule();
             Compare();//with stack to separate different layer of compare code
             SaveResult();
             */
        }

      /*  private void SetLayer()
        {
            AnalysisTool analysisTool = AnalysisTool.GetInstance();
            List<ICodeBlock>[] before = analysisTool.GetFinalBeforeBlockList();

            foreach (RuleBlock ruleBlock in ruleList)
            {
                for (int fileCount = 0; fileCount < before.Length; fileCount++)
                {
                    List<ICodeBlock> beforeCodeBlock = before[fileCount];

                    for (int i = 0; i < beforeCodeBlock.Count; i++)
                    {
                        string startToken = "";
                        string endToken = "";
                        if (beforeCodeBlock[fileCount].Content.Contains("<block"))
                        {
                            if (i > 0 && i < beforeCodeBlock.Count - 1)
                            {
                                startToken = beforeCodeBlock[i - 1].Content;
                                endToken = beforeCodeBlock[i + 1].Content;
                            }

                        }
                    }
               
                }
            }
            
        }*/
 
    }
}
