using AnalysisExtension.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisExtension.Tool
{
    public class FileLoader
    {
        private static FileLoader instanceFileLoader = null;
        private static string[] fileList = null;
        private static List<ICodeBlock> ruleList = null;
        public int FILE_NUMBER = 0;

        private FileLoader()
        {
            ruleList = new List<ICodeBlock>();
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
        }

        public string[] GetFileList()
        {
            return fileList;
        }

        //-----rule-----
        public void SetRuleList()
        {
            //TODO : set rule List
        }

        public List<ICodeBlock> GetRuleList()
        {
            return ruleList;
        }
    }
}
