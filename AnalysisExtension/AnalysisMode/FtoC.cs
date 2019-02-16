using AnalysisExtension.Model;
using System;
using System.Collections.Generic;

namespace AnalysisExtension.AnalysisMode
{
    public class FtoC : Analysis
    {
        public FtoC(string name) : base(name)
        {
            Type.Add("f");
        }

        //override
        public new bool AnalysisMethod()
        {
            bool result = false;

            return result;
        }
    }
}
