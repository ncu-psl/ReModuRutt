﻿using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class CodeBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set;  }
        public int LayerId { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
        public string TypeName => StaticValue.CODE_BLOCK_TYPE_NAME;
        
        public CodeBlock()
        {
            Content = "";
            BlockId = -1;
            LayerId = -1;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock(string content)
        {
            Content = content;
            LayerId = -1;
            BackgroundColor = new SolidColorBrush(Colors.White);
            SetBlockId();
        }

        public CodeBlock(string content,int id)
        {
            Content = content;
            BlockId = id;
            LayerId = -1;
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

        public void SetBlockId()
        {
            BlockId = StaticValue.CODE_BLOCK_ID_COUNT;
            StaticValue.CODE_BLOCK_ID_COUNT++;
        }
    }
}
