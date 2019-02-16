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

        //----------------set------------------------
        public ChooseAnalysisWindowControl(List<FileTreeNode> chooseFile)
        {
            InitializeComponent();
            this.chooseFile = chooseFile;
            SetTextInfo();
        }

        private void SetTextInfo()
        {
            List<string> type = new List<string>();

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

        //-----------Listener---------------------------------------
        private void OnAnalysisChooseListener(object sender, RoutedEventArgs args)
        {

        }

        private void OnAnalysisDisChooseListener(object sender, RoutedEventArgs args)
        {

        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            StaticValue.BtCancelListener(sender, e,this);
        }

        private void OnPreviewMouseWheelListener(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender, e);
        }
    }
}
