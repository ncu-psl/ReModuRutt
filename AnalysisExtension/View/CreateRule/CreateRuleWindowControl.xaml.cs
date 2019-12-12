namespace AnalysisExtension
{
    using AnalysisExtension.Model;
    using AnalysisExtension.Tool;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class CreateRuleToolWindow1Control : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRuleToolWindow1Control"/> class.
        /// </summary>
        private string[][] ruleList;
        private FileLoader fileLoader = FileLoader.GetInstance();
        private string[] ruleNameList;
        private int ruleSetIndexOpenNow;
        private string ruleNameOpenNow;

        private TextBox ruleBefore = new TextBox();
        private TextBox ruleAfter = new TextBox();

        private RuleBlock ruleBlockEditNow;

        public CreateRuleToolWindow1Control()
        {            
            this.InitializeComponent();
            Refresh();
            SizeChanged += OnSizeChanged;
        }

        private void Refresh()
        {       
            
            ruleSetIndexOpenNow = -1;

            ruleNameOpenNow = null;
            SetRuleListView();
            //TODO : set rule set index open now

        }

        private void SetRuleListView()
        {
            //TODO : clean
            allRuleSetTreeView.Items.Clear();
            ruleNameList = fileLoader.GetAllFileName(StaticValue.RULE_FOLDER_PATH);
            ruleList = new string[ruleNameList.Length][];

            for (int i = 0; i < ruleNameList.Length; i++)
            {
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Header = StaticValue.GetNameFromPath(ruleNameList[i]);
                allRuleSetTreeView.Items.Add(treeViewItem);
                ruleList[i] = fileLoader.GetRuleListByPath(ruleNameList[i]);
                AddRuleListIntoTreeViewByName(treeViewItem, ruleList[i]);
            }
        }

        private void AddRuleListIntoTreeViewByName(TreeViewItem treeViewItem,string[] ruleList)
        {       
            for (int i = 0; i < ruleList.Length; i++)
            {
                TreeViewItem rule = new TreeViewItem();
                rule.Header = StaticValue.GetNameFromPath(ruleList[i]);
                rule.DataContext = ruleList[i];
                rule.MouseDoubleClick += OnDoubleClickRuleListListener;
                treeViewItem.Items.Add(rule);
            }            
        }

        private void AddRuleEditView(string filePath)
        {
            string beforeContent = "";
            string afterContent = "";
            if (filePath != null)
            {
                ruleBlockEditNow = fileLoader.LoadSingleRuleByPath(filePath);
                ruleNameOpenNow = ruleBlockEditNow.RuleName;
                beforeContent = ruleBlockEditNow.GetOrgText("before");
                afterContent = ruleBlockEditNow.GetOrgText("after");
                whitespaceIgnoreCheckBox.IsChecked = ruleBlockEditNow.CanSpaceIgnore;
                //TODO : set rule id
            }

            SetRuleEditView(beforeContent, afterContent);
        }

        private void SetRuleEditView(string beforeContent,string afterContent)
        {
            ruleCreateStackPanel.Children.Clear();

            SetEditTextBoxTemplate(ruleBefore, beforeContent);
            SetEditTextBoxTemplate(ruleAfter, afterContent);

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
        }


        //-----tool-----
        private string GetFilePathInRuleSet(string name,int ruleSetIndex)
        {
            string filePath = null;
            for (int i = 0; i < ruleList[ruleSetIndex].Length; i++)
            {
                if (ruleList[ruleSetIndex][i].Equals(name))
                {
                    return ruleList[ruleSetIndex] + "//" + name;
                }
            }
            return filePath;
        }

        private string GetFinalRule()
        {
            int id = -1;//TODO : get by Metadata 
            string head = @"<rule id=" + "\"" + id +"\"" + " name=" + "\"" + ruleNameOpenNow + "\"" + @">"+"\n" ;
            string before = @"<before>" + "\n" + ruleBefore.Text + "\n" + @"</before>"+"\n";
            string after = @"<after>" + "\n" + ruleAfter.Text + "\n" + @"</after>"+"\n";
            string end = @"</rule>";


            return head + before + after + end;
        }

        //listener
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWindowHeight = e.NewSize.Height;
            double newWindowWidth = e.NewSize.Width;
            int padding = 20;
            ruleCreateStackPanel.Width = newWindowWidth - ruleTreeView.ActualWidth;
            ruleCreateStackPanel.Height = newWindowHeight - padding;
            ruleBefore.Height = ruleCreateStackPanel.Height / 2 - padding;
            ruleBefore.Width = ruleCreateStackPanel.Width - padding;
            ruleAfter.Height = ruleCreateStackPanel.Height / 2 - padding;
            ruleAfter.Width = ruleCreateStackPanel.Width - padding;
        }
        
        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
        }

        private void OnClickBtSaveListener(object sender, RoutedEventArgs e)
        {
            if (ruleNameOpenNow == null)
            {
                //enter rule name                
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "(*.xml)|*.xml";
                saveFileDialog.Title = "save as";
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    ruleNameOpenNow = StaticValue.GetNameFromPath(saveFileDialog.FileName).Split('.')[0];
                    string final = GetFinalRule();
                    FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
                    fileStream.Write(Encoding.ASCII.GetBytes(final), 0, Encoding.ASCII.GetByteCount(final));
                    fileStream.Close();
                    MessageBox.Show("file save");
                }
            }
            else if (GetFilePathInRuleSet(ruleNameOpenNow,ruleSetIndexOpenNow) != null)
            {
                /*TODO : check and save*/
                MessageBoxResult result = MessageBox.Show("file is exist , sure to overwrite this file?", "Save File", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    string path = GetFilePathInRuleSet(ruleNameOpenNow, ruleSetIndexOpenNow);
                   // File.WriteAllText(path, /*text*/);
                    MessageBox.Show("file save");
                }
            }
            Refresh();
        }

        private void OnClickBtCreateNewListener(object sender, RoutedEventArgs e)
        {
            string filePath = null;
            ruleNameOpenNow = null;
            AddRuleEditView(filePath);          
        }

        private void OnDoubleClickRuleListListener(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem rule = (TreeViewItem)sender;
            string path = (string)rule.DataContext;
            AddRuleEditView(path);
        }
    }
}