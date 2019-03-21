﻿using AnalysisExtension.Model;

namespace AnalysisExtension.AnalysisMode
{
    public class FtoC : Analysis
    {
        public FtoC(string name) : base(name)
        {
            Type.Add("f");
        }

        //override
        public override void AnalysisMethod()
        {
            System.Threading.Thread.Sleep(2000);
        }
    }
}
