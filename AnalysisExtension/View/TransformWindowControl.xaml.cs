using AnalysisExtension.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private UserControl previousControl;

        private Analysis analysisMode;
        private int fileNum = 0;
        
        public TransformWindowControl(Analysis analysisMode,UserControl previousControl)
        {
            this.previousControl = previousControl;
            this.analysisMode = analysisMode;
            fileNum = analysisMode.GetFileList().Count();

            Refresh();
        }

        private void Refresh()
        {
            InitializeComponent();           
            SetTabView();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Refresh();
        }

        private void SetTabView()
        {
            InitTabView();
            string[] fileList = analysisMode.GetFileList();

            for(int i = 0; i < fileList.Length; i++)
            {
                TabItem item = new TabItem();
                item.Header = StaticValue.GetNameFromPath(fileList[i]);

                SetTabControl(item, analysisMode.GetBeforeCode(i), analysisMode.GetAfterCode(i));

                resultTabControl.Items.Add(item);
                            
            }
        }

        private void InitTabView()
        {
            while (resultTabControl.Items.Count > 0)
            {
                resultTabControl.Items.RemoveAt(0);
            }
        }

        private void SetTabControl(TabItem item,List<CodeBlock> beforeCodeBlock, List<CodeBlock> afterCodeBlock)
        {
            DockPanel dockPanel = new DockPanel();
            
            GroupBox topBtnGroup = new GroupBox();
            topBtnGroup.Template = (ControlTemplate)FindResource("topBtnGrop");
            DockPanel.SetDock(topBtnGroup, Dock.Top);

            ScrollViewer beforeScrollViewer = SetListView(beforeCodeBlock);
            DockPanel.SetDock(beforeScrollViewer, Dock.Left);

            ScrollViewer afterScrollViewer = SetListView(afterCodeBlock);
            DockPanel.SetDock(afterScrollViewer, Dock.Right);

            DockPanel bottomBtnGroup = SetButtonGroup(afterScrollViewer);
            DockPanel.SetDock(bottomBtnGroup, Dock.Bottom);

            dockPanel.Children.Add(bottomBtnGroup);
            dockPanel.Children.Add(topBtnGroup);
            dockPanel.Children.Add(beforeScrollViewer);
            dockPanel.Children.Add(afterScrollViewer);

            item.Content = dockPanel;
        }

        private DockPanel SetButtonGroup(ScrollViewer rightPanel)
        {
            DockPanel dockPanel = new DockPanel();
            GroupBox changeBtnGroup = new GroupBox();
            changeBtnGroup.Template = (ControlTemplate)FindResource("centerButtonGroup");
            GroupBox cancelBtnGroup = new GroupBox();
            cancelBtnGroup.Template = (ControlTemplate)FindResource("rightButtonGroup");
            DockPanel.SetDock(cancelBtnGroup, Dock.Right);

            dockPanel.Children.Add(changeBtnGroup);
            dockPanel.Children.Add(cancelBtnGroup);

            return dockPanel;
        }

        private ScrollViewer SetListView(List<CodeBlock> content)
        {
            int padding = 15;
            double width = StaticValue.WINDOW.Width / 2 ;

            ListView listView = new ListView();
            listView.ItemTemplate = (DataTemplate)FindResource("codeListView");
            listView.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            listView.ItemsSource = content;
            listView.SelectionChanged += CodeListBefore_SelectionChanged;

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Width = width - padding;
            scrollViewer.Content = listView;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheelListener;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            
            return scrollViewer;
        }


        private void CodeListBefore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            int index = listView.SelectedIndex;

            if (index != -1)
            {
                /*CodeBlock chooseBlock = ((listView.Items[index] as ListViewItem).Content as TextBlock).DataContext as CodeBlock;
                Color changeColor = Colors.Blue;
                Color orgColor = Colors.White;
                foreach (CodeBlock codeBlock in beforeList)
                {
                    if (codeBlock.BlockId == chooseBlock.BlockId)
                    {
                        chooseBlock.BackgroundColor = changeColor;
                        codeBlock.BackgroundColor = changeColor;
                    }
                    else
                    {
                        codeBlock.BackgroundColor = orgColor;
                    }
                }

                foreach (CodeBlock codeBlock in afterList)
                {
                    if (codeBlock.BlockId == chooseBlock.BlockId)
                    {
                        chooseBlock.BackgroundColor = changeColor;
                        codeBlock.BackgroundColor = changeColor;
                    }
                    else
                    {
                        codeBlock.BackgroundColor = orgColor;
                    }
                }

                listView.UnselectAll();
                Refresh();*/
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
