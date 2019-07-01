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
        private static List<RuleBlock> ruleList = null;
        public int FILE_NUMBER = 0;

        private FileLoader()
        {
            ruleList = new List<RuleBlock>();
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
        public void SetRuleList(Analysis analysis)
        {
            //TODO : set rule List
            string[] rulePath = Directory.GetFiles(analysis.RuleFolderPath,"*.xml");

            for (int i = 0; i < rulePath.Length; i++)
            {
                string ruleText = File.ReadAllText(rulePath[i]);
                RuleBlock ruleBlock = new RuleBlock(ruleText);
                ruleList.Add(ruleBlock);
            }
            
        }

        public List<RuleBlock> GetRuleList()
        {
            return ruleList;
        }

    }
}
