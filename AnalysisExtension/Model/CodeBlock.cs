using System.Collections.Generic;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class CodeBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set; }
        public int BlockListIndex { get; set; }//the id in the ruleBlock's codeBlockList, if not the block ,then id = -1 
        public SolidColorBrush BackgroundColor { get; set; }
        public List<ICodeBlock> BeforeList { get; set; }
        public List<ICodeBlock> AfterList { get; set; }
        public bool IsMatchRule { get; set; }

        public CodeBlock()
        {
            Content = "";
            BlockId = -1;
            BlockListIndex = -1;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
            BeforeList = new List<ICodeBlock>();
            AfterList = new List<ICodeBlock>();
        }

        public CodeBlock(string content)
        {
            Content = content;
            BlockListIndex = -1;
            IsMatchRule = false;
            BlockId = StaticValue.GetNextBlockId();
            BackgroundColor = new SolidColorBrush(Colors.White);
            BeforeList = new List<ICodeBlock>();
            AfterList = new List<ICodeBlock>();
        }

        public CodeBlock(string content, int blockListIndex)
        {
            Content = content;
            BlockId = StaticValue.GetNextBlockId();
            BlockListIndex = blockListIndex;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
            BeforeList = new List<ICodeBlock>();
            AfterList = new List<ICodeBlock>();
        }

        public CodeBlock(string content, int blockId, int blockListIndex)
        {
            Content = content;
            BlockId = blockId;
            BlockListIndex = blockListIndex;
            IsMatchRule = false;
            BackgroundColor = new SolidColorBrush(Colors.White);
            BeforeList = new List<ICodeBlock>();
            AfterList = new List<ICodeBlock>();
        }

        public string GetPrintInfo()
        {
            return Content;
        }

        public ICodeBlock GetCopy()
        {
            CodeBlock copy = new CodeBlock();

            copy.Content = Content;
            copy.BlockId = BlockId;
            copy.BlockListIndex = BlockListIndex;
            copy.BackgroundColor = BackgroundColor;
            copy.BeforeList = BeforeList;
            copy.AfterList = AfterList;
            copy.IsMatchRule = IsMatchRule;

            return copy;
        }

        public List<ICodeBlock> ExpandBeforeList()
        {
            foreach (ICodeBlock codeBlock in BeforeList.ToArray())
            {
                if (codeBlock is CodeBlock && (codeBlock as CodeBlock).BeforeList.Count > 0)
                {
                    int index = BeforeList.IndexOf(codeBlock);
                    BeforeList.RemoveAt(index);
                    BeforeList.InsertRange(index,(codeBlock as CodeBlock).ExpandBeforeList());
                }
            }

            return BeforeList;
        }

        public List<ICodeBlock> ExpandAfterList()
        {
            foreach (ICodeBlock codeBlock in AfterList.ToArray())
            {
                if (codeBlock is CodeBlock && (codeBlock as CodeBlock).AfterList.Count > 0)
                {
                    int index = AfterList.IndexOf(codeBlock);
                    AfterList.RemoveAt(index);
                    AfterList.InsertRange(index, (codeBlock as CodeBlock).ExpandAfterList());
                }
            }

            return AfterList;
        }
    }
}
