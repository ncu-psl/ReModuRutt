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
        public Color BackgroundColor { get; set; }
    }
}
