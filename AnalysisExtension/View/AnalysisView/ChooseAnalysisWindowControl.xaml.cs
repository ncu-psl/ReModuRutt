using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public partial class ChooseAnalysisWindowControl : UserControl
    {
        private UserControl previousControl = null;

        //private List<Analysis> analysisListElement;
        private RuleSet chooseAnalysis;
        private bool hasChoose = false;
        private FileLoader fileLoader = FileLoader.GetInstance();
        private List<RuleSet> ruleSetList;

        //----------------set------------------------
        public ChooseAnalysisWindowControl(UserControl previousControl)
        {
            this.previousControl = previousControl;
            chooseAnalysis = null;

            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();
            ruleSetList = RuleMetadata.GetInstance().GetRuleSetList();
           // analysisListElement = new List<Analysis>();            

            SetAnalysisList();
            hasChoose = false;
        }
        
        private void SetAnalysisList()
        {
            //set
            analysisList.ItemsSource = ruleSetList;
           // analysisList.UnselectAll();
           
        }

        private void ShowNextWindow()
        {
            AnalysisManager.GetInstance().SetAnalysisMode(new Analysis(chooseAnalysis));
            AnalysisManager.GetInstance().LoadRuleList();
            StaticValue.WINDOW.Content = new CheckInfoWindowControl(this);
        }

        //-----------Listener---------------------------------------

        private void OnClickBtNextListener(object sender, RoutedEventArgs e)
        {
            if (chooseAnalysis == null)
            {
                MessageBox.Show("not choose analysis mode yet.");
            }
            else
            {
                Refresh();
                ShowNextWindow();          
            }
            
        }

        private void OnAnalysisChooseListener(object sender, RoutedEventArgs args)
        {
            foreach (RuleSet ruleSet in ruleSetList)
            {
                ruleSet.IsChoose = false;
            }

            if (!hasChoose)
            {
                CheckBox checkBox = sender as CheckBox;
                chooseAnalysis = checkBox.DataContext as RuleSet;
                chooseAnalysis.IsChoose = true;
                hasChoose = true;
            }
        }

        private void OnAnalysisDisChooseListener(object sender, RoutedEventArgs args)
        {
            if (hasChoose)
            {
                CheckBox checkBox = sender as CheckBox;
                chooseAnalysis = checkBox.DataContext as RuleSet;
                chooseAnalysis.IsChoose = false;
                hasChoose = false;
                chooseAnalysis = null;
            }            
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.BtCancelListener(sender, e,this);
        }

        private void OnPreviewMouseWheelListener(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender, e);
        }

        private void OnClickBtPreviousListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.WINDOW.Content = this.previousControl;
        }
    }
}

