using MLNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MLTools
{
    public class CSVHelper
    {
        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static void SaveCSV(DataTable dt, string fullPath)
        {
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                FileInfo fi = new FileInfo(fullPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                string data = "";
                //写出列名称
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName.ToString();
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
                //写出各行数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        string str = dt.Rows[i][j].ToString();
                        str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                        if (str.Contains(",") || str.Contains("\"")
                            || str.Contains("\\r") || str.Contains("\\n")) //含逗号 冒号 换行符的需要放到引号中
                        {
                            str = string.Format("\"{0}\"", str);
                        }

                        data += str;
                        if (j < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);
                }
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if(null!=sw)
                    sw.Close();
                if(null!=fs)
                    fs.Close();
            }
        }

        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static DataTable OpenCSV(string filePath)
        {
            FileStream fs=null;
            StreamReader sr = null;
            try
            {
                Encoding encoding = TxtFileEncoder.GetEncoding(filePath); //Encoding.ASCII;//
                DataTable dt = new DataTable();
                fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                sr = new StreamReader(fs, encoding);
                //string fileContent = sr.ReadToEnd();
                //encoding = sr.CurrentEncoding;
                //记录每次读取的一行记录
                string strLine = "";
                //记录每行记录中的各字段内容
                string[] aryLine = null;
                string[] tableHead = null;
                //标示列数
                int columnCount = 0;
                //标示是否是读取的第一行
                bool IsFirst = true;
                //逐行读取CSV中的数据
                while ((strLine = sr.ReadLine()) != null)
                {
                    //strLine = Common.ConvertStringUTF8(strLine, encoding);
                    //strLine = Common.ConvertStringUTF8(strLine);

                    if (IsFirst == true)
                    {
                        tableHead = strLine.Split(',');
                        IsFirst = false;
                        columnCount = tableHead.Length;
                        //创建列
                        for (int i = 0; i < columnCount; i++)
                        {
                            DataColumn dc = new DataColumn(tableHead[i]);
                            dt.Columns.Add(dc);
                        }
                    }
                    else
                    {
                        aryLine = strLine.Split(',');
                        DataRow dr = dt.NewRow();
                        for (int j = 0; j < columnCount; j++)
                        {
                            dr[j] = aryLine[j];
                        }
                        dt.Rows.Add(dr);
                    }
                }
                if (aryLine != null && aryLine.Length > 0)
                {
                    dt.DefaultView.Sort = tableHead[0] + " " + "asc";
                }

                sr.Close();
                fs.Close();
                return dt;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if(null!=sr)
                    sr.Close();
                if (null != fs)
                    fs.Close();
            }
        }
        /// <summary>
        /// 获取路径下的.csv文件
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        /// <summary>
        /// 将Csv读入DataTable
        /// </summary>
        /// <param name="filePath">csv文件路径</param>
        /// <param name="n">表示第n行是字段title,第n+1行是记录开始</param>
        /// <param name="k">可选参数表示最后K行不算记录默认0</param>
        public static DataTable csv2dt(string filePath, int n, DataTable dt) //这个dt 是个空白的没有任何行列的DataTable
        {
            String csvSplitBy = "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)";
            StreamReader reader = new StreamReader(filePath, System.Text.Encoding.UTF8, false);
            int i = 0, m = 0;
            reader.Peek();
            while (reader.Peek() > 0)
            {
                m = m + 1;
                string str = reader.ReadLine();
                if (m >= n + 1)
                {
                    if (m == n + 1) //如果是字段行，则自动加入字段。
                    {
                        MatchCollection mcs = Regex.Matches(str, csvSplitBy);
                        foreach (Match mc in mcs)
                        {
                            dt.Columns.Add(mc.Value); //增加列标题
                        }

                    }
                    else
                    {
                        MatchCollection mcs = Regex.Matches(str, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
                        i = 0;
                        System.Data.DataRow dr = dt.NewRow();
                        foreach (Match mc in mcs)
                        {
                            dr[i] = mc.Value;
                            i++;
                        }
                        dt.Rows.Add(dr);  //DataTable 增加一行     
                    }

                }
            }
            return dt;
        }






    }
}
