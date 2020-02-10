namespace AnalysisExtension
{
    using AnalysisExtension.Model;
    using AnalysisExtension.Tool;
    using AnalysisExtension.View;
    using AnalysisExtension.View.CreateRule;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Xml;

    public partial class CreateRuleToolWindow1Control : UserControl
    {        
        private FileLoader fileLoader = FileLoader.GetInstance();
        private RuleMetadata ruleMetadata = RuleMetadata.GetInstance();

        private RuleSet ruleSetOpenNow;
        private RuleBlock ruleBlockEditNow;
        private string ruleNameOpenNow;

        private RichTextBox ruleBefore = new RichTextBox();
        private RichTextBox ruleAfter = new RichTextBox();
        private TextBlock ruleSetName = new TextBlock();

        private bool IsEditViewChange = false;

        public CreateRuleToolWindow1Control()
        {            
            this.InitializeComponent();
            Refresh();
            SizeChanged += OnSizeChanged;
        }

        private void Refresh()
        {  
            RefreshRuleSetListView();
            //  ruleCreateStackPanel.Children.Clear();
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
                RefreshRuleList();
            }
        }

        private void RefreshRuleList()
        {
            ruleListTreeView.Items.Clear();
            AddRuleListIntoTreeViewByName(ruleSetOpenNow);
        }

        //-----tool-----
        private Paragraph ChangeToColor(string orgText)
        {
            int blockCount = 1;// index/number of <block> in <layer>
            int paraCount = 1;// index/number of <para> in <layer>

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<xml xml:space=\"" + "preserve\"" + ">" + orgText + "</xml>");

            Paragraph result = new Paragraph();
            result.Margin = new Thickness(0, 0, 0, 0);

            while (orgText.IndexOf("<block") > -1 || orgText.IndexOf("<para") > -1)
            {
                string findResult = null;
                if (orgText.IndexOf("<block") > -1 && 
                    ( (orgText.IndexOf("<para") <= -1) || (orgText.IndexOf("<para") > -1 && orgText.IndexOf("<block") < orgText.IndexOf("<para"))))
                {//is block
                    findResult = ChangeBlockToColor(result,xmlDocument,orgText,blockCount);
                    blockCount++;
                }
                else if (orgText.IndexOf("<para") > -1 &&
                    ((orgText.IndexOf("<block") <= -1) || (orgText.IndexOf("<block") > -1 && orgText.IndexOf("<para") < orgText.IndexOf("<block"))))
                {//is para
                    findResult = ChangeParameterToColor(result, xmlDocument, orgText, paraCount);
                    paraCount++;
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
            return result;
        }

        private string ChangeToText(RichTextBox textBox)
        {
            string result = "";

            foreach (Paragraph paragraph in textBox.Document.Blocks)
            {
                foreach (Run run in paragraph.Inlines)
                {
                    //TODO : add include block 
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

            return result;
        }

        private string ChangeBlockToColor(Paragraph result, XmlDocument xmlDocument, string orgText,int blockCount)
        {
            int startIndex = orgText.IndexOf("<block");
            int endIndex = orgText.Substring(startIndex).IndexOf("/>") + startIndex - 1;
            int endTokenLen = 2;                

            XmlElement blockElement = StaticValue.FindElementByTag(xmlDocument, blockCount, "block", "");
            string codeBlockString = orgText.Substring(startIndex, endIndex - startIndex + endTokenLen+1);

            if (blockElement == null)
            {
                return null;
            }
            else
            {
                string frontContent = orgText.Substring(0, startIndex);
                Run front = new Run(frontContent, result.ContentEnd);

                int codeBlockId = int.Parse(StaticValue.GetAttributeInElement(blockElement, "id"));
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

            string codeBlockString = orgText.Substring(startIndex, endIndex - startIndex + endTokenLen + 1);

            if (paraElement == null)
            {
                return null;
            }
            else
            {
                string frontContent = orgText.Substring(0, startIndex);
                Run front = new Run(frontContent, result.ContentEnd);

                int paraId = int.Parse(StaticValue.GetAttributeInElement(paraElement, "id"));
                Run run = new Run("(" + paraId + ")", result.ContentEnd);
                run.Foreground = SystemColors.HighlightBrush;

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
                SetRuleEditView(beforeContent, afterContent);
                IsEditViewChange = false;
            }
        }

        private void SetRuleEditView(string beforeContent, string afterContent)
        {
            IsEditViewChange = false;
            ruleCreateStackPanel.Children.Clear();

            SetEditTextBoxTemplate(ruleBefore, beforeContent);
            SetEditTextBoxTemplate(ruleAfter, afterContent);

            ruleSetName.Text = "rule set edit now : " + ruleSetOpenNow.Name;
            ruleSetName.HorizontalAlignment = HorizontalAlignment.Center;
            ruleCreateStackPanel.Children.Add(ruleSetName);
            ruleCreateStackPanel.Children.Add(ruleBefore);
            ruleCreateStackPanel.Children.Add(new Label());
            ruleCreateStackPanel.Children.Add(ruleAfter);
        }

        private void SetEditTextBoxTemplate(RichTextBox textBox, string content)
        {
            textBox.Document.Blocks.Clear();
            textBox.AcceptsReturn = true;
            textBox.AcceptsTab = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.Background = SystemColors.WindowBrush;
            textBox.Document.Blocks.Add(ChangeToColor(content));
            textBox.TextChanged += new TextChangedEventHandler(OnTextBoxChangedListener);
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

        private void OnTextBoxChangedListener(object sender, TextChangedEventArgs e)
        {
            IsEditViewChange = true;
        }
    }
}