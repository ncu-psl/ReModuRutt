using System.Text.RegularExpressions;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class NormalBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set;  }
        public bool IsMatchRule { get; set; }
        public RuleBlock MatchRule { get; set; }

        public NormalBlock()
        {
            Content = "";
            BlockId = -1;
            IsMatchRule = false;
        }

        public NormalBlock(string content)
        {
            Content = content;
            IsMatchRule = false;
            BlockId = StaticValue.GetNextBlockId();
        }

        public NormalBlock(string content, int blockId)
        {
            Content = content;
            BlockId = blockId;
            IsMatchRule = false;
        }

        public string GetPrintInfo()
        {
            return Content;
        }

        public ICodeBlock GetCopy()
        {
            NormalBlock copy = new NormalBlock();
            copy.Content = Content;
            copy.BlockId = BlockId;
            copy.IsMatchRule = IsMatchRule;
            copy.MatchRule = MatchRule;
            return copy;
        }
    }
}
