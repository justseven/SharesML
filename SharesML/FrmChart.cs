using GetTraceData;
using MLModel;
using SharesML.MLModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharesML
{
    public partial class FrmChart : Form
    {
        private string gpdm;
        private bool isSH;
        private uint traTime;
        private int testCoutn;
        private string filePath;
        private string trainPath;
        private string predictFilePath = string.Empty;
        public FrmChart()
        {
            InitializeComponent();
        }

        public FrmChart(string gpdm,bool isSH,uint time,decimal count)
        {
            this.gpdm = gpdm;
            this.isSH = isSH;
            this.traTime = time;
            this.testCoutn = Convert.ToInt32(count);
            InitializeComponent();
        }

        private void DownLoadData(string gpCode, string startTime = "20000101")
        {
            ThreadPool.QueueUserWorkItem((object obj) =>
            {
                List<TestModel> testSource = new List<TestModel>();
                List<TestModel> realSource = new List<TestModel>();
                string endTime = string.Format("{0:yyyyMMdd}", DateTime.Now);// DateTime.Now.ToString("yyyymmdd");
                string filePath = InitData.DownLoadFile(startTime, endTime, gpCode, isSH);
                int count = InitData.GetDataCount(filePath);
                string trainPath = InitData.CreateTrainData(filePath, "trainData.csv", 5, 0, ref predictFilePath);
                string testPath = InitData.CreateTestData(filePath, testCoutn);
                MyAutoML.TrainAndSave("mspj", trainPath, traTime);


                List<ModelInput> testList = MyAutoML.CreateDataSampleList(testPath);
                List<ModelInput> sourceList = MyAutoML.CreateDataSampleList(filePath);
                foreach (var item in testList)
                {

                    ModelOutput output = MyAutoML.LoadAndPrediction(item);
                    string day = item.Riqi;
                    ModelInput real = null;
                    do
                    {
                        day = GetNextDataStr(day, 1);
                        real = sourceList.FirstOrDefault(a => a.Riqi == day);
                    } while (null == real);
                    this.Invoke(new Action(() =>
                    {
                        this.chartControl1.Series[1].Points.Add(new DevExpress.XtraCharts.SeriesPoint(day, output.Score));
                        this.chartControl1.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint(day, real.Spj));
                    }));
                }
            });
        }


        private string GetNextDataStr(string dateStr,int addDay)
        {
            DateTime today = Convert.ToDateTime(dateStr);
            return today.AddDays(addDay).ToString("yyyy-MM-dd");
        }
        private void FrmChart_Load(object sender, EventArgs e)
        {
            DownLoadData(gpdm);
        }
    }
}
