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

        public Analysis(string name)
        {
            this.Name = name;
            this.IsChoose = false;
            Type = new List<string>();
            LoadRulePath();
        }

        public Analysis(string name,string type)
        {
            this.Name = "name";
            this.IsChoose = false;
            Type = new List<string>();
            Type.Add(type);
        }


        private void LoadRulePath()
        {
            RuleFolderPath = Path.Combine(@"..\..\Rule\", Name);
        }

        public virtual void AnalysisMethod()
        {//TODO : add analysis template 
            /*
             IgnoreSpace();
             GetRule();
             Compare();//with stack to separate different layer of compare code
             SaveResult();
             */
        }
    }
}
