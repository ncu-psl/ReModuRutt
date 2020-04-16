using AnalysisExtension.Model;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace AnalysisExtension
{
    public class StaticValue
    {
        //-----value-----
        public static int WINDOW_WIDTH = 800;
        public static int WINDOW_HEIGHT = 400;

        public static string FOLDER_TYPE = "folder";

        public static Window WINDOW = null;

        public static int CODE_BLOCK_ID_COUNT = 0;
      //  public static int PARAMETER_BLOCK_TYPE_ID_COUNT = 0;

        public static string RULE_FOLDER_PATH = @"..\..\Rule";

        //-----method-----
        public static void AddTextIntoRuleCreateFrame(string selectContent)
        {
            CreateRuleToolWindowControl control = CreateRuleToolWindowControl.GetInstance();
            if (control != null && control.IsVisible)
            {
                control.AddTextIntoRuleCreateFrame(selectContent);
            }
        }

        public static void BtCancelListener(object sender, RoutedEventArgs e, UserControl control)
        {
            CloseWindow(control);
        }

        public static void OnPreviewMouseWheelListener(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public static void CloseWindow(UserControl control)
        {
            Window.GetWindow(control).Close();
        }

        public static string GetNameFromPath(string path)
        {
            string name = null;
            string[] split = path.Split('\\');

            name = split[split.Length - 1];

            return name;
        }
        
        public static bool IsFile(string fileName)
        {
            bool result = false;
            string[] split = fileName.Split('.');

            if (split.Length < 2)
            {
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        public static int GetNextBlockId()
        {
            int idNow = CODE_BLOCK_ID_COUNT;
            CODE_BLOCK_ID_COUNT++;
            return idNow;
        }

        public static FrameworkElement DeepCopyUIElement(FrameworkElement element)
        {
            string xmlString = XamlWriter.Save(element);
            StringReader stringReader = new StringReader(xmlString);
            XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
            FrameworkElement copy = (FrameworkElement)XamlReader.Load(xmlTextReader);
            copy.DataContext = element.DataContext;
            return copy;
        }

        //----xml tool-----
        public static string GetAttributeInElement(XmlElement element, string attributeName)
        {
            if (element.HasAttribute(attributeName))
            {
                return element.GetAttribute(attributeName);
            }
            return null;
        }

        public static XmlElement FindElementByTag(XmlDocument xmlDocument,int index, string tag, string layer)
        {
            return (XmlElement)xmlDocument.DocumentElement.SelectSingleNode(layer + tag + "[" + index + "]");
        }

        public static XmlNodeList FindAllElementByTag(XmlDocument xmlDocument, int index, string tag, string layer)
        {
            return xmlDocument.DocumentElement.SelectNodes(layer + tag + "[" + index + "]");
        }

        public static string GetXmlTextByTag(XmlDocument xmlDocument, string tag)
        {
            return FindElementByTag(xmlDocument, 1, tag, "").InnerXml;
        }
               
        public static bool IsListSame(List<ICodeBlock> org, List<ICodeBlock> compare,bool isWhitespaceIgnore)
        {
            int orgCount = 0,compareCount = 0;
            bool result = false;

            while (orgCount < org.Count && compareCount < compare.Count)
            {
                if (Regex.Match(org[orgCount].Content, @"[\W]").Length == org[orgCount].Content.Length)
                {
                    orgCount++;
                    continue;
                }
                else if(Regex.Match(compare[compareCount].Content, @"[\W]").Length == compare[compareCount].Content.Length)
                {
                    compareCount++;
                    continue;
                }

                if (isWhitespaceIgnore)
                {
                    org[orgCount].Content = Regex.Replace(org[orgCount].Content, "[\n\r\t ]","");
                    compare[compareCount].Content = Regex.Replace(compare[compareCount].Content, "[\n\r\t ]", "");
                }

                if (org[orgCount].Content.Equals(compare[compareCount].Content))
                {
                    result = true;
                }
                else
                {
                    return false;
                }
                orgCount++;
                compareCount++;
            }
            return result;
        }

        public static bool IsListSame(List<ICodeBlock> org, List<ICodeBlock> compare)
        {
            return IsListSame(org, compare, false);
        }

        public static string GetAllContent(List<ICodeBlock> codeBlockList)
        {
            string result = "";
            foreach (ICodeBlock codeBlock in codeBlockList)
            {
                result += codeBlock.Content;
            }
            return result;
        }

        public static List<ICodeBlock> CopyList(List<ICodeBlock> orgList)
        {
            List<ICodeBlock> result = new List<ICodeBlock>();

            foreach (ICodeBlock codeBlock in orgList)
            {
                result.Add(codeBlock.GetCopy());
            }
            return result;
        }
    }

}
