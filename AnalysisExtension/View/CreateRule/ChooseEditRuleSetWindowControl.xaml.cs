using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnalysisExtension.View.CreateRule
{
    /// <summary>
    /// ChooseEditRuleSetWindowControl.xaml 的互動邏輯
    /// </summary>
    public partial class ChooseEditRuleSetWindowControl : UserControl
    {
        public bool IsCheck { get; set; }
        public RuleSet chooseRuleSet { get; set; }

        private RuleMetadata ruleMetadata = RuleMetadata.GetInstance();

        public ChooseEditRuleSetWindowControl()
        {           
            InitializeComponent();
            SetRuleSetList();
        }

        private void SetRuleSetList()
        {
            ruleSetSelectList.ItemsSource = ruleMetadata.GetRuleSetList();
        }

        private void OnClickBtOKListener(object sender, RoutedEventArgs e)
        {
            if (IsCheck)
            {                
                StaticValue.CloseWindow(this);
            }
            else
            {
                MessageBox.Show("not choose rule set yet.");
            }
        }

        private void OnRuleSetCheckListener(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            IsCheck = true;
            chooseRuleSet = checkBox.DataContext as RuleSet;
        }

        private void OnRuleSetUnCheckListener(object sender, RoutedEventArgs e)
        {
            IsCheck = false;
            chooseRuleSet = null;
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            IsCheck = false;
            chooseRuleSet = null;
            StaticValue.CloseWindow(this);
        }
    }
}
