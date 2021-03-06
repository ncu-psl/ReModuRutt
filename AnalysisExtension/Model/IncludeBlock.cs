﻿using System.Collections.Generic;
using System.Windows.Media;

namespace AnalysisExtension.Model
{
    public class IncludeBlock : ICodeBlock
    {
        public string Content { get; set; }
        public int BlockId { get; set; }
        public int IncludeBlockListIndex { get; set; }//the id in the ruleBlock's includeBlockList, if not the block ,then id = -1 
        public int CompareRuleId { get; set; }
        public int FromRuleSetId { get; set; } //if == -1, include rule from same rule set
        public List<ICodeBlock> BeforeList { get; set; }
        public List<ICodeBlock> AfterList { get; set; }
        public bool IsMatchRule { get; set; }
        public RuleBlock MatchRule { get ; set; }

        public IncludeBlock()
        {
            Content = "";
            BlockId = StaticValue.GetNextBlockId();
            IncludeBlockListIndex = -1;
            IsMatchRule = false;
        }

        public IncludeBlock(string content,int compareRuleId,int fromRuleSetId)
        {
            Content = content;
            IncludeBlockListIndex = -1;
            IsMatchRule = false;
            BlockId = StaticValue.GetNextBlockId();
            CompareRuleId = compareRuleId;
            FromRuleSetId = fromRuleSetId;
        }

        public IncludeBlock(string content, int includeBlockListIndex, int compareRuleId, int fromRuleSetId)
        {
            Content = content;
            BlockId = StaticValue.GetNextBlockId();
            IncludeBlockListIndex = includeBlockListIndex;
            IsMatchRule = false;
            CompareRuleId = compareRuleId;
            FromRuleSetId = fromRuleSetId;
        }

        public IncludeBlock(string content, int blockId, int includeBlockListIndex, int compareRuleId, int fromRuleSetId)
        {
            Content = content;
            BlockId = blockId;
            IncludeBlockListIndex = includeBlockListIndex;
            IsMatchRule = false;
            CompareRuleId = compareRuleId;
            FromRuleSetId = fromRuleSetId;
        }

        public string GetPrintInfo()
        {
            return "<include id=\"" + IncludeBlockListIndex + "\" compareRuleId=\"" + CompareRuleId + "\" fromRuleSetId=\"" + FromRuleSetId+"\"/>";
        }

        public ICodeBlock GetCopy()
        {
            IncludeBlock copy = new IncludeBlock();

            copy.Content = Content;
            copy.BlockId = BlockId;
            copy.IncludeBlockListIndex = IncludeBlockListIndex;
            copy.CompareRuleId = CompareRuleId;
            copy.FromRuleSetId = FromRuleSetId;
            copy.BeforeList = StaticValue.CopyList(BeforeList);
            copy.AfterList = StaticValue.CopyList(AfterList);
            copy.IsMatchRule = IsMatchRule;
            copy.MatchRule = MatchRule;
            return copy;
        }
    }
}
