namespace AnalysisExtension
{
    using AnalysisExtension.Tool;
    using System.IO;
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

        public CreateRuleToolWindow1Control()
        {
            this.InitializeComponent();
            SizeChanged += OnSizeChanged;
            string[] ruleNameList = fileLoader.GetAllFileName(StaticValue.RULE_FOLDER_PATH);
            ruleList = new string[ruleNameList.Length][];

            for (int i = 0;i < ruleNameList.Length;i++)
            {
                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Header = StaticValue.GetNameFromPath(ruleNameList[i]);
                allRuleSetTreeView.Items.Add(treeViewItem);
                ruleList[i] = fileLoader.GetRuleListByPath(ruleNameList[i]);
                AddRuleListIntoTreeViewByName(treeViewItem,ruleList[i]);
            }
        }

        private void AddRuleListIntoTreeViewByName(TreeViewItem treeViewItem,string[] ruleList)
        {       
            for (int i = 0; i < ruleList.Length; i++)
            {
                TreeViewItem rule = new TreeViewItem();
                rule.Header = StaticValue.GetNameFromPath(ruleList[i]);
                treeViewItem.Items.Add(rule);
            }            
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
            StaticValue.CloseWindow(this);
        }
    }
}