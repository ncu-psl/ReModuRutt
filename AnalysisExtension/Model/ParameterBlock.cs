using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class ParameterBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set; }
        public int ParaListIndex { get; set; }//the id in the ruleBlock's parameterList
        public bool IsMatchRule { get; set; }
        public RuleBlock MatchRule { get; set; }

        public ParameterBlock()
        {
            Content = "";
            BlockId = -1;
            ParaListIndex = -1;
            IsMatchRule = true;
        }

        public ParameterBlock(string content)
        {
            Content = content;
            BlockId = StaticValue.GetNextBlockId();
            ParaListIndex = -1;
            IsMatchRule = true;
        }

        public ParameterBlock(string content, int paraListIndex)
        {
            Content = content;
            BlockId = StaticValue.GetNextBlockId();
            ParaListIndex = paraListIndex;
            IsMatchRule = true;
        }

        public ParameterBlock(string content, int blockId,int paraListIndex)
        {
            Content = content;
            BlockId = blockId;
            ParaListIndex = paraListIndex;
            IsMatchRule = true;
        }

        public string GetPrintInfo()
        {
            return "<para id=\"" + ParaListIndex + "\"/>";
        }

        public ICodeBlock GetCopy()
        {
            ParameterBlock copy = new ParameterBlock();

            copy.Content = Content;
            copy.BlockId = BlockId;
            copy.ParaListIndex = ParaListIndex;
            copy.IsMatchRule = IsMatchRule;
            copy.MatchRule = MatchRule;

            return copy;
        }
    }
}
