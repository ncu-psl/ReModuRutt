using AnalysisExtension.AnalysisMode;
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

        private List<Analysis> analysisListElement;
        private List<string> typeList;
        private Analysis chooseAnalysis;
        private bool hasChoose = false;
        private FileLoader fileLoader = FileLoader.GetInstance();

        //----------------set------------------------
        public ChooseAnalysisWindowControl(UserControl previousControl)
        {
            this.previousControl = previousControl;
            typeList = fileLoader.GetFileType();
            chooseAnalysis = null;

            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            analysisListElement = new List<Analysis>();            

            SetTextInfo();
            SetAnalysisList();
            hasChoose = false;
        }

        private void SetTextInfo()
        {
            choose_analysis_info_tb.Text = "Select the analysis method that want to do.";            

            if (typeList.Count > 0)
            {
                string info = "\nType of Choose File :";

                for(int i = 0;i < typeList.Count;i++)
                {
                    info += " " + typeList[i] + " ";
                }

                choose_analysis_info_tb.Text += info;
            }
        }

        private void SetAnalysisList()
        {
            //TODO : get real list

            for (int i = 0; i < 10; i++)
            {
                string s = "FtoC" ;
                FtoC element = new FtoC(s);

                //check type
                bool isFind = false;
                foreach (string t in typeList)
                {
                    if (element.Type.Contains(t))
                    {
                        isFind = true;
                        break;
                    }
                }

                if (isFind)
                {
                    //add
                    analysisListElement.Add(element);
                }
            }
            
            //set
            analysisList.ItemsSource = analysisListElement;
            analysisList.UnselectAll();
           
        }

        private void ShowNextWindow()
        {
            chooseAnalysis.LoadRuleList();
            AnalysisTool.GetInstance().SetAnalysisMode(chooseAnalysis);
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
            if (!hasChoose)
            {
                CheckBox checkBox = sender as CheckBox;
                Analysis analysis = checkBox.DataContext as Analysis;
                analysis.IsChoose = true;
                hasChoose = true;
                chooseAnalysis = analysis;
            }            
        }

        private void OnAnalysisDisChooseListener(object sender, RoutedEventArgs args)
        {
            if (hasChoose)
            {
                CheckBox checkBox = sender as CheckBox;
                Analysis analysis = checkBox.DataContext as Analysis;
                analysis.IsChoose = false;
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


//TODO : single choose
