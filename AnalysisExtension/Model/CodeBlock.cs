using System.Text.RegularExpressions;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class CodeBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set;  }
        public int BlockListIndex { get; set; }//the id in the ruleBlock's codeBlockList, if not the block ,then id = -1 
        public SolidColorBrush BackgroundColor { get; set; }
        public string TypeName { get { return StaticValue.CODE_BLOCK_TYPE_NAME; } }

        public bool IsMatchRule { get; set; }

        public CodeBlock()
        {
            Content = "";
            BlockId = -1;
            BlockListIndex = -1;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock(string content)
        {
            Content = content;
            BlockListIndex = -1;
            IsMatchRule = false;
            BlockId = StaticValue.GetNextBlockId();
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock(string content,int blockListIndex)
        {
            Content = content;
            BlockId = StaticValue.GetNextBlockId();
            BlockListIndex = blockListIndex;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock(string content, int blockId, int blockListIndex)
        {
            Content = content;
            BlockId = blockId;
            BlockListIndex = blockListIndex;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock GetCodeBlock()
        {
            return this;
        }

        public ParameterBlock GetParameterCodeBlock()
        {
            return null;
        }

        public string GetPrintInfo()
        {
            return Content;
        }
    }
}
