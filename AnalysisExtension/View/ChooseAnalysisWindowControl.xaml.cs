using AnalysisExtension.AnalysisMode;
using AnalysisExtension.Model;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public partial class ChooseAnalysisWindowControl : UserControl
    {
        private UserControl previousControl = null;

        private List<FileTreeNode> chooseFile;
        private List<Analysis> analysisListElement;
        private List<string> type;
        private Analysis chooseAnalysis;
        private bool hasChoose = false;

        //----------------set------------------------
        public ChooseAnalysisWindowControl(List<FileTreeNode> chooseFile, UserControl previousControl)
        {
            this.previousControl = previousControl;
            this.chooseFile = chooseFile;
            chooseAnalysis = null;

            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            analysisListElement = new List<Analysis>();
            type = StaticValue.GetFileType(chooseFile);

            SetTextInfo();
            SetAnalysisList();
            hasChoose = false;
        }

        private void SetTextInfo()
        {
            choose_analysis_info_tb.Text = "Select the analysis method that want to do.";            

            if (type.Count > 0)
            {
                string info = "\nType of Choose File :";

                for(int i = 0;i < type.Count;i++)
                {
                    info += " " + type[i] + " ";
                }

                choose_analysis_info_tb.Text += info;
            }
        }

        private void SetAnalysisList()
        {
            //TODO : get real list

            for (int i = 0; i < 10; i++)
            {
                string s = "FtoC" + i;
                FtoC element = new FtoC(s);

                //check type
                bool isFind = false;
                foreach (string t in type)
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
            StaticValue.WINDOW.Content = new CheckInfoWindowControl(chooseFile, chooseAnalysis, this);
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
