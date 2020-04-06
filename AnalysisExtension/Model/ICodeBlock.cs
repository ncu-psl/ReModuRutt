﻿using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public interface ICodeBlock
    {
        string Content { get; set; }
        int BlockId { get; set; }
        SolidColorBrush BackgroundColor { get; set; }
        bool IsMatchRule { get; set; }

        ICodeBlock GetCopy();
        string GetPrintInfo();
    }
}
