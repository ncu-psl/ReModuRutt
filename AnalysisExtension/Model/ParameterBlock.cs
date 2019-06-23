using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class ParameterBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set; }
        public int LayerId { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
        public string TypeName => StaticValue.PARAMETER_BLOCK_TYPE_NAME;

        public ParameterBlock()
        {
            Content = "";
            BlockId = -1;
            LayerId = -1;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public ParameterBlock(string content)
        {
            Content = content;
            LayerId = -1;
            BackgroundColor = new SolidColorBrush(Colors.White);
            SetBlockId();
        }

        public ParameterBlock(string content, int id)
        {
            Content = content;
            BlockId = id;
            LayerId = -1;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock GetCodeBlock()
        {
            return null;
        }

        public ParameterBlock GetParameterCodeBlock()
        {
            return this;
        }

        public string GetPrintInfo()
        {
            return "<$" + BlockId + ">";
        }

        public void SetBlockId()
        {
            BlockId = StaticValue.PARAMETER_BLOCK_TYPE_ID_COUNT;
            StaticValue.PARAMETER_BLOCK_TYPE_ID_COUNT++;
        }
    }
}
