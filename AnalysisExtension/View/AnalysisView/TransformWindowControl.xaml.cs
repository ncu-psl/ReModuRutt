using AnalysisExtension.Model;
using AnalysisExtension.Tool;
using AnalysisExtension.View.AnalysisView;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AnalysisExtension.View
{
    public partial class TransformWindowControl : UserControl
    {
        private UserControl previousControl;

        private int fileNum;
        private int nowPageIndex = 0;

        private RichTextBox[][] richTextBoxList;
        private double[,] scrollViewIndex = null;//[i,0] - HorizontalOffset [i,1] - VerticalOffset

        private AnalysisManager analysisTool = AnalysisManager.GetInstance();
        private FileLoader fileLoader = FileLoader.GetInstance();

        private List<ICodeBlock>[] beforeList;
        private List<ICodeBlock>[] afterList;

        private SolidColorBrush changeColor = new SolidColorBrush(Colors.LightSkyBlue);
        private SolidColorBrush orgColor = new SolidColorBrush(Colors.White);

        private bool isRichTextBockSelect = false;
        private int chooseBlockId = -1;

        public TransformWindowControl(UserControl previousControl)
        {
            this.previousControl = previousControl;
            fileNum = fileLoader.FILE_NUMBER;
            richTextBoxList = new RichTextBox[fileNum][];
            for (int i = 0; i < fileNum; i++)
            {
                richTextBoxList[i] = new RichTextBox[2];
            }
            GetListFromAnalysisTool();
            Refresh();
        }

        //-----init-----
        private void GetListFromAnalysisTool()
        {
            beforeList = analysisTool.GetFinalBeforeBlockList();
            afterList = analysisTool.GetFinalAfterBlockList();
        }

        private void InitRichTextBoxIndex()
        {
            scrollViewIndex = new double[fileNum * 2, 2];

            for (int i = 0; i < fileNum * 2; i++)
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

            if (richTextBoxList.Length > 0)
            {
                richTextBoxList = new RichTextBox[fileNum][];
                for (int i = 0; i < fileNum; i++)
                {
                    richTextBoxList[i] = new RichTextBox[2];
                }
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

            for (int i = 0; i < fileNum; i++)
            {
                for (int j = 0; j < richTextBoxList[i].Length; j++)
                {
                    richTextBoxList[i][j].Width = width - padding;
                    richTextBoxList[i][j].Document.PageWidth = width * 2;
                }
            }
        }

        private void SetScrollOffset()
        {
            if (scrollViewIndex == null)
            {
                InitRichTextBoxIndex();
            }
            else
            {
                for (int i = 0; i < fileNum; i++)
                {
                    for (int j = 0; j < richTextBoxList[i].Length; j++)
                    {
                        richTextBoxList[i][j].ScrollToHorizontalOffset(scrollViewIndex[i, 0]);
                        richTextBoxList[i][j].ScrollToVerticalOffset(scrollViewIndex[i, 1]);
                    }
                }
            }
        }

        private void SaveScrollOffset()
        {
            for (int i = 0; i < fileNum; i++)
            {
                for (int j = 0; j < richTextBoxList[i].Length; j++)
                {
                    scrollViewIndex[i, 0] = richTextBoxList[i][j].HorizontalOffset;
                    scrollViewIndex[i, 1] = richTextBoxList[i][j].VerticalOffset;
                }
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

                SetTabControl(i, item, beforeList[i], afterList[i]);

                resultTabControl.Items.Add(item);
            }
            resultTabControl.SelectedIndex = nowPageIndex;
        }

        private void SetTabControl(int fileIndex, TabItem item, List<ICodeBlock> beforeCodeBlock, List<ICodeBlock> afterCodeBlock)
        {
            DockPanel dockPanel = new DockPanel();

            RichTextBox beforeRichTextBox = SetListView(beforeCodeBlock);
            richTextBoxList[fileIndex][0] = beforeRichTextBox;
            DockPanel.SetDock(beforeRichTextBox, Dock.Left);

            RichTextBox afterRichTextBox = SetListView(afterCodeBlock);
            richTextBoxList[fileIndex][1] = afterRichTextBox;
            DockPanel.SetDock(afterRichTextBox, Dock.Right);

            DockPanel bottomBtnGroup = SetButtonGroup();
            DockPanel.SetDock(bottomBtnGroup, Dock.Bottom);

            dockPanel.Children.Add(bottomBtnGroup);
            dockPanel.Children.Add(beforeRichTextBox);
            dockPanel.Children.Add(afterRichTextBox);

            item.Content = dockPanel;
        }

        private DockPanel SetButtonGroup()
        {
            DockPanel dockPanel = new DockPanel();
            GroupBox cancelBtnGroup = new GroupBox();
            cancelBtnGroup.Template = (ControlTemplate)FindResource("rightButtonGroup");
            DockPanel.SetDock(cancelBtnGroup, Dock.Right);
            GroupBox centerButtonGroup = new GroupBox();
            centerButtonGroup.Template = (ControlTemplate)FindResource("centerButtonGroup");
            DockPanel.SetDock(centerButtonGroup, Dock.Left);

            dockPanel.Children.Add(cancelBtnGroup);
            dockPanel.Children.Add(centerButtonGroup);

            return dockPanel;
        }

        private RichTextBox SetListView(List<ICodeBlock> content)
        {
            RichTextBox resultTextBox = new RichTextBox();
            resultTextBox.AcceptsReturn = true;
            resultTextBox.AcceptsTab = true;
            resultTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            resultTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            resultTextBox.Document.Blocks.AddRange(SetCodeBlockList(content));
            resultTextBox.SelectionChanged += OnResultTextBoxSelectionChangedListener;

            return resultTextBox;

        }

        private List<Paragraph> SetCodeBlockList(List<ICodeBlock> codeBlockList)
        {
            List<Paragraph> result = new List<Paragraph>();

            Paragraph paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0, 0, 0, 0);

            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                if (codeBlock.Content.EndsWith("\n"))
                {
                    result.Add(paragraph);
                    paragraph = new Paragraph();
                    paragraph.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    TextBlock inner = new TextBlock() { Text = codeBlock.Content, Background = orgColor, DataContext = codeBlock };
                    InlineUIContainer container = new InlineUIContainer(inner, paragraph.ContentEnd);
                    if (codeBlockList.IndexOf(codeBlock) == codeBlockList.Count - 1)
                    {
                        result.Add(paragraph);
                        paragraph = new Paragraph();
                        paragraph.Margin = new Thickness(0, 0, 0, 0);
                    }
                }
            }
            return result;
        }

        private int FindNextBlockId(int idNow)
        {
            int nextId = idNow;
            int dis = 99999;

            for (int i = 0; i < fileNum; i++)
            {
                for (int j = 0; j < richTextBoxList[i].Length; j++)
                {
                    RichTextBox richTextBox = richTextBoxList[i][j];
                    foreach (Block block in richTextBox.Document.Blocks)
                    {
                        foreach (Inline inline in (block as Paragraph).Inlines)
                        {
                            TextBlock textBlock = (inline as InlineUIContainer).Child as TextBlock;
                            ICodeBlock codeBlock = textBlock.DataContext as ICodeBlock;
                            if (textBlock.Text.Length > 0 && codeBlock.BlockId - idNow > 0)
                            {
                                if (codeBlock.BlockId - idNow < dis)
                                {
                                    dis = codeBlock.BlockId - nextId;
                                    nextId = codeBlock.BlockId;
                                }
                            }
                        }
                    }
                }
            }

            if (nextId == idNow)
            {
                nextId = FindNextBlockId(0);
            }

            return nextId;
        }

        private void MarkSameIdBlock(int id)
        {
            chooseBlockId = id;
            for (int i = 0; i < fileNum; i++)
            {
                for (int j = 0; j < richTextBoxList[i].Length; j++)
                {
                    RichTextBox richTextBox = richTextBoxList[i][j];
                    foreach (Block block in richTextBox.Document.Blocks)
                    {
                        foreach (Inline inline in (block as Paragraph).Inlines)
                        {
                            TextBlock textBlock = (inline as InlineUIContainer).Child as TextBlock;
                            ICodeBlock codeBlock = textBlock.DataContext as ICodeBlock;
                            if (textBlock.Text.Length > 0)
                            {
                                if (codeBlock.BlockId == id)
                                {
                                    textBlock.Background = changeColor;
                                    if (i != resultTabControl.SelectedIndex)
                                    {
                                        resultTabControl.SelectedIndex = i;
                                    }
                                    textBlock.BringIntoView();

                                }
                                else
                                {
                                    textBlock.Background = orgColor;
                                }
                            }
                        }
                    }
                }
            }
        }

        private ICodeBlock GetBlockById(int id)
        {
            ICodeBlock findBlock = null;
            for (int i = 0; i < fileNum; i++)
            {
                for (int j = 0; j < richTextBoxList[i].Length; j++)
                {
                    RichTextBox richTextBox = richTextBoxList[i][j];
                    foreach (Block block in richTextBox.Document.Blocks)
                    {
                        foreach (Inline inline in (block as Paragraph).Inlines)
                        {
                            TextBlock textBlock = (inline as InlineUIContainer).Child as TextBlock;
                            ICodeBlock codeBlock = textBlock.DataContext as ICodeBlock;
                            if (textBlock.Text.Length > 0)
                            {
                                if (codeBlock.BlockId == id)
                                {
                                    findBlock = codeBlock;
                                    return codeBlock;
                                }
                            }
                        }
                    }
                }
            }
            return findBlock;
        }
        //-----Listener-----
        private void OnResultTextBoxSelectionChangedListener(object sender, RoutedEventArgs e)
        {
            if (isRichTextBockSelect)
            {
                return;
            }

            RichTextBox textBox = sender as RichTextBox;
            TextSelection selection = textBox.Selection;
            isRichTextBockSelect = true;


            Paragraph paragraph = selection.End.Paragraph;
            TextBlock chooseTextBlock = null;
            int id = -1;

            foreach (InlineUIContainer inline in paragraph.Inlines)
            {
                if (inline.ContentEnd.GetOffsetToPosition(selection.End) == 1)
                {
                    chooseTextBlock = inline.Child as TextBlock;
                    id = (chooseTextBlock.DataContext as ICodeBlock).BlockId;
                    break;
                }
            }

            if (chooseTextBlock != null)
            {
                MarkSameIdBlock(id);                
                //unselect text
                textBox.Selection.Select(textBox.Selection.End, textBox.Selection.End);
            }
            isRichTextBockSelect = false;
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

        //-----save file-----
        private string SaveFile(int indexNow)
        {
            string afterString = "";

            if (richTextBoxList[indexNow][1].Document.Blocks.Count > 1)
            {
                foreach (Block block in richTextBoxList[indexNow][1].Document.Blocks)
                {
                    foreach (Inline inline in (block as Paragraph).Inlines)
                    {
                        if (inline is InlineUIContainer)
                        {
                            TextBlock textBlock = (inline as InlineUIContainer).Child as TextBlock;
                            ICodeBlock codeBlock = textBlock.DataContext as ICodeBlock;
                            afterString += codeBlock.Content;
                        }
                        else
                        {
                            afterString += (inline as Run).Text;
                            
                        }

                        if (inline.NextInline == null)
                        {
                            afterString += "\n";
                        }
                    }
                }
            }
            return afterString;
        }

        private void OnClickSaveAsListener(object sender, RoutedEventArgs e)
        {
            int indexNow = resultTabControl.SelectedIndex;
            string afterString = SaveFile(indexNow);

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
            int indexNow = resultTabControl.SelectedIndex;
            string afterString = SaveFile(indexNow);

            MessageBoxResult result = MessageBox.Show("sure to overwrite the file now choose?","Save File",MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                string path = fileLoader.GetFileList()[indexNow];
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
                    string afterString = SaveFile(i);
                    string path = fileLoader.GetFileList()[i];
                    File.WriteAllText(path, afterString);
                }
                
                MessageBox.Show("all file save");
            }
        }

        //-----edit btn------
        private void OnClickEditChooseBlockListener(object sender, RoutedEventArgs e)
        {
            if (chooseBlockId > -1)
            {
                EditTransformResultWindowControl content = new EditTransformResultWindowControl(beforeList[nowPageIndex], GetBlockById(chooseBlockId), nowPageIndex);

                Window editWindow = new Window
                {
                    Title = "edit match rule",
                    Content = content,
                    Width = StaticValue.WINDOW_WIDTH,
                    Height = StaticValue.WINDOW_HEIGHT
                };
                editWindow.ShowDialog();
                if (content.isAnalysisSuccess)
                {
                    GetListFromAnalysisTool();
                    Refresh();
                }
            }
        }

        private void OnClickShowNextBlockListener(object sender, RoutedEventArgs e)
        {
            int nextId = 0;
            if (chooseBlockId != -1)
            {
                nextId = FindNextBlockId(chooseBlockId);
            }
            else
            {
                nextId = FindNextBlockId(nextId);
            }

            MarkSameIdBlock(nextId);
        }        
    }
}
