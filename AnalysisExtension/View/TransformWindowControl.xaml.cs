using AnalysisExtension.Model;
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


namespace AnalysisExtension.View
{

    public partial class TransformWindowControl : UserControl
    {
        private List<CodeBlock> beforeList = null;
        private List<CodeBlock> afterList = null;
        private double beforeListWidth = 0;
        private double afterListWidth = 0;

        public TransformWindowControl(List<CodeBlock> codeBefore, List<CodeBlock> codeAfter)
        {
            beforeList = codeBefore;
            afterList = codeAfter;

            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();

            beforeListWidth = codeListBefore.Width;
            afterListWidth = codeListAfter.Width;

            ResetListViewItem();
            
         //   SetBackgroundColor();
        }

        private void ResetListViewItem()
        {
            //if org list is not null,remove all
            if (codeListBefore.Items.Count > 0)
            {
                while(codeListBefore.Items.Count > 0)
                {
                    codeListBefore.Items.RemoveAt(0);
                }
            }

            if (codeListAfter.Items.Count > 0)
            {
                while (codeListAfter.Items.Count > 0)
                {
                    codeListAfter.Items.RemoveAt(0);
                }
            }

            //add item into each list
            foreach (CodeBlock codeBlock in beforeList)
            {
                TextBlock item = new TextBlock();
                item.DataContext = codeBlock;
                item.Text = codeBlock.Content;
                item.Width = beforeListWidth;

                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Content = item;
                listViewItem.Background = new SolidColorBrush(codeBlock.BackgroundColor);
                codeListBefore.Items.Add(listViewItem);
            }

            foreach (CodeBlock codeBlock in afterList)
            {
                TextBlock item = new TextBlock();
                item.DataContext = codeBlock;
                item.Text = codeBlock.Content;
                item.Width = afterListWidth;

                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Content = item;
                listViewItem.Background = new SolidColorBrush(codeBlock.BackgroundColor);
                codeListAfter.Items.Add(listViewItem);
            }

        }

        private void CodeListBefore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            int index = listView.SelectedIndex;

            if (index != -1)
            {
                CodeBlock chooseBlock = ((listView.Items[index] as ListViewItem).Content as TextBlock).DataContext as CodeBlock;

                foreach (CodeBlock codeBlock in beforeList)
                {
                    if (codeBlock.BlockId == chooseBlock.BlockId)
                    {
                        chooseBlock.BackgroundColor = Colors.AliceBlue;
                        codeBlock.BackgroundColor = Colors.AliceBlue;
                    }
                    else
                    {
                        codeBlock.BackgroundColor = Colors.White;
                    }
                }

                foreach (CodeBlock codeBlock in afterList)
                {
                    if (codeBlock.BlockId == chooseBlock.BlockId)
                    {
                        chooseBlock.BackgroundColor = Colors.AliceBlue;
                        codeBlock.BackgroundColor = Colors.AliceBlue;
                    }
                    else
                    {
                        codeBlock.BackgroundColor = Colors.White;
                    }
                }

                listView.UnselectAll();
                Refresh();
            }
        }

        private void OnPreviewMouseWheelListener(object sender, MouseWheelEventArgs e)
        {
            StaticValue.OnPreviewMouseWheelListener(sender, e);
        }

        private void OnClickBtCancelListener(object sender, RoutedEventArgs e)
        {
            Refresh();
            StaticValue.CloseWindow(this);
        }
    }
}
