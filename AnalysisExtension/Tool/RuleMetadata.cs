using AnalysisExtension.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;


namespace AnalysisExtension.Tool
{
    public class RuleMetadata
    {
        private static RuleMetadata instanceMetadata = null;
        private static List<RuleSet> ruleSetList = null;
        private FileLoader fileLoader = FileLoader.GetInstance();
        private string metadataPath = StaticValue.RULE_FOLDER_PATH + "\\metadata.xml";

        private RuleMetadata()
        {
        }

        public static RuleMetadata GetInstance()
        {
            if (instanceMetadata == null)
            {
                instanceMetadata = new RuleMetadata();
            }
            instanceMetadata.Refresh();
            return instanceMetadata;
        }
        //-----refresh metadata-----
        public void Refresh()
        {
            ruleSetList = new List<RuleSet>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(FileLoader.GetInstance().GetFileText(metadataPath));
            XmlElement element = xmlDocument.DocumentElement;
            XmlNodeList elementList = element.GetElementsByTagName("ruleSet");
            for (int i = 0; i < elementList.Count; i++)
            {                
                int id = int.Parse(StaticValue.GetAttributeInElement((XmlElement)elementList[i], "id"));
                string name = StaticValue.GetAttributeInElement((XmlElement)elementList[i], "name");
                RuleSet ruleSet = new RuleSet(id, name);
                XmlNodeList ruleList = ((XmlElement)elementList[i]).GetElementsByTagName("rule");
                for (int j = 0; j < ruleList.Count; j++)
                {
                    string[] rule = new string[2];
                    rule[0] = StaticValue.GetAttributeInElement((XmlElement)ruleList[j], "id");
                    rule[1] = StaticValue.GetAttributeInElement((XmlElement)ruleList[j], "name");
                    ruleSet.AddChild(rule);
                }
                ruleSetList.Add(ruleSet);
            }
            Sort();
        }

        public void RewriteMetadata()
        {
            File.WriteAllText(metadataPath, GetMetadata());
        }
        //-----tool-----
        private string GetMetadata()
        {
            string result = "<metadata>\n";
            foreach (RuleSet ruleSet in ruleSetList)
            {
                //<ruleSet name="FtoC" id="1">
                result += "<ruleSet name=" + "\"" + ruleSet.Name + "\" id=" + "\"" + ruleSet.Id + "\">\n";
                foreach (Dictionary<string,string> pairs in ruleSet.RuleList)
                {
                    //<rule name = "dowhile" id = "2"/>
                    result += "    <rule name=" +"\"" + pairs["name"] + "\" id=" + "\"" + pairs["id"] + "\"/>\n";
                }
                result += @"</ruleSet>"+"\n";
            }
            result += @"</metadata>";

            return result;
        }

        //-----sort-----
        public void Sort()
        {
            foreach (RuleSet ruleSet in ruleSetList)
            {
                ruleSet.SortById();
            }
            ruleSetList.Sort();
        }
        //-----add-----
        public void AddRuleIntoRuleSet(int ruleSetId, int ruleId,string ruleName)
        {
            RuleSet ruleSet = GetRuleSetById(ruleSetId);
            if (!ruleSet.HasRuleId(ruleId))
            {
                ruleSet.AddChild(new string[]{ ruleId.ToString(),ruleName});
            }            
        }

        public void AddRuleSet(RuleSet ruleSet)
        {
            ruleSetList.Add(ruleSet);
            RewriteMetadata();
            Refresh();
        }

        //-----id count-----
        public int GetNextRuleIdByRuleSetId(int ruleSetId)
        {
            foreach (RuleSet ruleSet in ruleSetList)
            {
                if (ruleSet.Id.Equals(ruleSetId))
                {
                    return ruleSet.GetNextRuleId();
                }
            }
            return -1;
        }

        public int GetNextRuleSetId()
        {
            return ruleSetList[ruleSetList.Count - 1].Id + 1;
        }

        //-----get-----
        public List<RuleSet> GetRuleSetList()
        {
            return ruleSetList;
        }

        public RuleSet GetRuleSetByName(string name)
        {
            foreach (RuleSet ruleSet in ruleSetList)
            {
                if (ruleSet.Name.Equals(name))
                {
                    return ruleSet;
                }
            }
            return null;
        }

        public RuleSet GetRuleSetById(int index)
        {
            foreach (RuleSet ruleSet in ruleSetList)
            {
                if (ruleSet.Id== index)
                {
                    return ruleSet;
                }
            }
            return null;
        }
    }
}
