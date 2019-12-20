namespace AnalysisExtension
{
    using AnalysisExtension.Model;
    using AnalysisExtension.Tool;
    using AnalysisExtension.View;
    using AnalysisExtension.View.CreateRule;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    public partial class CreateRuleToolWindow1Control : UserControl
    {        
        //TODO : save whitespaceIgnore
        private FileLoader fileLoader = FileLoader.GetInstance();
        private RuleMetadata ruleMetadata = RuleMetadata.GetInstance();

        private RuleSet ruleSetOpenNow;
        private RuleBlock ruleBlockEditNow;
        private string ruleNameOpenNow;

        private TextBox ruleBefore = new TextBox();
        private TextBox ruleAfter = new TextBox();
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
                    ruleBefore.Text = "";
                    ruleAfter.Text = "";
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
                beforeContent = ruleBlockEditNow.GetOrgText("before");
                afterContent = ruleBlockEditNow.GetOrgText("after");
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

        private void SetRuleEditView(string beforeContent,string afterContent)
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

        private void SetEditTextBoxTemplate(TextBox textBox,string content)
        {
            textBox.AcceptsReturn = true;
            textBox.AcceptsTab = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.Background = SystemColors.WindowBrush;
            textBox.Text = content;
            textBox.TextChanged += new TextChangedEventHandler(OnTextBoxChangedListener);
        }


        //-----tool-----
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
            string head = @"<rule id=" + "\"" + ruleId + "\"" + " name=" + "\"" + ruleNameOpenNow + "\"" + @">"+"\n" ;
            string before = @"<before>" + "\n" + ruleBefore.Text + "\n" + @"</before>"+"\n";
            string after = @"<after>" + "\n" + ruleAfter.Text + "\n" + @"</after>"+"\n";
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
            ruleBefore.Width = ruleCreateStackPanel.Width - padding/2;
            ruleAfter.Height = ruleCreateStackPanel.Height / 2 - padding*1.5;
            ruleAfter.Width = ruleCreateStackPanel.Width - padding/2;
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