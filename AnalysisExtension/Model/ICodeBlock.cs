using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public interface ICodeBlock
    {
        string Content { get; set; }
        int BlockId { get; set; }
        int LayerId { get; set; }
        SolidColorBrush BackgroundColor { get; set; }
        string TypeName { get; }

        CodeBlock GetCodeBlock();
        ParameterBlock GetParameterCodeBlock();
        string GetPrintInfo();
        void SetBlockId();
    }
}
