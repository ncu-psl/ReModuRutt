using AnalysisExtension.Model;
using System.Collections.Generic;
using System.IO;

namespace AnalysisExtension.Tool
{
    public class FileLoader
    {
        private static FileLoader instanceFileLoader = null;
        private static string[] fileList = null;
        public int FILE_NUMBER = 0;

        private static string[] fileContent = null;

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
            fileContent = new string[FILE_NUMBER];
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
                fileContent[i] = GetFileText(fileList[i]);
            }
        }

        public List<RuleBlock> LoadRuleList(string ruleFolderPath)
        {
            string[] rulePath = GetRuleListByPath(ruleFolderPath);
            List<RuleBlock> ruleList = new List<RuleBlock>();

            for (int i = 0; i < rulePath.Length; i++)
            {
                RuleBlock ruleBlock = LoadSingleRuleByPath(rulePath[i]);
                ruleList.Add(ruleBlock);
            }

            return ruleList;
        }

        public RuleBlock LoadSingleRuleByPath(string rulePath)
        {
            string ruleText = GetFileText(rulePath);
            RuleBlock ruleBlock = new RuleBlock(ruleText);
            return ruleBlock;
        }

        public string[] GetRuleListByPath(string ruleFolderPath)
        {
            return Directory.GetFiles(ruleFolderPath, "*.xml");
        }

        public string[] GetAllFileName(string ruleFolderPath)
        {
            return Directory.GetDirectories(ruleFolderPath);
        }

        public string GetFileText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        //-----get content -----
        public string[] GetFileContent()
        {
            return fileContent;
        }
    }
}
