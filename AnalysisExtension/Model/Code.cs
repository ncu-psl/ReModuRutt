using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisExtension.Model
{
    public class Code
    {
        public string CodeBefore { get; set; }
        public string CodeAfter { get; set; }
        public bool IsDiff { get; set; }
    }
}
