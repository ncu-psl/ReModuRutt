using System;
using System.Collections.Generic;

namespace AnalysisExtension.Model
{
    public class Analysis
    {
        public string Name { get; set; }
        public bool IsChoose { get; set; }
        public List<string> Type { get; set; }

        public Analysis(string name)
        {
            this.Name = name;
            this.IsChoose = false;
            Type = new List<string>();
        }

        public Analysis(string name,string type)
        {
            this.Name = "name";
            this.IsChoose = false;
            Type = new List<string>();
            Type.Add(type);
        }

        public bool AnalysisMethod()
        {
            bool result = false;

            return result;
        }
    }
}
