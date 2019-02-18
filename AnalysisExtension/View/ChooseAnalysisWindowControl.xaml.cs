using AnalysisExtension.AnalysisMode;
using AnalysisExtension.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace AnalysisExtension
{
    public partial class ChooseAnalysisWindowControl : UserControl
    {
        private List<FileTreeNode> chooseFile;
        private List<Analysis> analysisListElement;
        private List<string> type = new List<string>();


        //----------------set------------------------
        public ChooseAnalysisWindowControl(List<FileTreeNode> chooseFile)
        {
            this.chooseFile = chooseFile;
            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            analysisListElement = new List<Analysis>();

            SetTextInfo();
            SetAnalysisList();
        }

        private void SetTextInfo()
        {
            foreach (FileTreeNode node in chooseFile)
            {
                if (node.Type != null && !type.Contains(node.Type))
                {
                    type.Add(node.Type);
                }
            }

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
           
        }

        //-----------Listener---------------------------------------
        private void OnAnalysisChooseListener(object sender, RoutedEventArgs args)
        {
            CheckBox checkBox = sender as CheckBox;
            Analysis analysis = checkBox.DataContext as Analysis;
            analysis.IsChoose = true;
        }

        private void OnAnalysisDisChooseListener(object sender, RoutedEventArgs args)
        {
            CheckBox checkBox = sender as CheckBox;
            Analysis analysis = checkBox.DataContext as Analysis;
            analysis.IsChoose = false;
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            StaticValue.BtCancelListener(sender, e,this);
        }

        private void OnPreviewMouseWheelListener(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender, e);
        }

        private void OnClickBtPreviousListener(object sender, RoutedEventArgs e)
        {
            StaticValue.CloseWindow(this);
            ChooseFileWindowCommand.command.ExecuteStartWin();
        }

        private void OnClickBtNextListener(object sender, RoutedEventArgs e)
        {

        }
    }
}
