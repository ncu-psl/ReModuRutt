using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AnalysisExtension.View.AnalysisView
{
    public partial class EditTransformResultWindowControl : UserControl
    {
        public bool isAnalysisSuccess = false;

        private RuleMetadata ruleMetadata = RuleMetadata.GetInstance();
        private RuleBlock ruleBlockUseNow = null;
        private List<ICodeBlock> beforeContent = null;
        private ICodeBlock codeBlockChooseNow = null;
        private int fileIndex;

        private SolidColorBrush changeColor = new SolidColorBrush(Colors.LightSkyBlue);
        private SolidColorBrush orgBackgroundColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush orgForegroundColor = new SolidColorBrush(Colors.Black);
        
        private ICodeBlock ruleSliceChooseNow = null;
        private List<Run> chooseRange = new List<Run>();
        private int chooseStartIndex = -1;
        private int chooseContentCount = 0;

        public EditTransformResultWindowControl(List<ICodeBlock> beforeContent,ICodeBlock codeBlockChooseNow,int fileIndex)
        {
            InitializeComponent();
            this.beforeContent = StaticValue.CopyList(beforeContent);
            this.ruleBlockUseNow = codeBlockChooseNow.MatchRule;
            this.codeBlockChooseNow = codeBlockChooseNow;
            this.fileIndex = fileIndex;

            Refresh();

            foreach (Paragraph paragraph in beforeEditBox.Document.Blocks)
            {
                foreach (Run run in paragraph.Inlines)
                {
                    ICodeBlock codeBlock = run.DataContext as ICodeBlock;
                    if (codeBlock.BlockId == codeBlockChooseNow.BlockId)
                    {
                        run.Background = changeColor;
                    }
                }
            }
        }

        private void Refresh()
        {
            isAnalysisSuccess = false;
            chooseRange = new List<Run>();
            SetRuleList();
            if (ruleBlockUseNow != null)
            {
                SetBeforeRule();
                SetAfterRule();
            }
            SetBeforeEditBox();

            SizeChanged += OnSizeChanged;
        }
        //-----rule-----
        private void SetBeforeRule()
        {
            StackPanel outer = new StackPanel() { Orientation = Orientation.Vertical };
            StackPanel line = new StackPanel() { Orientation = Orientation.Horizontal };
            foreach (ICodeBlock codeBlock in ruleBlockUseNow.BeforeRuleSliceList)
            {
                if (codeBlock is NormalBlock)
                {
                    string content = codeBlock.Content;
                    while (content.Contains("[\\s]"))
                    {
                        string pattern = "[\\s]";
                        if (content.Contains("[\\s]+"))
                        {
                            pattern += "+";
                        }
                        else if(content.Contains("[\\s]*"))
                        {
                            pattern += "*";
                        }

                        int index = content.IndexOf(pattern);

                        if (content.Substring(0, index).Length > 0)
                        {
                            string front = RemoveRegexToken(content.Substring(0, index));
                            Button button = SetEditRuleButton(front, codeBlock);
                            line.Children.Add(button);
                        }

                        outer.Children.Add(line);
                        line = new StackPanel() { Orientation = Orientation.Horizontal };


                        if (index + pattern.Length < content.Length)
                        {
                            content = content.Substring(index + pattern.Length);
                        }
                        else
                        {//is last
                            content = "";
                            break;
                        }
                    }

                    if (content.Length > 0)
                    {
                        content = RemoveRegexToken(content);
                        Button back = SetEditRuleButton(content, codeBlock);
                        line.Children.Add(back);
                    }
                }
                else if (codeBlock is ParameterBlock)
                {
                    string content = "parameter " + (codeBlock as ParameterBlock).ParaListIndex;
                    Button button = SetEditRuleButton(content, codeBlock);
                    button.Foreground = SystemColors.HighlightBrush;
                    line.Children.Add(button);
                }
                else if (codeBlock is CodeBlock)
                {
                    string content = "code block " + (codeBlock as CodeBlock).BlockListIndex;
                    Button button = SetEditRuleButton(content,codeBlock);
                    button.Background = SystemColors.HighlightBrush;
                    line.Children.Add(button);
                }
                else if (codeBlock is IncludeBlock)
                {
                    IncludeBlock includeBlock = codeBlock as IncludeBlock;

                    RuleSet ruleSet = ruleMetadata.GetRuleSetById(includeBlock.FromRuleSetId);
                    string content = "( " + includeBlock.IncludeBlockListIndex + " ) by rule set " + ruleSet.Name + ", rule " + ruleSet.GetRuleInfoById(includeBlock.CompareRuleId)["name"];

                    Button button = SetEditRuleButton(content, codeBlock);
                    button.Background = SystemColors.InactiveCaptionBrush;
                    line.Children.Add(button);
                }
            }
            outer.Children.Add(line);

            beforeRuleScrollView.Content = outer;
        }

        private void SetAfterRule()
        {
            StackPanel outer = new StackPanel() { Orientation = Orientation.Vertical };
            StackPanel line = new StackPanel() { Orientation = Orientation.Horizontal };
            foreach (ICodeBlock codeBlock in ruleBlockUseNow.AfterRuleSliceList)
            {
                if (codeBlock is NormalBlock)
                {
                    string content = codeBlock.Content;
                    if (codeBlock.Content.Contains("\n"))
                    {
                        string pattern = "\n";
                        int index = content.IndexOf(pattern);

                        if (content.Substring(0, index).Length > 0)
                        {
                            string front = RemoveRegexToken(content.Substring(0, index));
                            line.Children.Add(SetAfterRuleTextBlock(front,codeBlock));
                        }
                        outer.Children.Add(line);
                        line = new StackPanel() { Orientation = Orientation.Horizontal };
                        if (index + pattern.Length < content.Length)
                        {
                            content = content.Substring(index + pattern.Length);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    content = RemoveRegexToken(content);
                    line.Children.Add(SetAfterRuleTextBlock(content,codeBlock));
                }
                else if (codeBlock is ParameterBlock)
                {
                    string content = "parameter " + (codeBlock as ParameterBlock).ParaListIndex;
                    TextBlock para = SetAfterRuleTextBlock(content, codeBlock);
                    para.Foreground = SystemColors.HighlightBrush;
                    line.Children.Add(para);
                }
                else if (codeBlock is CodeBlock)
                {
                    string content = "code block " + (codeBlock as CodeBlock).BlockListIndex;
                    TextBlock block = SetAfterRuleTextBlock(content, codeBlock);
                    block.Background = SystemColors.HighlightBrush;
                    line.Children.Add(block);
                }
                else if (codeBlock is IncludeBlock)
                {
                    IncludeBlock includeBlock = codeBlock as IncludeBlock;

                    RuleSet ruleSet = ruleMetadata.GetRuleSetById(includeBlock.FromRuleSetId);
                    string content = "( " + includeBlock.IncludeBlockListIndex + " ) by rule set " + ruleSet.Name + ", rule " + ruleSet.GetRuleInfoById(includeBlock.CompareRuleId)["name"];

                    TextBlock include = SetAfterRuleTextBlock(content, codeBlock);
                    include.Background = SystemColors.InactiveCaptionBrush;
                    line.Children.Add(include);
                }
            }
            outer.Children.Add(line);
            afterRuleScrollView.Content = outer;
        }
        
        private TextBlock SetAfterRuleTextBlock(string content,ICodeBlock codeBlock)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = content;
            textBlock.Background = new SolidColorBrush(Colors.White);
            textBlock.DataContext = codeBlock;
            return textBlock;
        }

        private Button SetEditRuleButton(string content,ICodeBlock codeBlock)
        {
            Button button = new Button();
            button.Content = content;
            button.BorderThickness = new Thickness() { Top = 0, Bottom = 0, Left = 0, Right = 0 };
            button.Background = new SolidColorBrush(Colors.White);
            button.DataContext = codeBlock;
            button.Click += OnRuleSliceBtnClickListener;
            return button;
        }    
    
        //-----richTextBox--------
        private void SetBeforeEditBox()
        {
            beforeEditBox.Document.Blocks.Clear();
            List<Paragraph> result = new List<Paragraph>();

            Paragraph paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0, 0, 0, 0);
            paragraph.Padding = new Thickness(0, 0, 0, 0);

            foreach (ICodeBlock codeBlock in beforeContent)
            {
                Run run = new Run(codeBlock.Content, paragraph.ContentEnd);
                run.DataContext = codeBlock;

                if (beforeContent.IndexOf(codeBlock) == beforeContent.Count - 1)
                {
                    result.Add(paragraph);
                    paragraph = new Paragraph();
                    paragraph.Margin = new Thickness(0, 0, 0, 0);
                    paragraph.Padding = new Thickness(0, 0, 0, 0);
                }
            }
             beforeEditBox.Document.Blocks.AddRange(result);
        }

        //-----list-----
        private void SetRuleList()
        {
            allRuleList.Items.Clear();
            for (int i = 0; i < ruleMetadata.GetRuleSetList().Count; i++)
            {
                RuleSet ruleSet = ruleMetadata.GetRuleSetList()[i];
                TreeViewItem ruleSetTreeView = new TreeViewItem();
                ruleSetTreeView.Header = StaticValue.GetNameFromPath(ruleSet.Name);
                AddRuleListIntoTreeViewByName(ruleSetTreeView, ruleSet);

                /*store ruleSet in TreeView*/
                ruleSetTreeView.DataContext = ruleSet;
                allRuleList.Items.Add(ruleSetTreeView);
            }
            allRuleList.ExpandSubtree();
        }

        private void AddRuleListIntoTreeViewByName(TreeViewItem ruleSetTree, RuleSet ruleSet)
        {
            foreach (Dictionary<string, string> ruleContent in ruleSet.RuleList)
            {
                TreeViewItem rule = new TreeViewItem();
                rule.Header = ruleContent["name"];
                rule.DataContext = ruleMetadata.GetFilePathInRuleSet(ruleContent["name"], ruleSet);
                rule.MouseDoubleClick += OnDoubleClickRuleListener;
                ruleSetTree.Items.Add(rule);
            }
        }

        //-----tool----- 
        private string RemoveRegexToken(string orgText)
        {
            orgText = orgText.Replace(@"[ \t]*\b", "");
            orgText = orgText.Replace(@"[ \t]*", "");
            orgText = orgText.Replace(@"[ \t]+", " ");
            orgText = orgText.Replace(@"\","");
            return orgText;
        }

        private List<ICodeBlock> GetAnalysisList()
        {
            List<ICodeBlock> content = new List<ICodeBlock>();
            foreach (Run run in chooseRange)
            {
                ICodeBlock codeBlock = run.DataContext as ICodeBlock;
                if (content.Count > 0)
                {
                    ICodeBlock lastBlock = content[content.Count - 1];
                    if (codeBlock.IsMatchRule == lastBlock.IsMatchRule && codeBlock.MatchRule == lastBlock.MatchRule)
                    {
                        lastBlock.Content += codeBlock.Content;
                    }
                    else
                    {
                        content.Add(codeBlock.GetCopy());
                    }
                }
                else
                {
                    content.Add(codeBlock.GetCopy());
                }
            }
            return content;
        }

        private void ReplaceBlock(int chooseStartIndex, int chooseContentCount)
        {
            ICodeBlock orgStartBlock = beforeContent[chooseStartIndex];
            beforeContent.RemoveRange(chooseStartIndex, chooseContentCount);
            List<ICodeBlock> insert = new List<ICodeBlock>();
            foreach (Run run in chooseRange)
            {
                insert.Add(run.DataContext as ICodeBlock);
            }

            List<ICodeBlock> result = SpiltString(StaticValue.GetAllContent(insert));

            foreach (ICodeBlock codeBlock in result)
            {
                codeBlock.BlockId = orgStartBlock.BlockId;
                codeBlock.MatchRule = ruleBlockUseNow;
            }

            beforeContent.InsertRange(chooseStartIndex, result);

            chooseRange = new List<Run>();
            SetBeforeEditBox();
            foreach (Paragraph paragraph in beforeEditBox.Document.Blocks)
            {
                foreach (Run run in paragraph.Inlines)
                {
                    if ((run.DataContext as ICodeBlock).MatchRule == ruleBlockUseNow)
                    {
                        run.Background = changeColor;
                        chooseRange.Add(run);
                    }
                }
            }
            ruleBlockUseNow = null;
        }

        private List<ICodeBlock> SpiltString(string orgContent)
        {
            List<ICodeBlock> result = new List<ICodeBlock>();

            result.AddRange(EscapeTokenSet.SpiltByEscapeToken(orgContent));
            SpiltContentInCodeBlockByToken(' ', result);
            SpiltContentInCodeBlockByToken('\n', result);
            SpiltContentInCodeBlockByToken('\t', result);
            SpiltContentInCodeBlockByToken(',', result);
            SpiltContentInCodeBlockByToken(';', result);
            return result;
        }

        private void SpiltContentInCodeBlockByToken(char token, List<ICodeBlock> codeBlockList)
        {
            foreach (ICodeBlock codeBlock in codeBlockList.ToArray())
            {
                List<ICodeBlock> innerResult = new List<ICodeBlock>();
                string[] lineSplit = codeBlock.Content.Split(token);
                for (int i = 0; i < lineSplit.Length; i++)
                {
                    string content = lineSplit[i];
                    ICodeBlock splitBlock = codeBlock.GetCopy();
                    splitBlock.Content = content;
                    innerResult.Add(splitBlock);
                    if (i != lineSplit.Length - 1)
                    {
                        ICodeBlock tokenBlock = codeBlock.GetCopy();
                        tokenBlock.Content = token.ToString();
                        innerResult.Add(tokenBlock);
                    }
                }
                int index = codeBlockList.IndexOf(codeBlock);
                //remove
                codeBlockList.Remove(codeBlock);
                //insert
                codeBlockList.InsertRange(index, innerResult);
            }
        }

        private void ResetEditBox()
        {
            foreach (Paragraph paragraph in beforeEditBox.Document.Blocks)
            {
                foreach (Run run in paragraph.Inlines)
                {
                    run.Background = orgBackgroundColor;
                    run.Foreground = orgForegroundColor;
                }
            }
        }
        //-----listener--------
        private void OnCancelBtnListener(object sender, RoutedEventArgs e)
        {
            StaticValue.BtCancelListener(sender, e, this);
        }

        private void OnOkBtnClickListener(object sender, RoutedEventArgs e)
        {
            if (chooseRange.Count > 0)
            {
                List<ICodeBlock> content = GetAnalysisList();
                RuleBlock ruleBlock = (chooseRange[0].DataContext as ICodeBlock).MatchRule;
                bool isSuccess = AnalysisTool.GetInstance().AnalysisSingleRule(ruleBlock, content, fileIndex,chooseStartIndex,chooseContentCount);

                if (isSuccess)
                {
                    MessageBox.Show("down");
                    isAnalysisSuccess = true;
                    StaticValue.CloseWindow(this);
                }
            }
        }

        private void OnDoubleClickRuleListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem rule = (TreeViewItem)sender;
            string path = (string)rule.DataContext;
            ruleBlockUseNow = FileLoader.GetInstance().LoadSingleRuleByPath(path);
            Refresh();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWindowHeight = e.NewSize.Height;
            double newWindowWidth = (e.NewSize.Width - ruleList.ActualWidth) / 2;
            int padding = 20;
            afterRuleScrollView.Width = newWindowWidth;
            afterRuleScrollView.Height = (newWindowHeight - padding)/2;

            beforeRuleScrollView.Width = newWindowWidth;
            beforeRuleScrollView.Height = (newWindowHeight - padding)/2;

            beforeEditBox.Width = newWindowWidth;
            beforeEditBox.Height = newWindowHeight - padding;
        }

        private void OnResetEditBtnClickListener(object sender, RoutedEventArgs e)
        {
            ResetEditBox();
            chooseContentCount = 0;
            chooseStartIndex = 0;
        }

        private void OnRuleSliceBtnClickListener(object sender, RoutedEventArgs e)
        {
            ruleSliceChooseNow = (sender as Button).DataContext as ICodeBlock;
        }

        private void OnBeforeEditBoxPreviewMouseLeftButtonUpListener(object sender, MouseButtonEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            TextSelection selection = richTextBox.Selection;
            bool isMatch = false;
            if (!selection.IsEmpty )
            {
                if (ruleBlockUseNow != null)
                {//choose reset range
                    chooseContentCount = 0;
                    chooseStartIndex = 0;
                    Run lastRun = null;
                    foreach (Paragraph paragraph in richTextBox.Document.Blocks)
                    {
                        foreach(Run run in paragraph.Inlines)
                        {
                            if (!isMatch && run.ContentStart.CompareTo(selection.Start) == 0)
                            {//start choose
                                //0 = same, -1 = run.start < selection.start , 1 = run.start > selection.start 
                                isMatch = true;
                                chooseStartIndex = beforeContent.IndexOf(run.DataContext as ICodeBlock);
                            }
                            else if (!isMatch && lastRun != null && (run.ContentStart.CompareTo(selection.Start) > lastRun.ContentStart.CompareTo(selection.Start)))
                            {// if string order is lastRun ->run->selection 
                                chooseStartIndex = beforeContent.IndexOf(lastRun.DataContext as ICodeBlock);
                                (lastRun.DataContext as ICodeBlock).MatchRule = ruleBlockUseNow;
                                (lastRun.DataContext as ICodeBlock).IsMatchRule = false;
                                lastRun.Background = changeColor;
                                chooseRange.Add(lastRun);
                                chooseContentCount++;
                                isMatch = true;
                            }

                            if (isMatch)
                            {
                                (run.DataContext as ICodeBlock).MatchRule = ruleBlockUseNow;
                                (run.DataContext as ICodeBlock).IsMatchRule = false;
                                run.Background = changeColor;
                                chooseRange.Add(run);
                                chooseContentCount++;

                                if (run.ContentEnd.CompareTo(selection.End) > -1)
                                {
                                    //  ruleBlockUseNow = null;
                                    ReplaceBlock(chooseStartIndex, chooseContentCount);
                                    return;
                                }
                            }                            
                            lastRun = run;
                        }                        
                    }
                }
                else if (ruleSliceChooseNow != null && selection.Text.Length > 0)
                {//set block
                    foreach (Run run in chooseRange)
                    {
                        if (run.ContentStart.CompareTo(selection.Start) > -1)
                        {//start choose
                            //0 = same, -1 = run.start < selection.start , 1 = run.start > selection.start 
                            isMatch = true;
                        }
                        if (isMatch)
                        {
                            if (ruleSliceChooseNow is ParameterBlock)
                            {
                                run.Foreground = SystemColors.HighlightBrush;
                                (run.DataContext as ICodeBlock).MatchRule = null;
                                (run.DataContext as ICodeBlock).IsMatchRule = false;
                            }
                            else if (ruleSliceChooseNow is CodeBlock)
                            {
                                run.Background = SystemColors.HighlightBrush;
                                (run.DataContext as ICodeBlock).MatchRule = null;
                                (run.DataContext as ICodeBlock).IsMatchRule = false;
                            }
                            else if (ruleSliceChooseNow is IncludeBlock)
                            {
                                run.Background = SystemColors.InactiveCaptionBrush;
                                IncludeBlock include = ruleSliceChooseNow as IncludeBlock;
                                (run.DataContext as ICodeBlock).MatchRule = FileLoader.GetInstance().LoadSingleRuleByPath(ruleMetadata.GetRulePathById(include.FromRuleSetId, include.CompareRuleId));
                                (run.DataContext as ICodeBlock).IsMatchRule = false;
                            }
                        }

                        if (run.ContentEnd.CompareTo(selection.End) > -1 )
                        {
                            ruleSliceChooseNow = null;
                            return;
                        }                            
                    }                   
                                     
                }
            }
        }

    }
}
