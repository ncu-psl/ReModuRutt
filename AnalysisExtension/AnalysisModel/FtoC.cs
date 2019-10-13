using AnalysisExtension.Model;
using System.Threading;

namespace AnalysisExtension.AnalysisMode
{
    public class FtoC : Analysis
    {
        public FtoC(string name) : base(name)
        {
            Type.Add("f");
        }

    }
}
