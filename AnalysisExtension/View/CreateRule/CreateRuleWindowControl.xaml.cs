namespace AnalysisExtension
{
    using AnalysisExtension.Model;
    using AnalysisExtension.Tool;
    using AnalysisExtension.View;
    using AnalysisExtension.View.CreateRule;
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
        private TextBlock ruleSetName = new TextBlock();

        private List<ParameterBlock> paraList;
        private List<CodeBlock> codeBlockList;
        private int newIncludeId = -1;

        private bool IsEditViewChange = false;
        private ParameterBlock parameterEditNow = null;
        private CodeBlock codeBlockEditNow = null;

        private Point startDragPoint;
        private Point endDragPoint;

        private static CreateRuleToolWindowControl instance = null;

        public static CreateRuleToolWindowControl GetInstance()
        {
            if (instance == null)
            {
                instance = new CreateRuleToolWindowControl();
            }
            return instance;
        }

        public static CreateRuleToolWindowControl CreateNewFrame()
        {
            instance = new CreateRuleToolWindowControl();
            return instance;
        }


        private CreateRuleToolWindowControl()
        {
            this.InitializeComponent();
            Refresh();
            SizeChanged += OnSizeChanged;
        }

        private void Refresh()
        {  
            RefreshRuleSetListView();
            paraList = new List<ParameterBlock>();
            codeBlockList = new List<CodeBlock>();

            if (ruleSetOpenNow != null)
            {
                ruleCreateStackPanel.Children.Clear();

                ruleSetName.Text = "rule set edit now : " + ruleSetOpenNow.Name;
                ruleSetName.HorizontalAlignment = HorizontalAlignment.Center;
                ruleCreateStackPanel.Children.Add(ruleSetName);
            }
        }

        private void RefreshRuleSetListView()
        {
            ruleMetadata.Refresh();

            allRuleSetTreeView.Items.Clear();
            for (int i = 0; i < ruleMetadata.GetRuleSetList().Count; i++)
            {
                RuleSet ruleSet = ruleMetadata.GetRuleSetList()[i];
                TreeViewItem ruleSetTreeView = new TreeViewItem();
                ruleSetTreeView.Header = StaticValue.GetNameFromPath(ruleSet.Name);
                /*store ruleSet in ruleSetTreeView*/
                ruleSetTreeView.DataContext = ruleSet;
                ruleSetTreeView.MouseDoubleClick += OnDoubleClickRuleListListener;

                allRuleSetTreeView.Items.Add(ruleSetTreeView);
            }

            if (ruleSetOpenNow != null)
            {
                ruleSetOpenNow = ruleMetadata.GetRuleSetById(ruleSetOpenNow.Id);
                paraList = new List<ParameterBlock>();
                codeBlockList = new List<CodeBlock>();
                RefreshRuleList();
            }
        }

        private void RefreshRuleList()
        {
            ruleListTreeView.Items.Clear();
            AddRuleListIntoTreeViewByName(ruleSetOpenNow);
        }

        private void ResetEditStatus()
        {
            parameterEditNow = null;
            codeBlockEditNow = null;
        }
        //-----tool-----       

        public void AddTextIntoRuleCreateFrame(string selectContent)
        {
            ruleBefore.Document.Blocks.AddRange(ChangeToColor(selectContent,"before"));
        }

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
                    string frontContent = orgText.Substring(0, orgText.IndexOf("<include"));
                    Run front = new Run(frontContent, result.ContentEnd);

                    allResult.Add(result);
                    result = new Paragraph();
                    result.Margin = new Thickness(0, 0, 0, 0);
                    result.Background = SystemColors.MenuBarBrush;
                    findResult = ChangeIncludeToColor(tag,result, xmlDocument, orgText, includeCount);
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

        private string ChangeToText(RichTextBox textBox)
        {
            string result = "";

            foreach (Paragraph paragraph in textBox.Document.Blocks)
            {
                if (paragraph.Background == SystemColors.MenuBarBrush)
                {//is include block
                    result += paragraph.DataContext;
                }
                else
                {
                    foreach (Run run in paragraph.Inlines)
                    {
                        if (run.Background == SystemColors.HighlightBrush)
                        {//is block
                            result += Regex.Replace(run.Text, "[(]" + @"(\d+)" + "[)]", "<block id=" + "\"$1\"/>");
                        }
                        else if (run.Foreground == SystemColors.HighlightBrush)
                        {//is parameter
                            result += Regex.Replace(run.Text, "[(]" + @"(\d+)" + "[)]", "<para id=" + "\"$1\"/>");
                        }
                        else
                        {
                            result += run.Text;
                        }
                    }
                }
            }

            return result;
        }

        private string ChangeBlockToColor(Paragraph result, XmlDocument xmlDocument, string orgText,int blockCount)
        {
            int startIndex = orgText.IndexOf("<block");
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
                AddIntoCodeBlockList(new CodeBlock("",codeBlockId));

                Run run = new Run("(" + codeBlockId + ")", result.ContentEnd);
                run.Background = SystemColors.HighlightBrush;

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
                AddIntoParaList(new ParameterBlock("", paraId));

                Run run = new Run("(" + paraId + ")", result.ContentEnd);
                run.Foreground = SystemColors.HighlightBrush;
                orgText = orgText.Substring(endIndex + endTokenLen + 1);
            }
            return orgText;
        }

        private string ChangeIncludeToColor(string tag,Paragraph result, XmlDocument xmlDocument, string orgText, int includeCount)
        {
            int startIndex = orgText.IndexOf("<include");
            int endIndex = orgText.Substring(startIndex).IndexOf("/>") + startIndex - 1;
            int endTokenLen = 2;

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

                Run run = new Run("<by rule set " + ruleSet.Name + ", rule "+ruleSet.GetRuleInfoById(compareRuleId)["name"] + ">\n", result.ContentEnd);
                result.DataContext = "<include id=\"" + codeBlockId + "\" compareRuleId=\"" + compareRuleId + "\" fromRuleSetId=\"" + fromRuleSetId + "\"/>";

                RuleBlock rule = fileLoader.LoadSingleRuleByPath(ruleMetadata.GetRulePathById(fromRuleSetId, compareRuleId));
                List<Paragraph> interResult = ChangeToColor(rule.GetOrgText(tag),tag);

                foreach (Paragraph paragraph in interResult)
                {
                    foreach (Run inline in paragraph.Inlines)
                    {
                        Run newRun = new Run(inline.Text, result.ContentEnd);
                        newRun.Background = inline.Background;
                        newRun.Foreground = inline.Foreground;
                    }
                }

                if (newIncludeId <= codeBlockId)
                {
                    newIncludeId = codeBlockId + 1;
                }
                orgText = orgText.Substring(endIndex + endTokenLen + 1);
            }
            return orgText;

        }

        private string RemoveLineAtFirstAndEnd(string orgText)
        {
            if (orgText.StartsWith("\r\n"))
            {
                orgText = orgText.Remove(0, 2);
            }
            else if(orgText.StartsWith("\n"))
            {
                orgText = orgText.Remove(0, 1);
            }

            if(orgText.EndsWith("\r\n"))
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

        private void AddRuleListIntoTreeViewByName(RuleSet ruleSet)
        {
            foreach (Dictionary<string, string> ruleContent in ruleSet.RuleList)
            {
                TreeViewItem rule = new TreeViewItem();
                rule.Header = ruleContent["name"];
                rule.DataContext = GetFilePathInRuleSet(ruleContent["name"], ruleSet);
                rule.MouseDoubleClick += OnDoubleClickRuleListener;
                ruleListTreeView.Items.Add(rule);
            }
        }

        private void RefreshParameterTreeView()
        {
            paraListTreeView.Items.Clear();
            foreach (ParameterBlock parameterBlock in paraList)
            {
                TreeViewItem para = new TreeViewItem();
                para.Header = "(" + parameterBlock.ParaListIndex +")";
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
                block.Header = "("+ codeBlock.BlockListIndex +")";
                block.DataContext = codeBlock;
                block.MouseDoubleClick += OnDoubleClickCodeBlockListListener;
                block.MouseDown += OnBlockMouseDownListener;
                block.MouseMove += OnBlockMouseMoveListener;
                blockListTreeView.Items.Add(block);
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
                    SaveNewRule();
                }
                else if (result == MessageBoxResult.No)
                {
                    ruleBefore.Document.Blocks.Clear();
                    ruleAfter.Document.Blocks.Clear();
                }
                else
                {
                    return;
                }
            }

            if (filePath != null)
            {
                ruleBlockEditNow = fileLoader.LoadSingleRuleByPath(filePath);
                ruleNameOpenNow = ruleBlockEditNow.RuleName;
                beforeContent = RemoveLineAtFirstAndEnd(ruleBlockEditNow.GetOrgText("before"));
                afterContent = RemoveLineAtFirstAndEnd(ruleBlockEditNow.GetOrgText("after"));
                whitespaceIgnoreCheckBox.IsChecked = ruleBlockEditNow.CanSpaceIgnore;
            }
            else if (ruleSetOpenNow != null)
            {
                MessageBoxResult result = MessageBox.Show("create new rule in rule set " + ruleSetOpenNow.Name, "create new rule", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            else
            {
                Window window = new Window();
                window.Width = 200;
                window.Height = 200;
                ChooseEditRuleSetWindowControl chooseEditRuleSetWindow = new ChooseEditRuleSetWindowControl();
                window.Content = chooseEditRuleSetWindow;
                window.ShowDialog();
                ruleSetOpenNow = chooseEditRuleSetWindow.chooseRuleSet;
            }

            if (ruleSetOpenNow != null)
            {
                paraList = new List<ParameterBlock>();
                codeBlockList = new List<CodeBlock>();
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
            
            ruleSetName.Text = "rule set edit now : " + ruleSetOpenNow.Name;
            ruleSetName.HorizontalAlignment = HorizontalAlignment.Center;
            ruleCreateStackPanel.Children.Add(ruleSetName);
            ruleCreateStackPanel.Children.Add(ruleBefore);
            ruleCreateStackPanel.Children.Add(new Label());
            ruleCreateStackPanel.Children.Add(ruleAfter);

            RefreshParameterTreeView();
            RefreshCodeBlockTreeView();
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

        private string GetFilePathInRuleSet(string name,RuleSet ruleSet)
        {
            return StaticValue.RULE_FOLDER_PATH + "\\" + ruleSet.Name + "\\" + name + ".xml";
        }

        private void AddRuleCreateNowIntoMetadata(int ruleId)
        {
            ruleMetadata.AddRuleIntoRuleSet(ruleSetOpenNow.Id, ruleId, ruleNameOpenNow);
            ruleMetadata.RewriteMetadata();
            RefreshRuleSetListView();
        }

        private string GetFinalRule(int ruleId)
        {
            string beforeText = ChangeToText(ruleBefore);//ChangeBlockToXmlText(new TextRange(ruleBefore.Document.ContentStart, ruleBefore.Document.ContentEnd).Text);
            beforeText = RemoveLineAtFirstAndEnd(beforeText);
            string afterText = ChangeToText(ruleAfter);//ChangeBlockToXmlText(new TextRange(ruleAfter.Document.ContentStart, ruleAfter.Document.ContentEnd).Text);
            afterText = RemoveLineAtFirstAndEnd(afterText);
            string head = @"<rule xml:space=" + "\"preserve\" " + "id=" + "\"" + ruleId + "\"" + " name=" + "\"" + ruleNameOpenNow + "\"" + " canWhitespaceIgnore=" + "\"" + whitespaceIgnoreCheckBox.IsChecked.ToString() +  "\"" + @">"+"\n" ;
            string before = @"<before>" + "\n" + beforeText + "\n" + @"</before>"+"\n";
            string after = @"<after>" + "\n" + afterText + "\n" + @"</after>"+"\n";
            string end = @"</rule>";

            return head + before + after + end;
        }

        private void SaveNewRule()
        {
            string final;
            int ruleId;
            if (ruleNameOpenNow == null)
            {
                ruleId = ruleMetadata.GetNextRuleIdByRuleSetId(ruleSetOpenNow.Id);
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "(*.xml)|*.xml";
                saveFileDialog.Title = "save as";
                saveFileDialog.InitialDirectory = Path.GetFullPath(StaticValue.RULE_FOLDER_PATH + "\\" + ruleSetOpenNow.Name);
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    ruleNameOpenNow = StaticValue.GetNameFromPath(saveFileDialog.FileName).Split('.')[0];
                    final = GetFinalRule(ruleId);

                    FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
                    fileStream.Write(Encoding.ASCII.GetBytes(final), 0, Encoding.ASCII.GetByteCount(final));
                    fileStream.Close();

                    AddRuleCreateNowIntoMetadata(ruleId);
                    IsEditViewChange = false;
                    MessageBox.Show("file save");
                }
            }
            else if (GetFilePathInRuleSet(ruleNameOpenNow, ruleSetOpenNow) != null)
            {
                MessageBoxResult result = MessageBox.Show("file is exist , sure to overwrite this file?", "Save File", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    ruleId = ruleBlockEditNow.RuleId;
                    final = GetFinalRule(ruleId);
                    string path = GetFilePathInRuleSet(ruleNameOpenNow, ruleSetOpenNow);
                    File.WriteAllText(path, final);

                    AddRuleCreateNowIntoMetadata(ruleId);
                    IsEditViewChange = false;
                    MessageBox.Show("file save");
                }
            }
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
        
        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
        }

        private void OnClickBtSaveListener(object sender, RoutedEventArgs e)
        {
            SaveNewRule();
            Refresh();
        }

        private void OnClickBtCreateNewRuleListener(object sender, RoutedEventArgs e)
        {
            string filePath = null;
            ruleNameOpenNow = null;
            AddRuleEditView(filePath);            
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

        private void OnDoubleClickRuleListListener(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem ruleSet = (TreeViewItem)sender;
            ruleSetOpenNow = ruleSet.DataContext as RuleSet;
            Refresh();
        }

        private void OnDoubleClickRuleListener(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem rule = (TreeViewItem)sender;
             string path = (string)rule.DataContext;
             AddRuleEditView(path);
        }

        private void OnDoubleClickParameterListListener(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            ParameterBlock parameter = treeViewItem.DataContext as ParameterBlock;
            ResetEditStatus();
            parameterEditNow = parameter;
        }

        private void OnDoubleClickCodeBlockListListener(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            CodeBlock codeBlock = treeViewItem.DataContext as CodeBlock;
            ResetEditStatus();
            codeBlockEditNow = codeBlock;
        }

        private void OnTextBoxChangedListener(object sender, TextChangedEventArgs e)
        {
            IsEditViewChange = true;
        }
        
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

                Run para = new Run("(" + paraId + ")", selection.Start);
                para.Foreground = SystemColors.HighlightBrush;
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

                Run block = new Run("(" + blockId + ")", selection.Start);
                block.Background = SystemColors.HighlightBrush;
                RefreshCodeBlockTreeView();
            }
        }

        private void OnTextBoxPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RichTextBox textBox = sender as RichTextBox;
            TextSelection selection = textBox.Selection;

            if (parameterEditNow != null)
            {
                Run para = new Run("(" + parameterEditNow.ParaListIndex + ")", selection.Start);
                para.Foreground = SystemColors.HighlightBrush;
                ResetEditStatus();
            }
            else if (codeBlockEditNow != null)
            {
                Run block = new Run("(" + codeBlockEditNow.BlockListIndex + ")", selection.Start);
                block.Background = SystemColors.HighlightBrush;
                ResetEditStatus();
            }
        }

        private void OnTextBoxDragOverListener(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            RichTextBox item = sender as RichTextBox;

            Point currentPosition = e.GetPosition(item);

            if (item != null)
            {
                if (parameterEditNow != null || codeBlockEditNow != null)
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
            e.Handled = true;
        }

        private void OnTextBoxDropListener(object sender, DragEventArgs e)
        {
            RichTextBox textBox = sender as RichTextBox;
            if (e.Data.GetDataPresent("para"))
            {
                ParameterBlock parameterBlock = e.Data.GetData("para") as ParameterBlock;

                Run para = new Run("(" + parameterEditNow.ParaListIndex + ")", textBox.CaretPosition.DocumentEnd);
                para.Foreground = SystemColors.HighlightBrush;
                ResetEditStatus();
            }
            else if (e.Data.GetDataPresent("codeBlock"))
            {
                CodeBlock codeBlock = e.Data.GetData("codeBlock") as CodeBlock;

                Run block = new Run("(" + codeBlockEditNow.BlockListIndex + ")", textBox.CaretPosition.DocumentEnd);
                block.Background = SystemColors.HighlightBrush;
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
    }
}