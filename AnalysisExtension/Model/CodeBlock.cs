using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class CodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }

        public CodeBlock()
        {
            Content = "";
            BlockId = -1;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }

        public CodeBlock(string content,int id)
        {
            this.Content = content;
            this.BlockId = id;
            BackgroundColor = new SolidColorBrush(Colors.White);
        }
    }
}
