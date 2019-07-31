using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AnalysisExtension.View
{
    public partial class TransformWindowControl : UserControl
    {
        private UserControl previousControl;

        private Analysis analysisMode;
        private int fileNum;
        private int nowPageIndex = 0;

        private List<ScrollViewer> scrollViewerList = new List<ScrollViewer>();
        private double[,] scrollViewIndex = null;//[i,0] - HorizontalOffset [i,1] - VerticalOffset

        private AnalysisTool analysisTool = AnalysisTool.GetInstance();
        private FileLoader fileLoader = FileLoader.GetInstance();

        public TransformWindowControl(Analysis analysisMode,UserControl previousControl)
        {
            this.previousControl = previousControl;
            this.analysisMode = analysisMode;
            fileNum = fileLoader.FILE_NUMBER;

            Refresh();
        }

        //-----init-----
        private void InitScrollViewIndex()
        {
            scrollViewIndex = new double[scrollViewerList.Count, 2];

            for (int i = 0; i < scrollViewerList.Count; i++)
            {
                scrollViewIndex[i, 0] = 0;
                scrollViewIndex[i, 1] = 0;
            }
        }

        private void InitTabView()
        {
            while (resultTabControl.Items.Count > 0)
            {
                resultTabControl.Items.RemoveAt(0);
            }

            if (scrollViewerList.Count > 0)
            {
                scrollViewerList = new List<ScrollViewer>();
            }
        }

        //-----refersh-----
        private void Refresh()
        {
            InitializeComponent();           
            SetTabView();
            ResizeScrollViewer();
            SetScrollOffset();
        }

        //----scrollViewer----
        private void ResizeScrollViewer()
        {
            int padding = 15;
            double width = StaticValue.WINDOW.Width / 2;
            foreach (ScrollViewer viewer in scrollViewerList)
            {
                viewer.Width = width - padding;
            }
        }

        private void SetScrollOffset()
        {
            if (scrollViewIndex == null)
            {
                InitScrollViewIndex();
            }
            else
            {
                for (int i = 0; i < scrollViewerList.Count; i++)
                {
                    scrollViewerList[i].ScrollToHorizontalOffset(scrollViewIndex[i, 0]);
                    scrollViewerList[i].ScrollToVerticalOffset(scrollViewIndex[i, 1]);
                }
            }
            
        }

        private void SaveScrollOffset()
        {
            for (int i = 0; i < scrollViewerList.Count; i++)
            {
                scrollViewIndex[i, 0] = scrollViewerList[i].HorizontalOffset;
                scrollViewIndex[i, 1] = scrollViewerList[i].VerticalOffset;
            }
        }

        //-----set view----
        private void SetTabView()
        {
            InitTabView();
            string[] fileList = fileLoader.GetFileList();

            for (int i = 0; i < fileNum; i++)
            {                
                TabItem item = new TabItem();
                item.Header = StaticValue.GetNameFromPath(fileList[i]);

                SetTabControl(item, analysisTool.GetFinalBeforeBlockList(i), analysisTool.GetFinalAfterBlockList(i));

                resultTabControl.Items.Add(item);
            }
            resultTabControl.SelectedIndex = nowPageIndex;
        }

        private void SetTabControl(TabItem item, List<ICodeBlock> beforeCodeBlock, List<ICodeBlock> afterCodeBlock)
        {
            DockPanel dockPanel = new DockPanel();
            
            GroupBox topBtnGroup = new GroupBox();
            topBtnGroup.Template = (ControlTemplate)FindResource("topBtnGrop");
            DockPanel.SetDock(topBtnGroup, Dock.Top);

            ScrollViewer leftScrollViewer = SetListView(beforeCodeBlock);
            DockPanel.SetDock(leftScrollViewer, Dock.Left);

            ScrollViewer rightScrollViewer = SetListView(afterCodeBlock);
            DockPanel.SetDock(rightScrollViewer, Dock.Right);

            DockPanel bottomBtnGroup = SetButtonGroup(rightScrollViewer);
            DockPanel.SetDock(bottomBtnGroup, Dock.Bottom);

            dockPanel.Children.Add(bottomBtnGroup);
            dockPanel.Children.Add(topBtnGroup);
            dockPanel.Children.Add(leftScrollViewer);
            dockPanel.Children.Add(rightScrollViewer);

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

        private ScrollViewer SetListView(List<ICodeBlock> content)
        {
            ListView listView = new ListView();
            listView.ItemTemplate = (DataTemplate)FindResource("codeListView");
            listView.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            listView.ItemsSource = content;
            listView.SelectionChanged += CodeList_SelectionChanged;

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = listView;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheelListener;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            scrollViewerList.Add(scrollViewer);

            return scrollViewer;
        }

        //-----Listener-----
        private void CodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            nowPageIndex = resultTabControl.SelectedIndex;
            SaveScrollOffset();
            ListView listView = sender as ListView;
            int index = listView.SelectedIndex;

            if (index != -1)
            {
                CodeBlock chooseBlock = listView.SelectedItem as CodeBlock;

                SolidColorBrush changeColor = new SolidColorBrush(Colors.Blue);
                SolidColorBrush orgColor = new SolidColorBrush(Colors.White);

                for (int i = 0; i < fileNum; i++)
                {
                    foreach (CodeBlock codeBlock in analysisTool.GetFinalBeforeBlockList(i))
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
                }

                for (int i = 0; i < fileNum; i++)
                {
                    foreach (CodeBlock codeBlock in analysisTool.GetFinalAfterBlockList(i))
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            ResizeScrollViewer();
        }
    }
}
