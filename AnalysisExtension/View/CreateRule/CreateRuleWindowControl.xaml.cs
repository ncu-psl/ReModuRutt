namespace AnalysisExtension
{
    using AnalysisExtension.Model;
    using AnalysisExtension.Tool;
    using AnalysisExtension.View;
    using AnalysisExtension.View.CreateRule;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Xml;

    public partial class CreateRuleToolWindowControl : UserControl
    {        
        private FileLoader fileLoader = FileLoader.GetInstance();
        private RuleMetadata ruleMetadata = RuleMetadata.GetInstance();

        private RuleSet ruleSetOpenNow;
        private RuleBlock ruleBlockEditNow;
        private string ruleNameOpenNow;

        private RichTextBox ruleBefore = new RichTextBox();
        private RichTextBox ruleAfter = new RichTextBox();

        private List<ParameterBlock> paraList;
        private List<CodeBlock> codeBlockList;
        private List<IncludeBlock> includeList;

        private bool IsEditViewChange = false;
        private ParameterBlock parameterEditNow = null;
        private CodeBlock codeBlockEditNow = null;
        private IncludeBlock IncludeBlockEditNow = null;

        private Point startDragPoint;

        private static CreateRuleToolWindowControl instance = null;
        private ToolWindowPane windowPane = null;

        //----- init -----
        public static CreateRuleToolWindowControl GetInstance()
        {
            return instance;
        }

        public static CreateRuleToolWindowControl CreateNewFrame(ToolWindowPane pane)
        {
            instance = new CreateRuleToolWindowControl(pane);
            return instance;
        }

        private CreateRuleToolWindowControl(ToolWindowPane pane)
        {
            this.InitializeComponent();
            windowPane = pane;
            Refresh();
            SizeChanged += OnSizeChanged;
        }

        //----- refresh -----
        private void Refresh()
        {  
            RefreshRuleSetListView();
            InitAllBlockList();

            windowPane.Caption = "";
            windowPane.Content = instance;

            if (ruleSetOpenNow != null)
            {
                ruleCreateStackPanel.Children.Clear();
                SetTitle("rule set open now : " + ruleSetOpenNow.Name);
            }
        }

        private void RefreshRuleSetListView()
        {
            ruleMetadata.Refresh();

            allRuleSetTreeView.Items.Clear();
            AddRuleSetListView();
        }

        private void ResetEditStatus()
        {
            parameterEditNow = null;
            codeBlockEditNow = null;
            IncludeBlockEditNow = null;
        }

        private void InitAllBlockList()
        {
            ResetEditStatus();
            paraList = new List<ParameterBlock>();
            codeBlockList = new List<CodeBlock>();
            includeList = new List<IncludeBlock>();
            RefreshParameterTreeView();
            RefreshCodeBlockTreeView();
            RefreshIncludeTreeView();
        }  

        //----- text pattern change-----        
        private List<Paragraph> ChangeToColor(string orgText,string tag)
        {
            int blockCount = 1;// index/number of <block> in <layer>
            int parameterCount = 1;// index/number of <para> in <layer>
            int includeCount = 1;// index/number of <include> in <layer>

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<xml xml:space=\"" + "preserve\"" + ">" + orgText + "</xml>");

            List<Paragraph> allResult = new List<Paragraph>();
            
            Paragraph result = new Paragraph();
            result.Margin = new Thickness(0, 0, 0, 0);

            while (orgText.IndexOf("<block") > -1 || orgText.IndexOf("<para") > -1 || orgText.IndexOf("<include") > -1)
            {
                string findResult = null;
                int min = GetMin( new int[]{orgText.IndexOf("<block"), orgText.IndexOf("<para"),orgText.IndexOf("<include")} , orgText.Length);
                if (orgText.IndexOf("<block") == min)
                {//is block
                    findResult = ChangeBlockToColor(result,xmlDocument,orgText,blockCount);
                    blockCount++;
                }
                else if (orgText.IndexOf("<para") == min)
                {//is para
                    findResult = ChangeParameterToColor(result, xmlDocument, orgText, parameterCount);
                    parameterCount++;
                } 
                else if(orgText.IndexOf("<include") == min)
                {
                    Match startMatch = Regex.Match(orgText, @"<include");
                    if (startMatch.Index != 0)
                    {
                        int index = startMatch.Index;
                        if (Regex.Match(orgText, @"[\s]*<include").Success && Regex.Match(orgText, @"[\s]*<include").Index < index)
                        {
                            index = Regex.Match(orgText, @"[\s]*<include").Index;
                        }
                        string frontContent = orgText.Substring(0, index);
                        orgText = orgText.Substring(index);
                        Run front = new Run(frontContent, result.ContentEnd);
                        allResult.Add(result);
                    }
                    result = new Paragraph();
                    result.Margin = new Thickness(0, 0, 0, 0);                    
                    findResult = ChangeIncludeToColor(tag, result, xmlDocument, orgText, includeCount);
                    includeCount++;

                    allResult.Add(result);
                    result = new Paragraph();
                    result.Margin = new Thickness(0, 0, 0, 0);
                }

                if (findResult == null)
                {
                    break;
                }
                else
                {
                   orgText = findResult;
                }
            }
            Run endText = new Run(orgText, result.ContentEnd);
            allResult.Add(result);
            return allResult;
        }       

        private string ChangeBlockToColor(Paragraph result, XmlDocument xmlDocument, string orgText,int blockCount)
        {
            int startIndex = /*Regex.Match(orgText,@"[\s*]<block").Index;//*/orgText.IndexOf("<block");
            int endIndex = orgText.Substring(startIndex).IndexOf("/>") + startIndex - 1;
            int endTokenLen = 2;                

            XmlElement blockElement = StaticValue.FindElementByTag(xmlDocument, blockCount, "block", "");

            if (blockElement == null)
            {
                return null;
            }
            else
            {
                string frontContent = orgText.Substring(0, startIndex);
                Run front = new Run(frontContent, result.ContentEnd);

                int codeBlockId = int.Parse(StaticValue.GetAttributeInElement(blockElement, "id"));
                SetBlockFormate(codeBlockId, result.ContentEnd);

                orgText = orgText.Substring(endIndex + endTokenLen + 1);
            }
            return orgText;
        }

        private string ChangeParameterToColor(Paragraph result, XmlDocument xmlDocument, string orgText, int paraCount)
        {
            int startIndex = orgText.IndexOf("<para");
            int endIndex = orgText.Substring(startIndex).IndexOf("/>") + startIndex - 1;
            int endTokenLen = 2;

            XmlElement paraElement = StaticValue.FindElementByTag(xmlDocument, paraCount, "para", "");

            if (paraElement == null)
            {
                return null;
            }
            else
            {
                string frontContent = orgText.Substring(0, startIndex);
                Run front = new Run(frontContent, result.ContentEnd);

                int paraId = int.Parse(StaticValue.GetAttributeInElement(paraElement, "id"));
                SetParameterFormate(paraId, result.ContentEnd);

                orgText = orgText.Substring(endIndex + endTokenLen + 1);
            }
            return orgText;
        }

        private string ChangeIncludeToColor(string tag, Paragraph result, XmlDocument xmlDocument, string orgText, int includeCount)
        {
            Match match = Regex.Match(orgText, @"<include");

            int index = match.Index;
            if (Regex.Match(orgText, @"[ \t]*<include").Success && Regex.Match(orgText, @"([ \t]*)<include").Index < index)
            {
                index = Regex.Match(orgText, @"([\s]*)<include").Index;
                string frontText = Regex.Match(orgText, @"([ \t]*)<include").Groups[1].Value;
                Run front = new Run(frontText, result.ContentEnd);
            }

            Match endToken = Regex.Match(orgText.Substring(index), @"/>[\n\r]*");//@"/>"
            int endIndex = endToken.Index + index - 1;

            XmlElement includeElement = StaticValue.FindElementByTag(xmlDocument, includeCount, "include", "");

            if (includeElement == null)
            {
                return null;
            }
            else
            {
                int codeBlockId = int.Parse(StaticValue.GetAttributeInElement(includeElement, "id"));
                int compareRuleId = int.Parse(StaticValue.GetAttributeInElement(includeElement, "compareRuleId"));
                int fromRuleSetId = int.Parse(StaticValue.GetAttributeInElement(includeElement, "fromRuleSetId"));

                RuleSet ruleSet = ruleMetadata.GetRuleSetById(fromRuleSetId);
                string includeInfo = "by rule set " + ruleSet.Name + ", rule " + ruleSet.GetRuleInfoById(compareRuleId)["name"];

                RuleBlock rule = fileLoader.LoadSingleRuleByPath(ruleMetadata.GetRulePathById(fromRuleSetId, compareRuleId));

                
                
                StackPanel inner = SetIncludeInline(rule,tag,includeInfo);
                InlineUIContainer container = new InlineUIContainer(inner, result.ContentEnd);
                container.DataContext = new IncludeBlock("", codeBlockId, compareRuleId, fromRuleSetId);

                orgText = orgText.Substring(endIndex + endToken.Length + 1);
            }
            return orgText;
        }

        private StackPanel SetIncludeInline(RuleBlock rule,string tag,string info)
        {
            StackPanel panel = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 0, 5)};

            TextBlock infoBlock = new TextBlock() { Text = info, Background = SystemColors.InactiveCaptionBrush, Foreground = SystemColors.InactiveCaptionTextBrush, Margin = new Thickness(0, 0, 0, 0), Padding = new Thickness(){ Left = 3, Right = 3 } };
            RichTextBox innerBox = new RichTextBox() { Background = SystemColors.MenuBarBrush , Margin = new Thickness(0, 0, 0, 0) };
            List<Paragraph> interResult = ChangeToColor(rule.GetOrgText(tag), tag);

             int maxLine = info.Length;
             foreach (Paragraph paragraph in interResult)
             {
                 int textLen = 0;
                 foreach (Inline inline in paragraph.Inlines)
                 {
                     if (inline is Run)
                    { 
                         Run inlineRun = inline as Run;
                         if (inlineRun.Text.Contains("\n"))
                         {
                             if (textLen > maxLine)
                             {
                                 maxLine = textLen;
                             }
                             textLen = 0;
                         }
                         else
                         {
                             textLen += inlineRun.Text.Length;
                         }
                     }
                 }

                 if (textLen > maxLine)
                 {
                     maxLine = textLen;
                 }
             }

            innerBox.Background = SystemColors.MenuBarBrush;
            innerBox.Margin = new Thickness(0, 0, 0, 0);
            innerBox.Width = maxLine * 10;
            innerBox.Document.Blocks.Clear();
            innerBox.Document.Blocks.AddRange(interResult);

            panel.Children.Add(infoBlock);
            panel.Children.Add(innerBox);

            return panel;
        }

        //----- main frame -----
        private void SetBlockList(RichTextBox richTextBox)
        {
            foreach (Paragraph paragraph in richTextBox.Document.Blocks)
            {
                foreach (Inline inline in paragraph.Inlines)
                {
                    if (inline is InlineUIContainer)
                    {
                        InlineUIContainer container = inline as InlineUIContainer;

                        if (container.Child is StackPanel)
                        {
                            AddIntoIncludeList(inline.DataContext as IncludeBlock);
                        }
                        else if (container.Child is TextBlock)
                        {
                            TextBlock textBlock = container.Child as TextBlock;
                            if (textBlock.Background == SystemColors.HighlightBrush)
                            {//is block
                                int codeBlockId = int.Parse(Regex.Match(textBlock.Text, "[(]" + @"(\d+)" + "[)]").Groups[1].Value);
                                AddIntoCodeBlockList(new CodeBlock("", codeBlockId));
                            }
                            else if (textBlock.Foreground == SystemColors.HighlightBrush)
                            {//is parameter
                                string match = Regex.Match(textBlock.Text, "[(]" + @"(\d+)" + "[)]").Groups[1].Value;
                                int paraId = int.Parse(match);
                                AddIntoParaList(new ParameterBlock("", paraId));
                            }
                        }
                    }
                }
            }
        }

        private void AddRuleEditView(string filePath)
        {
            string beforeContent = "";
            string afterContent = "";
            if (IsEditViewChange)
            {
                MessageBoxResult result = MessageBox.Show("save file open now?", "Save File", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                {
                    SaveRule();

                    ruleNameOpenNow = null;
                    ruleBlockEditNow = null;
                }
                else if (result == MessageBoxResult.No)
                {
                    ruleBefore.Document.Blocks.Clear();
                    ruleAfter.Document.Blocks.Clear();
                    IsEditViewChange = false;
                    ruleNameOpenNow = null;
                    ruleBlockEditNow = null;
                }
                else
                {
                    return;
                }
            }

            if (filePath == null)
            {
                if (ruleSetOpenNow != null)
                {
                    MessageBoxResult result = MessageBox.Show("create new rule in rule set " + ruleSetOpenNow.Name, "create new rule", MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        ShowChooseRuleSetWindow();
                    }
                }
                else
                {
                    ShowChooseRuleSetWindow();
                }

                //create file  
                filePath = CreateNewRule();
                if (filePath == null)
                {
                    return;
                }
            }
            
            ruleBlockEditNow = fileLoader.LoadSingleRuleByPath(filePath);
            ruleNameOpenNow = ruleBlockEditNow.RuleName;
            beforeContent = RemoveLineAtFirstAndEnd(ruleBlockEditNow.GetOrgText("before"));
            afterContent = RemoveLineAtFirstAndEnd(ruleBlockEditNow.GetOrgText("after"));
            whitespaceIgnoreCheckBox.IsChecked = ruleBlockEditNow.CanSpaceIgnore;


            if (ruleBlockEditNow != null)
            {
                paraList = new List<ParameterBlock>();
                codeBlockList = new List<CodeBlock>();
                includeList = new List<IncludeBlock>();

                string title = "rule set open now : " + ruleSetOpenNow.Name;
                if (ruleBlockEditNow != null)
                {
                    title += " , rule : " + ruleBlockEditNow.RuleName;
                }

                SetTitle(title);
                SetRuleEditView(beforeContent, afterContent);
                IsEditViewChange = false;
            }
        }

        private void SetRuleEditView(string beforeContent, string afterContent)
        {
            IsEditViewChange = false;
            ruleCreateStackPanel.Children.Clear();

            SetEditTextBoxTemplate(ruleBefore);
            ruleBefore.Document.Blocks.AddRange(ChangeToColor(beforeContent,"before"));
            SetEditTextBoxTemplate(ruleAfter);
            ruleAfter.Document.Blocks.AddRange(ChangeToColor(afterContent, "after"));

            ruleCreateStackPanel.Children.Add(SetEditInfoPanel("before"));
            ruleCreateStackPanel.Children.Add(ruleBefore);
            ruleCreateStackPanel.Children.Add(new Label());
            ruleCreateStackPanel.Children.Add(SetEditInfoPanel("after"));
            ruleCreateStackPanel.Children.Add(ruleAfter);

            SetBlockList(ruleBefore);
            SetBlockList(ruleAfter);

            RefreshParameterTreeView();
            RefreshCodeBlockTreeView();
            RefreshIncludeTreeView();
        }

        private void SetEditTextBoxTemplate(RichTextBox textBox)
        {
            textBox.Document.Blocks.Clear();
            textBox.ContextMenu = (ContextMenu)FindResource("richTextBoxMenu");
            textBox.AcceptsReturn = true;
            textBox.AcceptsTab = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.Background = SystemColors.WindowBrush;
            textBox.TextChanged += new TextChangedEventHandler(OnTextBoxChangedListener);
            textBox.AllowDrop = true;
            textBox.PreviewDrop += OnTextBoxDropListener; ;
            textBox.PreviewDragOver += OnTextBoxDragOverListener; ;
            textBox.PreviewMouseLeftButtonUp += OnTextBoxPreviewMouseLeftButtonUp;
        }

        private Panel SetEditInfoPanel(string labelText)
        {
            Panel panel = new DockPanel();
            panel.Children.Add(SetEditLabel(labelText));

            StackPanel btPanel = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Right, Orientation = Orientation.Horizontal};
            btPanel.Margin = new Thickness { Right = 5 };

            Button clear = new Button() { Content = "Clear", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness { Left = 5 } };
            if (labelText.Equals("after"))
            {
                Button copy = new Button() { Content = "Copy form before", HorizontalAlignment = HorizontalAlignment.Right };
                copy.Click += OnClickBtRuleEditCopyListener;
                btPanel.Children.Add(copy);

                clear.Click += OnClickBtRuleEditAfterClearListener;
            }
            else
            {
                clear.Click += OnClickBtRuleEditBeforeClearListener;
            }

            btPanel.Children.Add(clear);

            panel.Children.Add(btPanel);

            return panel;
        }

        private TextBlock SetEditLabel(string labelText)
        {
            TextBlock label = new TextBlock();
            label.Text = labelText;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.Margin = new Thickness() { Left = 5 };
            label.Padding = new Thickness() { Left = 3, Right = 3 };
            label.Background = SystemColors.InactiveCaptionBrush;
            label.Foreground = SystemColors.InactiveCaptionTextBrush;

            return label;
        }

        private void AddRuleCreateNowIntoMetadata(int ruleSetId,int ruleId,string ruleName)
        {
            ruleMetadata.AddRuleIntoRuleSet(ruleSetId, ruleId, ruleName);
            ruleMetadata.RewriteMetadata();
            RefreshRuleSetListView();
        }

        //----- menu -----
        private void AddRuleSetListView()
        {
            for (int i = 0; i < ruleMetadata.GetRuleSetList().Count; i++)
            {
                RuleSet ruleSet = ruleMetadata.GetRuleSetList()[i];
                TreeViewItem ruleSetTreeView = new TreeViewItem();
                ruleSetTreeView.Header = StaticValue.GetNameFromPath(ruleSet.Name);
                AddRuleListIntoTreeViewByName(ruleSetTreeView, ruleSet);

                /*store ruleSet in ruleSetTreeView*/
                ruleSetTreeView.DataContext = ruleSet;
                allRuleSetTreeView.Items.Add(ruleSetTreeView);
            }
        }

        private void AddRuleListIntoTreeViewByName(TreeViewItem ruleSetTree ,RuleSet ruleSet)
        {
            foreach (Dictionary<string, string> ruleContent in ruleSet.RuleList)
            {
                TreeViewItem rule = new TreeViewItem();
                rule.Header = ruleContent["name"];
                rule.DataContext = GetFilePathInRuleSet(ruleContent["name"], ruleSet);
                rule.ContextMenu = (ContextMenu)FindResource("ruleRightClickMenu");
                rule.MouseDoubleClick += OnDoubleClickRuleListener;
                rule.MouseDown += OnRuleListMouseDownListener;
                rule.MouseMove += OnRuleListMouseMoveListener;
                ruleSetTree.Items.Add(rule);
            }
        }

        private void RefreshParameterTreeView()
        {
            paraListTreeView.Items.Clear();
            foreach (ParameterBlock parameterBlock in paraList)
            {
                TreeViewItem para = new TreeViewItem();
                para.Header = "(" + parameterBlock.ParaListIndex + ")";
                para.DataContext = parameterBlock;
                para.MouseDoubleClick += OnDoubleClickParameterListListener;
                para.MouseDown += OnParaListMouseDownListener;
                para.MouseMove += OnParaListMouseMoveListener;
                paraListTreeView.Items.Add(para);
            }
        }

        private void RefreshCodeBlockTreeView()
        {
            blockListTreeView.Items.Clear();
            foreach (CodeBlock codeBlock in codeBlockList)
            {
                TreeViewItem block = new TreeViewItem();
                block.Header = "(" + codeBlock.BlockListIndex + ")";
                block.DataContext = codeBlock;
                block.MouseDoubleClick += OnDoubleClickCodeBlockListListener;
                block.MouseDown += OnBlockMouseDownListener;
                block.MouseMove += OnBlockMouseMoveListener;
                blockListTreeView.Items.Add(block);
            }
        }

        private void RefreshIncludeTreeView()
        {
            includeListTreeView.Items.Clear();
            foreach (IncludeBlock includeBlock in includeList)
            {
                TreeViewItem block = new TreeViewItem();
                block.Header = "(" + includeBlock.IncludeBlockListIndex + ")";
                block.DataContext = includeBlock;
                block.MouseDoubleClick += OnDoubleClickIncludeListListener;
                block.MouseDown += OnIncludeBlockMouseDownListener;
                block.MouseMove += OnIncludeBlockMouseMoveListener;
                includeListTreeView.Items.Add(block);
            }
        }
        //-----final result-----
        private string ChangeToText(RichTextBox textBox)
        {
            string result = "";
            foreach (Paragraph paragraph in textBox.Document.Blocks)
            {
                foreach (Inline inline in paragraph.Inlines)
                {
                    if (inline is InlineUIContainer)
                    {
                        InlineUIContainer container = inline as InlineUIContainer;

                        if (container.Child is StackPanel)
                        {
                            //result += "\n";
                            IncludeBlock includeBlock = inline.DataContext as IncludeBlock;
                            result += "<include id=\"" + includeBlock.IncludeBlockListIndex + "\" compareRuleId=\"" + includeBlock.CompareRuleId + "\" fromRuleSetId=\"" + includeBlock.FromRuleSetId + "\"/>";
                        }
                        else if (container.Child is TextBlock)
                        {
                            TextBlock textBlock = container.Child as TextBlock;
                            if (textBlock.Background == SystemColors.HighlightBrush)
                            {//is block
                                result += Regex.Replace(textBlock.Text, "[(]" + @"(\d+)" + "[)]", "<block id=" + "\"$1\"/>");
                            }
                            else if (textBlock.Foreground == SystemColors.HighlightBrush)
                            {//is parameter
                                result += Regex.Replace(textBlock.Text, "[(]" + @"(\d+)" + "[)]", "<para id=" + "\"$1\"/>");
                            }
                        }
                    }
                    else
                    {
                        result += (inline as Run).Text;                        
                    }                    
                }

                result += "\n";
            }
            return result;
        }

        private string GetFinalRule(int ruleId)
        {
            string beforeText = ChangeToText(ruleBefore);
            beforeText = RemoveLineAtFirstAndEnd(beforeText);
            string afterText = ChangeToText(ruleAfter);
            afterText = RemoveLineAtFirstAndEnd(afterText);

            return GetRuleXml(beforeText, afterText, ruleId);
        }

        private string GetRuleXml(string beforeText, string afterText,int ruleId)
        {
            string head = @"<rule xml:space=" + "\"preserve\" " + "id=" + "\"" + ruleId + "\"" + " name=" + "\"" + ruleNameOpenNow + "\"" + " canWhitespaceIgnore=" + "\"" + whitespaceIgnoreCheckBox.IsChecked.ToString() + "\"" + @">" + "\n";
            string before = @"<before>" + "\n" + beforeText + "\n" + @"</before>" + "\n";
            string after = @"<after>" + "\n" + afterText + "\n" + @"</after>" + "\n";
            string end = @"</rule>";

            return head + before + after + end;
        }

        private void SaveRule()
        {
            int ruleId = ruleBlockEditNow.RuleId;
            string final = GetFinalRule(ruleId);
            string path = GetFilePathInRuleSet(ruleNameOpenNow, ruleSetOpenNow);
            File.WriteAllText(path, final);

            AddRuleCreateNowIntoMetadata(ruleSetOpenNow.Id, ruleId, ruleNameOpenNow);
            IsEditViewChange = false;
            MessageBox.Show("file save");
        }

        private string SaveAsRule()
        {
            int ruleId = ruleMetadata.GetNextRuleIdByRuleSetId(ruleSetOpenNow.Id);
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "(*.xml)|*.xml";
            saveFileDialog.Title = "save as";
            saveFileDialog.InitialDirectory = Path.GetFullPath(StaticValue.RULE_FOLDER_PATH + "\\" + ruleSetOpenNow.Name);
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                ruleNameOpenNow = StaticValue.GetNameFromPath(saveFileDialog.FileName).Split('.')[0];
                string[] split = Path.GetFullPath(saveFileDialog.FileName).Split('\\');
                string ruleSetName = split[split.Length - 2];// ruleSetName(-2) \\ ruleName.xml (-1)
                string final = GetFinalRule(ruleId);

                FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
                fileStream.Write(Encoding.ASCII.GetBytes(final), 0, Encoding.ASCII.GetByteCount(final));
                fileStream.Close();

                AddRuleCreateNowIntoMetadata(ruleMetadata.GetRuleSetByName(ruleSetName).Id,ruleId,ruleNameOpenNow);
                IsEditViewChange = false;
                MessageBox.Show("file save");
                return Path.GetFullPath(saveFileDialog.FileName);
            }

            return null;
        }

        private string CreateNewRule()
        {
            int ruleId = ruleMetadata.GetNextRuleIdByRuleSetId(ruleSetOpenNow.Id);
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "(*.xml)|*.xml";
            saveFileDialog.Title = "save as";
            saveFileDialog.InitialDirectory = Path.GetFullPath(StaticValue.RULE_FOLDER_PATH + "\\" + ruleSetOpenNow.Name);
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                ruleNameOpenNow = StaticValue.GetNameFromPath(saveFileDialog.FileName).Split('.')[0];
                string[] split = Path.GetFullPath(saveFileDialog.FileName).Split('\\');
                string ruleSetName = split[split.Length - 2];// ruleSetName(-2) \\ ruleName.xml (-1)
                string final = GetRuleXml("","",ruleId);

                FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
                fileStream.Write(Encoding.ASCII.GetBytes(final), 0, Encoding.ASCII.GetByteCount(final));
                fileStream.Close();

                AddRuleCreateNowIntoMetadata(ruleMetadata.GetRuleSetByName(ruleSetName).Id, ruleId, ruleNameOpenNow);
                IsEditViewChange = false;
                MessageBox.Show("file create");
                return Path.GetFullPath(saveFileDialog.FileName);
            }

            return null;
        }

        private void CopyRule(string before,string after,int ruleId,string ruleName)
        {
            string final = GetRuleXml(before,after,ruleId);

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = Path.GetFullPath(StaticValue.RULE_FOLDER_PATH + "\\" + ruleSetOpenNow.Name)+"\\"+ruleName +".xml";

            FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
            fileStream.Write(Encoding.ASCII.GetBytes(final), 0, Encoding.ASCII.GetByteCount(final));
            fileStream.Close();

            AddRuleCreateNowIntoMetadata(ruleSetOpenNow.Id, ruleId, ruleName);
            IsEditViewChange = false;
            MessageBox.Show("copy success");

        }

        //-----tool-----
        private void SetParameterFormate(int paraId, TextPointer position)
        {
            TextBlock para = new TextBlock() { Text = "(" + paraId + ")", Foreground = SystemColors.HighlightBrush };
            InlineUIContainer container = new InlineUIContainer(para, position);
        }

        private void SetBlockFormate(int blockId, TextPointer position)
        {
            TextBlock block = new TextBlock() { Text = "(" + blockId + ")", Background = SystemColors.HighlightBrush };
            InlineUIContainer container = new InlineUIContainer(block, position);
        }


        private void ShowChooseRuleSetWindow()
        {
            Window window = new Window();
            window.Width = 200;
            window.Height = 200;
            ChooseEditRuleSetWindowControl chooseEditRuleSetWindow = new ChooseEditRuleSetWindowControl();
            window.Content = chooseEditRuleSetWindow;
            window.ShowDialog();
            ruleSetOpenNow = chooseEditRuleSetWindow.chooseRuleSet;
        }

        public void AddTextIntoRuleCreateFrame(string selectContent)
        {
            ruleBefore.Document.Blocks.AddRange(ChangeToColor(selectContent, "before"));
        }

        private int GetMin(int[] list,int upperBound)
        {
            int result = upperBound;

            foreach (int i in list)
            {
                if (i > -1 && i < result)
                {
                    result = i;
                }
            }

            return result;
        }

        private void SetTitle(string title)
        {
            windowPane.Caption = title;
        }

        private string GetFilePathInRuleSet(string name, RuleSet ruleSet)
        {
            return StaticValue.RULE_FOLDER_PATH + "\\" + ruleSet.Name + "\\" + name + ".xml";
        }

        private string RemoveLineAtFirstAndEnd(string orgText)
        {
            if (orgText.StartsWith("\r\n"))
            {
                orgText = orgText.Remove(0, 2);
            }
            else if (orgText.StartsWith("\n"))
            {
                orgText = orgText.Remove(0, 1);
            }

            if (orgText.EndsWith("\r\n"))
            {
                int len = orgText.Length;
                orgText = orgText.Remove(orgText.Length - 2, 2);
            }
            else if (orgText.EndsWith("\n"))
            {
                int len = orgText.Length;
                orgText = orgText.Remove(orgText.Length - 1, 1);
            }

            return orgText;
        }

        private bool IsIncludeRuleContain(int ruleSetId,int ruleId, int findRuleSetId, int findRuleId,string tag)
        {
            List<ICodeBlock> list;

            if (ruleSetId == findRuleSetId && ruleId == findRuleId)
            {
                return true;
            }

            RuleBlock ruleBlock = fileLoader.LoadSingleRuleByPath(ruleMetadata.GetRulePathById(ruleSetId, ruleId));
            if (tag.Equals("before"))
            {
                list = ruleBlock.BeforeRuleSliceList;
            }
            else
            {
                list = ruleBlock.AfterRuleSliceList;
            }

            foreach (ICodeBlock block in list)
            {
                if (block.TypeName.Equals(StaticValue.INCLUDE_TYPE_NAME))
                {
                    IncludeBlock includeBlock = block as IncludeBlock;

                    if (IsIncludeRuleContain(includeBlock.FromRuleSetId, includeBlock.CompareRuleId, findRuleSetId, findRuleId, tag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //-----add into list
        private void AddIntoParaList(ParameterBlock parameterBlock)
        {
            bool isFind = false;
            foreach (ParameterBlock findBlock in paraList)
            {
                if (findBlock.ParaListIndex == parameterBlock.ParaListIndex)
                {
                    isFind = true;
                    break;
                }
            }

            if (!isFind)
            {
                paraList.Add(parameterBlock);
            }
        }

        private void AddIntoCodeBlockList(CodeBlock codeBlock)
        {
            bool isFind = false;
            foreach (CodeBlock findBlock in codeBlockList)
            {
                if (findBlock.BlockListIndex == codeBlock.BlockListIndex)
                {
                    isFind = true;
                    break;
                }
            }

            if (!isFind)
            {
                codeBlockList.Add(codeBlock);
            }
        }

        private void AddIntoIncludeList(IncludeBlock includeBlock)
        {
            bool isFind = false;
            foreach (IncludeBlock findBlock in includeList)
            {
                if (findBlock.IncludeBlockListIndex == includeBlock.IncludeBlockListIndex)
                {
                    isFind = true;
                    break;
                }
            }

            if (!isFind)
            {
                includeList.Add(includeBlock);
            }
        }
        //-----listener-----
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWindowHeight = e.NewSize.Height;
            double newWindowWidth = e.NewSize.Width;
            int padding = 20;
            ruleCreateStackPanel.Width = newWindowWidth - ruleTreeView.ActualWidth;
            ruleCreateStackPanel.Height = newWindowHeight - padding;
            ruleBefore.Height = ruleCreateStackPanel.Height / 2 - padding*1.5;
            ruleBefore.Width = ruleCreateStackPanel.Width  - padding/2;
            ruleBefore.Document.PageWidth = ruleCreateStackPanel.Width * 2;

            ruleAfter.Height = ruleCreateStackPanel.Height / 2 - padding*1.5;
            ruleAfter.Width = ruleCreateStackPanel.Width - padding/2;
            ruleAfter.Document.PageWidth = ruleCreateStackPanel.Width * 2;
        }

        private void OnTextBoxChangedListener(object sender, TextChangedEventArgs e)
        {
            IsEditViewChange = true;
        }

        private void OnTextBoxPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RichTextBox textBox = sender as RichTextBox;
            TextSelection selection = textBox.Selection;
            TextPointer textPointer = textBox.GetPositionFromPoint(e.GetPosition(textBox), true);
            if (textPointer.Paragraph.Inlines.Count == 1 && textPointer.Paragraph.Inlines.FirstInline is InlineUIContainer)
            {//include rule
            }
            else
            {
                //TODO : add click insert include rule method
                ResetEditStatus();             
            }
        }

        //-----button listener-----
        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
        }

        private void OnClickBtSaveListener(object sender, RoutedEventArgs e)
        {
            if (GetFilePathInRuleSet(ruleNameOpenNow, ruleSetOpenNow) != null)
            {
                MessageBoxResult result = MessageBox.Show("file is exist , sure to overwrite this file?", "Save File", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    SaveRule();
                }
                else
                {
                    return;
                }
            }
            else
            {
                SaveAsRule();
            }            
            Refresh();
        }

        private void OnClickBtSaveAsListener(object sender, RoutedEventArgs e)
        {
            if(SaveAsRule() != null)
            {
                Refresh();
            }
        }

        private void OnClickBtCreateNewRuleListener(object sender, RoutedEventArgs e)
        {
            AddRuleEditView(null);            
        }

        private void OnClickBtCreateNewRuleSetListener(object sender, RoutedEventArgs e)
        {
            Window window = new Window();
            window.Width = 350;
            window.Height = 150;
            InputDialog inputDialog = new InputDialog("enter folder name","folder name");
            window.Content = inputDialog;
            window.ShowDialog();

            //create rule set
            if (inputDialog.HasInput)
            {
                string folderName = inputDialog.Input;
                string filePath = StaticValue.RULE_FOLDER_PATH + "\\" + inputDialog.Input;
                //check if folder is exsts or not
                if (Directory.Exists(Path.GetFullPath(filePath)))
                {
                    MessageBox.Show("the rule set is exists , please use other name ");
                }
                else
                {
                    //add rule set
                    RuleSet ruleSet = new RuleSet(ruleMetadata.GetNextRuleSetId(), folderName);
                    ruleMetadata.AddRuleSet(ruleSet);
                    //create folder
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(filePath);
                    MessageBox.Show("create rule set " + ruleSet.Name + " successfully");
                    //refresh list
                    Refresh();
                }                
            }      
        }

        private void OnClickBtRuleEditBeforeClearListener(object sender, RoutedEventArgs e)
        {
            ruleBefore.Document.Blocks.Clear();
            Paragraph result = new Paragraph();
            result.Margin = new Thickness(0, 0, 0, 0);
            ruleBefore.Document.Blocks.Add(result);
            if (ruleAfter.Document.Blocks.Count == 0)
            {
                InitAllBlockList();
            }
        }

        private void OnClickBtRuleEditAfterClearListener(object sender, RoutedEventArgs e)
        {
            ruleAfter.Document.Blocks.Clear();
            Paragraph result = new Paragraph();
            result.Margin = new Thickness(0, 0, 0, 0);
            ruleAfter.Document.Blocks.Add(result);
            if (ruleBefore.Document.Blocks.Count == 0)
            {
                InitAllBlockList();
            }
        }

        private void OnClickBtRuleEditCopyListener(object sender, RoutedEventArgs e)
        {
            foreach (Paragraph paragraph in ruleBefore.Document.Blocks)
            {
                Paragraph copy = new Paragraph();
                copy.Margin = new Thickness(0, 0, 0, 0);
                foreach (Run run in paragraph.Inlines)
                {
                    Run newRun = new Run(run.Text, copy.ContentEnd);
                    newRun.Background = run.Background;
                    newRun.Foreground = run.Foreground;
                    newRun.DataContext = run.DataContext;
                }
                ruleAfter.Document.Blocks.Add(copy);
            }
        }

        //-----list click listener-----
        private void OnDoubleClickRuleListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem rule = (TreeViewItem)sender;
            ruleSetOpenNow = (rule.Parent as TreeViewItem).DataContext as RuleSet;
            string path = (string)rule.DataContext;
            AddRuleEditView(path);
        }
         
        private void OnDoubleClickParameterListListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            ParameterBlock parameter = treeViewItem.DataContext as ParameterBlock;
            ResetEditStatus();
            parameterEditNow = parameter;
        }

        private void OnDoubleClickCodeBlockListListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            CodeBlock codeBlock = treeViewItem.DataContext as CodeBlock;
            ResetEditStatus();
            codeBlockEditNow = codeBlock;
        }

        private void OnDoubleClickIncludeListListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            IncludeBlock includeBlock = treeViewItem.DataContext as IncludeBlock;
            ResetEditStatus();
            IncludeBlockEditNow = includeBlock;
        }

        //-----menu listener-----
        private void OnMenuSetParameterChooseListener(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            RichTextBox richTextBox = ((item.Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as RichTextBox;

            if (richTextBox.IsSelectionActive)
            {
                TextSelection selection = richTextBox.Selection;
                selection.Start.DeleteTextInRun(selection.Start.GetOffsetToPosition(selection.End));
                int paraId = paraList.Count + 1;
                ParameterBlock parameterBlock = new ParameterBlock("", paraId);
                AddIntoParaList(parameterBlock);
                SetParameterFormate(paraId, selection.Start);
                RefreshParameterTreeView();
            }
        }

        private void OnMenuSetCodeBlockChooseListener(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            RichTextBox richTextBox = ((item.Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as RichTextBox;

            if (richTextBox.IsSelectionActive)
            {
                TextSelection selection = richTextBox.Selection;
                selection.Start.DeleteTextInRun(selection.Start.GetOffsetToPosition(selection.End));
                int blockId = codeBlockList.Count + 1;
                AddIntoCodeBlockList(new CodeBlock("", blockId));

                SetBlockFormate(blockId, selection.Start);
                RefreshCodeBlockTreeView();
            }
        }

        private void OnMenuCopyRuleClickListener(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            TreeViewItem treeViewItem = ((item.Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as TreeViewItem;

            string rulePath = treeViewItem.DataContext as string;
            //copy rule
            RuleBlock rule = fileLoader.LoadSingleRuleByPath(rulePath);
            string before = rule.GetOrgText("before");
            string after = rule.GetOrgText("after");
            int ruleId = ruleSetOpenNow.GetNextRuleId();

            CopyRule(before,after,ruleId,rule.RuleName);
        }

        //------drag and drop listener------
        private void OnTextBoxDragOverListener(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            RichTextBox item = sender as RichTextBox;

            Point currentPosition = e.GetPosition(item);

            if (item != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void OnTextBoxDropListener(object sender, DragEventArgs e)
        {
            RichTextBox textBox = sender as RichTextBox;
            TextPointer textPointer = textBox.GetPositionFromPoint(e.GetPosition(textBox), true);
           
            if (e.Data.GetDataPresent("para"))
            {
                ParameterBlock parameterBlock = e.Data.GetData("para") as ParameterBlock;
                SetParameterFormate(parameterBlock.ParaListIndex, textPointer);
                ResetEditStatus();
            }
            else if (e.Data.GetDataPresent("codeBlock"))
            {
                CodeBlock codeBlock = e.Data.GetData("codeBlock") as CodeBlock;
                SetBlockFormate(codeBlock.BlockListIndex, textPointer);
                ResetEditStatus();
            }
            else if (e.Data.GetDataPresent("include rule"))
            {
                IncludeBlock includeBlock = e.Data.GetData("include rule") as IncludeBlock;
                int codeBlockId = includeBlock.IncludeBlockListIndex;
                int compareRuleId = includeBlock.CompareRuleId;
                int fromRuleSetId = includeBlock.FromRuleSetId;

                string tag;
                if (textBox == ruleBefore)
                {
                    tag = "before";
                }
                else
                {
                    tag = "after";
                }

                if (IsIncludeRuleContain(fromRuleSetId, compareRuleId, ruleSetOpenNow.Id, ruleBlockEditNow.RuleId, tag))
                {
                    MessageBox.Show("cannot include this rule ,it will include itself");
                }
                else
                {
                    textPointer.InsertTextInRun("\n<include id=\"" + codeBlockId + "\" compareRuleId=\"" + compareRuleId + "\" fromRuleSetId=\"" + fromRuleSetId + "\"/>");
                    SetRuleEditView(ChangeToText(ruleBefore), ChangeToText(ruleAfter));
                }
                ResetEditStatus();
            }
            e.Handled = true;
        }

        private void OnParaListMouseMoveListener(object sender, MouseEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(item);

                if ((Math.Abs(currentPosition.X - startDragPoint.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - startDragPoint.Y) > 10.0))
                {
                    DataObject data = new DataObject();

                    parameterEditNow = item.DataContext as ParameterBlock;
                    data.SetData("para", parameterEditNow);

                    if (data != null)
                    {
                        DragDropEffects dropEffects = DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void OnParaListMouseDownListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.ChangedButton == MouseButton.Left)
            {
                startDragPoint = e.GetPosition(item);
            }
        }

        private void OnBlockMouseMoveListener(object sender, MouseEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(item);

                if ((Math.Abs(currentPosition.X - startDragPoint.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - startDragPoint.Y) > 10.0))
                {
                    DataObject data = new DataObject();

                    codeBlockEditNow = item.DataContext as CodeBlock;
                    data.SetData("codeBlock", codeBlockEditNow);

                    if (data != null)
                    {
                        DragDropEffects dropEffects = DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void OnBlockMouseDownListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.ChangedButton == MouseButton.Left)
            {
                startDragPoint = e.GetPosition(item);
            }
        }

        private void OnIncludeBlockMouseMoveListener(object sender, MouseEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(item);

                if ((Math.Abs(currentPosition.X - startDragPoint.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - startDragPoint.Y) > 10.0))
                {
                    DataObject data = new DataObject();

                    IncludeBlockEditNow = item.DataContext as IncludeBlock;
                    data.SetData("include rule", IncludeBlockEditNow);

                    if (data != null)
                    {
                        DragDropEffects dropEffects = DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void OnIncludeBlockMouseDownListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.ChangedButton == MouseButton.Left)
            {
                startDragPoint = e.GetPosition(item);
            }
        }

        private void OnRuleListMouseMoveListener(object sender, MouseEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(item);

                if ((Math.Abs(currentPosition.X - startDragPoint.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - startDragPoint.Y) > 10.0))
                {
                    DataObject data = new DataObject();

                    string rulePath = item.DataContext as string;
                    RuleBlock rule = fileLoader.LoadSingleRuleByPath(rulePath);
                    string[] split = Path.GetFullPath(rulePath).Split('\\');
                    string ruleSetName = split[split.Length - 2];// ruleSetName(-2) \\ ruleName.xml (-1)
                    IncludeBlock includeBlock = new IncludeBlock();
                    includeBlock.IncludeBlockListIndex = includeList.Count + 1;
                    includeBlock.FromRuleSetId = ruleMetadata.GetRuleSetByName(ruleSetName).Id;
                    includeBlock.CompareRuleId = rule.RuleId;
                    includeBlock.BeforeList = rule.BeforeRuleSliceList;
                    includeBlock.AfterList = rule.AfterRuleSliceList;
                    data.SetData("include rule", includeBlock);

                    if (data != null)
                    {
                        DragDropEffects dropEffects = DragDrop.DoDragDrop(item, data, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void OnRuleListMouseDownListener(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null && e.ChangedButton == MouseButton.Left)
            {
                startDragPoint = e.GetPosition(item);
            }
        }
    }
}