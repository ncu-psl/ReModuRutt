using AnalysisExtension.Model;

namespace AnalysisExtension.AnalysisMode
{
    public class FtoC : Analysis
    {
        public FtoC(string name) : base(name)
        {
            Type.Add("f");
        }

        //override
        public new void AnalysisMethod()
        {

        }
    }
}
