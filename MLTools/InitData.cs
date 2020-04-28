using MLTools;
using SufeiUtil;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Untils;

namespace GetTraceData
{
    public class InitData
    {
        public const string url="http://data.gtimg.cn/flashdata/hushen/daily/{0}/{1}.js";

        const string DataUrl = "http://quotes.money.163.com/service/chddata.html?code={0}&start={1}&end={2}&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP";
        /// <summary>
        /// 以前3天数据做为特征
        /// </summary>
        const int dayCount = 3;

        public static string DownLoadFile(string startTime,string endTime,string gpCode,bool isSH)
        {
            try
            {
                string code = isSH ? "0" + gpCode : "1" + gpCode;
                string getUrl = string.Format(DataUrl, code, startTime, endTime);
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = getUrl,//URL这里都是测试     必需项
                    Method = "get",//URL     可选项 默认为Get
                    Allowautoredirect = true,//是否根据301跳转     可选项   
                    UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",////用户的浏览器类型，版本，操作系统     可选项有默认值  
                    ContentType = "application/octet-stream",
                    ResultType = ResultType.Byte,
                    Encoding= System.Text.Encoding.GetEncoding("GB2312")
            };
                item.Header.Add("Accept-Language", "zh-CN");
                item.Header.Add("Accept-Encoding", "gzip, deflate");
                //得到HTML代码
                HttpResult result = http.GetHtml(item);
                byte[] data = result.ResultByte;
                if (data.Length < 180)
                    throw new Exception("下载错误!");
                string str = System.Text.Encoding.GetEncoding("GB2312").GetString(data);
                str=str.Replace("日期", "riqi").Replace("股票代码", "gpdm").Replace("名称", "mc").Replace("收盘价", "spj")
                    .Replace("最高价", "zgj").Replace("最低价", "zdj").Replace("开盘价", "kpj").Replace("前收盘", "qsp")
                    .Replace("涨跌额", "zde").Replace("涨跌幅", "zdf").Replace("换手率", "hsl").Replace("成交量", "cjl")
                    .Replace("成交金额", "cjje").Replace("总市值", "zsz").Replace("流通市值", "ltsz");
                if (data.Length == 108)
                    throw new Exception("股票代码错误!");
                string fileName = $"{gpCode}.csv";
                string filePath = FileHelper.MapPath(fileName);
                WriteToFile(filePath, str);
                return filePath;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Http下载文件
        /// </summary>
        public static string HttpDownloadFile(string url, string path)
        {
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();

            //创建本地文件写入流
            Stream stream = new FileStream(path, FileMode.Create);

            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return path;
        }

        public static int GetDataCount(string filePath)
        {
            DataTable dt = new DataTable();
            dt = CSVHelper.csv2dt(filePath, 0, dt);
            return dt.Rows.Count;
        }

        public static DataTable GetTestSource(string testPath)
        {
            DataTable dt = new DataTable();
            dt = CSVHelper.csv2dt(testPath, 0, dt);
            return dt;
        }

        public static string CreateTrainData(string filePath,int count=1)
        {
            
            try
            {
                DataTable dt = new DataTable();
                dt = CSVHelper.csv2dt(filePath,0,dt);
                dt.Columns.Remove("gpdm");
                dt.Columns.Remove("mc");
                dt.Columns.Add("mspj");
                for (int i = 1; i < dt.Rows.Count-1; i++)
                {
                    dt.Rows[i]["mspj"] = dt.Rows[i - 1]["spj"];
                }
                for (int i = 0; i < count; i++)
                {
                    dt.Rows.RemoveAt(0);
                }
                string fullPath = FileHelper.MapPath("trainData.csv");
                
                CSVHelper.SaveCSV(dt, fullPath);
                return fullPath;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static string CreateTrainData(string filePath,string trainFileName,int days, int removeCount,ref string predictFilePath)
        {
            try
            {
                DataTable dt = GetDataTableByFile(filePath);
                dt.Columns.Remove("gpdm");
                dt.Columns.Remove("mc");
                string[] selectColNames = new string[dt.Columns.Count+1];

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    selectColNames[i] = dt.Columns[i].ColumnName;
                }
                selectColNames[dt.Columns.Count] = "mspj";
                DataTable trainDT = AddColAndData(dt, selectColNames, days);

                DataTable predictDT = trainDT.Clone();

                predictDT.Rows.Add(trainDT.Rows[0].ItemArray);
                predictFilePath=wirteDataTableToFile(predictDT, "predict.csv");

                trainDT.Rows.RemoveAt(removeCount);

                return wirteDataTableToFile(trainDT, trainFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CreatePredictData(DataRow row,string predictFileName)
        {
            DataTable dt = new DataTable();
            dt.ImportRow(row);
            return wirteDataTableToFile(dt, predictFileName);

        }

        public static DataTable GetDataTableByFile(string filePath)
        {
            DataTable dt = new DataTable();
            dt = CSVHelper.csv2dt(filePath, 0, dt);
            return dt;
        }
        public static string wirteDataTableToFile(DataTable dt,string fileName)
        {
            string fullPath = FileHelper.MapPath(fileName);
            CSVHelper.SaveCSV(dt, fullPath);
            return fullPath;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="selectColName">需要添加的特征列</param>
        /// <param name="labelCol">预测列 mspj</param>
        /// <param name="valueCol">给预测列赋值的数据量 spj</param>
        /// <param name="days">取前N天数据组成特征值</param>
        /// <returns></returns>
        static DataTable AddColAndData(DataTable source,string[] selectColName, int days,string labelCol="mspj",string valueCol="spj")
        {
            DataTable dt = source.Copy();
            
            for (int j=0;j< selectColName.Length;j++)
            {
                for (int i = 0; i < source.Rows.Count; i++)
                {
                    
                    List<DataRow> rows = new List<DataRow>();
                    string colName = selectColName[j];
                    if(colName.Equals("riqi"))
                    {
                        continue;
                    }
                    if (colName.Equals(labelCol))
                    {
                        if (!dt.Columns.Contains(colName))
                        {
                            dt.Columns.Add(colName);
                        }
                        if(i>0)
                        dt.Rows[i][labelCol] = dt.Rows[i - 1][valueCol];
                    }
                    else
                    {

                        if (source.Rows.Count - i >= days)
                        {
                            rows = GetRowCollection(source, i+1, days);
                            for (int K = 0; K < rows.Count; K++)
                            {
                                string newColname = colName + K;
                                if (!dt.Columns.Contains(newColname))
                                {
                                    dt.Columns.Add(newColname);
                                }
                                dt.Rows[i][newColname] = rows[K][colName];
                            }
                           
                        }
                    }
                }
            }
            return dt;
        }

        static List<DataRow> GetRowCollection(DataTable dt,int startIndex,int count)
        {
            List<DataRow> list= dt.Rows.OfType<DataRow>().Skip(startIndex).Take(count).ToList();
            return list;
        }

        public static string CreateTestData(string filePath, int count)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = CSVHelper.csv2dt(filePath, 0, dt);

                dt = dt.AsEnumerable().Take(count+1).CopyToDataTable<DataRow>();
                string fullPath = FileHelper.MapPath("testData.csv");
                dt.Rows.RemoveAt(0);
                CSVHelper.SaveCSV(dt, fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void WriteToFile(string path,string pagestr)
        {
            FileHelper.CreateFile(path, pagestr, false);
        }
        public static void WriteToFile(string path, byte[] data)
        {
            FileHelper.CreateFile(path, data, false);
        }
    }
}
