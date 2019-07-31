using AnalysisExtension.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace AnalysisExtension.Tool
{
    public class FileLoader
    {
        private static FileLoader instanceFileLoader = null;
        private static string[] fileList = null;
        public int FILE_NUMBER = 0;

        private FileLoader()
        {
        }

        public static FileLoader GetInstance()
        {
            if (instanceFileLoader == null)
            {
                instanceFileLoader = new FileLoader();
            }
            return instanceFileLoader;
        }

        //-----file-----
        public void SetFileList(List<FileTreeNode> list)
        {
            FILE_NUMBER = list.Count;
            fileList = new string[FILE_NUMBER];

            for (int i = 0; i < FILE_NUMBER; i++)
            {
                fileList[i] = list[i].Path;
            }

            AddFileIntoCodeBlock();
        }

        public string[] GetFileList()
        {
            return fileList;
        }

        //-----read file-----
        public void AddFileIntoCodeBlock()
        {
            AnalysisTool analysisTool = AnalysisTool.GetInstance();
            analysisTool.InitBlockList();

            for (int i = 0; i < FILE_NUMBER; i++)
            {
                /*fake data
                 * for (int j = 0; j < 10; j++)
                {
                    CodeBlock before = new CodeBlock();
                    CodeBlock after = new CodeBlock();

                    before.Content = "code before" + "\n" + "code Before" + j;
                    before.BlockId = j;
                    after.Content = "code after" + "\n" + "code After" + j;
                    after.BlockId = j % 5;

                    codeListBefore[i].Add(before);
                    codeListAfter[i].Add(after);
                }*/

                string content = File.ReadAllText(fileList[i]);

                //TODO : need to split codeBlock into before rule's block
                CodeBlock codeBlock = new CodeBlock(content);

                
                analysisTool.AddIntoBeforeList(codeBlock, i);
                analysisTool.AddIntoAfterList(codeBlock, i);
            }
        }
    }
}
