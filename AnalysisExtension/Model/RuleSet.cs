using AnalysisExtension.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalysisExtension.Model
{
    public class RuleSet : IComparable
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public List<Dictionary<string, string>> RuleList { get; set; }//List<string[]> RuleList { get; set; } //id,name
        public bool IsChoose {get;set;}

        public RuleSet(int id,string name)
        {
            IsChoose = false;
            Name = name;
            Id = id;
            Refresh();
        }
        //-----refresh-----
        public void Refresh()
        {
            RuleList = new List<Dictionary<string, string>>();//new List<string[]>();
        }
        //------add-----
        public void AddChild(string[] rule)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add("id", rule[0]);
            pairs.Add("name", rule[1]);
            RuleList.Add(pairs);    
            
        }
        //-----get-----
        public string GetRulePath(int ruleId)
        {
            Dictionary<string, string> ruleInfo = GetRuleInfoById(ruleId);
            if (ruleInfo != null)
            {
                return Name + "//" + ruleInfo["name"];
            }
            return null;
        }

        public Dictionary<string, string> GetRuleInfoById(int ruleId)
        {
            foreach (Dictionary<string, string> pairs in RuleList)
            {
                if (ruleId == int.Parse(pairs["id"]))
                {
                    return pairs;
                }
            }
            return null;
        }

        public bool HasRuleId(int id)
        {
            foreach (Dictionary<string, string> pairs in RuleList)
            {
                if (id == int.Parse(pairs["id"]))
                {
                    return true;
                }
            }
            return false;
        }

        public int GetNextRuleId()
        {
            int newId = 0;

            foreach (Dictionary<string, string> pairs in RuleList)
            {
                int id = int.Parse(pairs["id"]);
                if (id > newId)
                {
                    newId = id;
                }
            }
            newId++;

            return newId;
        }

        public void SortById()
        {
            IOrderedEnumerable<Dictionary<string,string>> result = RuleList.OrderBy(x => x["id"]);
            RuleList = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> pairs in result)
            {
                RuleList.Add(pairs);
            }
        }

        public int CompareTo(object obj)
        {
            RuleSet ruleSet = obj as RuleSet;
            if (ruleSet.Id < this.Id)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
