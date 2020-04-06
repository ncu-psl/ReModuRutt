using System.Text.RegularExpressions;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class NormalBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set;  }
        public SolidColorBrush BackgroundColor { get; set; }
        public bool IsMatchRule { get; set; }

        public NormalBlock()
        {
            Content = "";
            BlockId = -1;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public NormalBlock(string content)
        {
            Content = content;
            IsMatchRule = false;
            BlockId = StaticValue.GetNextBlockId();
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public NormalBlock(string content, int blockId)
        {
            Content = content;
            BlockId = blockId;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
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
            copy.BackgroundColor = BackgroundColor;
            copy.IsMatchRule = IsMatchRule;

            return copy;
        }
    }
}
