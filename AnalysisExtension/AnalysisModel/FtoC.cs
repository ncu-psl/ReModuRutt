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
        public override void AnalysisMethod()
        {
            //TODO : save result to finalAfterBlockList
            System.Threading.Thread.Sleep(2000);
        }
    }
}
