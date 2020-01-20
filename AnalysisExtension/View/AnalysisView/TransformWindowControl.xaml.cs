using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text;

namespace AnalysisExtension.View
{
    public partial class TransformWindowControl : UserControl
    {
        private UserControl previousControl;

        private int fileNum;
        private int nowPageIndex = 0;

        private List<ScrollViewer> scrollViewerList = new List<ScrollViewer>();
        private double[,] scrollViewIndex = null;//[i,0] - HorizontalOffset [i,1] - VerticalOffset

        private AnalysisTool analysisTool = AnalysisTool.GetInstance();
        private FileLoader fileLoader = FileLoader.GetInstance();

        private List<ICodeBlock>[] beforeList;
        private List<ICodeBlock>[] afterList;
        
        public TransformWindowControl(UserControl previousControl)
        {        
            this.previousControl = previousControl;
            fileNum = fileLoader.FILE_NUMBER;
            GetListFromAnalysisTool();
            Refresh();
        }

        //-----init-----
        private void GetListFromAnalysisTool()
        {            
            beforeList = analysisTool.GetFinalBeforeBlockList();
            afterList = analysisTool.GetFinalAfterBlockList();
        }

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

                SetTabControl(item, beforeList[i], afterList[i]);

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
            StackPanel outerPanel = new StackPanel();
            outerPanel.Orientation = Orientation.Vertical;

            List<ICodeBlock> codeBlockInLine = new List<ICodeBlock>();
            int blockIdLast = -1;
            bool isChange = false;

            foreach (ICodeBlock codeBlock in content)
            {
                /*if (content.IndexOf(codeBlock) != 0)
                {
                    if (codeBlock.BlockId != blockIdLast)
                    {
                        isChange = true;
                    }
                }
                blockIdLast = codeBlock.BlockId;

                if (isChange)
                {
                    codeBlockInLine.Add(codeBlock);
                    outerPanel.Children.Add(SetInnerStackPanel(codeBlockInLine));
                    codeBlockInLine = new List<ICodeBlock>();
                }
                else */if (Regex.IsMatch(codeBlock.Content, @"[\n\r]+"))
                {
                    outerPanel.Children.Add(SetInnerStackPanel(codeBlockInLine));
                    codeBlockInLine = new List<ICodeBlock>();
                }
                else if (content.IndexOf(codeBlock) == content.Count - 1)//is last
                {
                    codeBlockInLine.Add(codeBlock);
                    outerPanel.Children.Add(SetInnerStackPanel(codeBlockInLine));
                }
                else
                {
                    codeBlockInLine.Add(codeBlock);
                }
            }

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = outerPanel;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheelListener;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            scrollViewerList.Add(scrollViewer);
            return scrollViewer;

        }

        private StackPanel SetInnerStackPanel(List<ICodeBlock> codeBlockInLine)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            foreach (ICodeBlock codeBlock in codeBlockInLine)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.DataContext = codeBlock;
                textBlock.Text = codeBlock.Content;
                textBlock.Background = codeBlock.BackgroundColor;
                textBlock.MouseDown += CodeList_SelectionChanged;
                stackPanel.Children.Add(textBlock);
            }

            return stackPanel;
        }
        //-----Listener-----
        private void CodeList_SelectionChanged(object sender, MouseButtonEventArgs e)
        {
            nowPageIndex = resultTabControl.SelectedIndex;
            SaveScrollOffset();
            TextBlock textBlock = sender as TextBlock;

            ICodeBlock chooseBlock = textBlock.DataContext as ICodeBlock;

            SolidColorBrush changeColor = new SolidColorBrush(Colors.LightSkyBlue);
            SolidColorBrush orgColor = new SolidColorBrush(Colors.White);

            for (int i = 0; i < fileNum; i++)
            {
                foreach (ICodeBlock codeBlock in beforeList[i])
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
                foreach (ICodeBlock codeBlock in afterList[i])
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
            Refresh();            
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

        private void OnClickSaveAsListener(object sender, RoutedEventArgs e)
        {
            string afterString = "";

            foreach (ICodeBlock codeBlock in afterList[nowPageIndex])
            {
                afterString += codeBlock.Content;
            }

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "(*.*)|*.*";
            saveFileDialog.Title = "save as";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                UnicodeEncoding unicode = new UnicodeEncoding();
                
                FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
                fileStream.Write(unicode.GetBytes(afterString), 0, unicode.GetByteCount(afterString));
                fileStream.Close();
                MessageBox.Show("file save");
            }
            
        }

        private void OnClickSaveListener(object sender, RoutedEventArgs e)
        {
            string afterString = "";

            foreach (ICodeBlock codeBlock in afterList[nowPageIndex])
            {
                afterString += codeBlock.Content;
            }

            MessageBoxResult result = MessageBox.Show("sure to overwrite the file now choose?","Save File",MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                string path = fileLoader.GetFileList()[nowPageIndex];
                File.WriteAllText(path, afterString);
                MessageBox.Show("file save");
            }
        }

        private void OnClickSaveAllListener(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("sure to overwrite all file?", "Save File", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                for (int i = 0; i < fileLoader.FILE_NUMBER; i++)
                {
                    string afterString = "";
                    foreach (ICodeBlock codeBlock in afterList[i])
                    {
                        afterString += codeBlock.Content;
                    }
                    string path = fileLoader.GetFileList()[i];
                    File.WriteAllText(path, afterString);
                }
                
                MessageBox.Show("all file save");
            }
        }
    }
}
