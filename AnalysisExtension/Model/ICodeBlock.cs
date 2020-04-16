using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public interface ICodeBlock
    {
        string Content { get; set; }
        int BlockId { get; set; }
        bool IsMatchRule { get; set; }
        RuleBlock MatchRule { get; set; }

        ICodeBlock GetCopy();
        string GetPrintInfo();
    }
}
