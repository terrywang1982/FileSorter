using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Common;

namespace FileSorter
{
    class Program
    {
       static Common.Log.LoggerWrapper mLog = Common.Log.LoggerWrapper.GetInstance();
        static void Main(string[] args)
        {
            try
            { 

                DateTime start = DateTime.UtcNow;
                DataTable dt = DataProvider.TMProvider.GetUnSortTMX();

                int finishCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    DateTime startTime = DateTime.UtcNow;
                    Lib.SortTMX sort = new Lib.SortTMX();
                    string catelog = sort.GetSortedResult(dr["TMXFileName"].ToString(), Int32.Parse(dr["ID"].ToString()));
                    DateTime endTime = DateTime.UtcNow;

                    finishCount++;
                    string tmxText = string.Format("Current running in {0}/{1},  TMX :{2}  Catelog: {3} Total Time:{4} seconds.", finishCount.ToString(), dt.Rows.Count, dr["TMXFileName"].ToString(), catelog, (endTime - startTime).TotalMilliseconds.ToString());
                    mLog.Log(Common.Log.LogSeverity.Info, tmxText);
                    Console.WriteLine(tmxText);
                }

                DateTime end = DateTime.UtcNow;
                string logText = string.Format(" Sort Finished! TMX total count: {0} Sort total count:{1} Total Time is: {2} seconds." + Environment.NewLine, dt.Rows.Count, dt.Rows.Count, (end - start).TotalSeconds.ToString());
                mLog.Log(Common.Log.LogSeverity.Info, logText);
                Console.WriteLine(logText);
            }
            catch(Exception ex)
                {
                     mLog.Log(Common.Log.LogSeverity.Error, ex.ToString());
                }

        }

        private bool AnalyseTMX(string tmxPath)
        {
            bool isSucc = false;

            StreamReader sr = null;
            DateTime startTime = DateTime.UtcNow;
            string sourceConext = string.Empty;
            if (tmxPath.ToLower().EndsWith(".tmx"))
            {
                sr = new StreamReader(tmxPath, Encoding.UTF8);
                while (sr.ReadLine() != null)
                {
                    string context = sr.ReadToEnd();
                }
                //获得源文件内容
                sourceConext = Lib.AnalyseWord.GetFileContent(tmxPath);
            }
            //获取各个行业名称及关键字
            string[] allClassifyFilename = null;
            string[] allClassifyContext = GetAllCatelogFileAndContext(ref allClassifyFilename);
            //统计此源文件与各个行业关键字匹配情况E:\work\文件自动分类\Aticle\很值得读的短篇医药健康短文.doc
            Dictionary<string, int> results = StatClassifyKeyCount(sourceConext, allClassifyFilename, allClassifyContext);


            //格式化输出
            int maxCount = 0;
            string classResult = string.Empty;
            string result = string.Empty;
            string totalTime = string.Empty;

            foreach (KeyValuePair<string, int> kv in results)
            {
                string cateName = kv.Key.Substring(kv.Key.LastIndexOf("\\") + 1);
                cateName = cateName.Substring(0, cateName.LastIndexOf('.'));
                result += "Catelog:" + cateName + ";  Keyword count:" + kv.Value + Environment.NewLine;
                if (kv.Value > maxCount)
                {
                    maxCount = kv.Value;
                    classResult = cateName;
                }
            }
            DateTime endTime = DateTime.UtcNow;
            TimeSpan ts = endTime - startTime;
            totalTime = ts.TotalSeconds.ToString();
            return isSucc;
        }
        private void Analyse_Click(string path)
        {
            
            StreamReader sr = null;
            DateTime startTime = DateTime.UtcNow;
            string sourceConext = string.Empty;
            if (path.ToLower().EndsWith(".tmx"))
            {
                sr = new StreamReader(path, Encoding.UTF8);
                while (sr.ReadLine() != null)
                {
                    string context = sr.ReadToEnd();
                }
                //获得源文件内容
                sourceConext = Lib.AnalyseWord.GetFileContent(path);
            }
            //获取各个行业名称及关键字
            string[] allClassifyFilename = null;
            string[] allClassifyContext = GetAllCatelogFileAndContext(ref allClassifyFilename);
            //统计此源文件与各个行业关键字匹配情况E:\work\文件自动分类\Aticle\很值得读的短篇医药健康短文.doc
            Dictionary<string, int> results = StatClassifyKeyCount(sourceConext, allClassifyFilename, allClassifyContext);


            //格式化输出
            int maxCount = 0;
            string classResult = string.Empty;
            string result=string.Empty;
            string totalTime = string.Empty;
            
            foreach (KeyValuePair<string, int> kv in results)
            {
                string cateName = kv.Key.Substring(kv.Key.LastIndexOf("\\") + 1);
                cateName = cateName.Substring(0, cateName.LastIndexOf('.'));
                result += "Catelog:" + cateName + ";  Keyword count:" + kv.Value + Environment.NewLine;
                if (kv.Value > maxCount)
                {
                    maxCount = kv.Value;
                    classResult = cateName;
                }
            }
            DateTime endTime = DateTime.UtcNow;
            TimeSpan ts = endTime - startTime;
            totalTime = ts.TotalSeconds.ToString();
             
        }

        //获取各个行业的关键字
        private string[] GetAllCatelogFileAndContext(ref string[] catelogFileName)
        {
            string[] allCatelogContext = null;
            string keywordPath = System.Configuration.ConfigurationManager.AppSettings["KeywordPath"];
            DirectoryInfo diPath = new DirectoryInfo(keywordPath);
            //get all directory
            DirectoryInfo[] dirInfo = diPath.GetDirectories();
            //stored class files.
            List<string> fileList = new List<string>();
            //get parent directory files
            FileInfo[] files = diPath.GetFiles();
            foreach (FileInfo fi in files)
            {
                fileList.Add(fi.FullName);
            }

            // 遍历文件夹

            foreach (DirectoryInfo dir in dirInfo)
            {
                FileInfo[] fileinfos = dir.GetFiles();
                foreach (FileInfo fi in fileinfos)
                {
                    fileList.Add(fi.FullName);
                }
            }
            allCatelogContext = new string[fileList.Count];
            for (int i = 0; i < fileList.Count; i++)
            {
                //读取word
                allCatelogContext[i] = Lib.AnalyseWord.GetFileContent(fileList[i]);// File.ReadAllText(fileList[i], Encoding.GetEncoding("gb2312"));

                ////读取txt
                //allCatelogContext[i] = File.ReadAllText(fileList[i], Encoding.GetEncoding("gb2312"));

            }
            catelogFileName = fileList.ToArray();
            return allCatelogContext;
        }

        public Dictionary<string, int> StatClassifyKeyCount(string sourceFileContext, string[] classifyFileName, string[] classifyContext)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < classifyContext.Length; i++)
            {
                string context = classifyContext[i];
                string[] arrKey = context.Split('、');
                int keycount = 0;
                foreach (string key in arrKey)
                {
                    if (sourceFileContext.Contains(key))
                    {
                        keycount++;
                    }
                }
                dictionary.Add(classifyFileName[i], keycount);
            }
            return dictionary;
        }
    }
}
