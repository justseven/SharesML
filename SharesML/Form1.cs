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
using Untils;

namespace SharesML
{
    public partial class Form1 : Form
    {
        private string filePath = string.Empty;
        private string trainPath = string.Empty;
        private string predictFilePath = string.Empty;
        private uint experimentTime = 10;
        private bool isSH = true;
        public Form1()
        {
            InitializeComponent();
            this.isSH = radioButton2.Checked;
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem((object obj) =>
                {
                    AddItemToListBox(this.lstResult, "数据下载中");
                    string gpCode = txtGPCode.Text.Trim();
                    DownLoadData(gpCode);
                    AddItemToListBox(this.lstResult, "数据下载完成");
                    AddItemToListBox(this.lstResult, "===========================================");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DownLoadData(string gpCode, string startTime = "20000101")
        {
            string endTime = string.Format("{0:yyyyMMdd}", DateTime.Now);// DateTime.Now.ToString("yyyymmdd");
            filePath = InitData.DownLoadFile(startTime, endTime, gpCode, isSH);
            trainPath = InitData.CreateTrainData(filePath, "trainData.csv",5,0,ref predictFilePath);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.isSH = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            this.isSH = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object obj) =>
            {
                try
                {
                    AddItemToListBox(this.lstResult, "训练数据中");
                    CreateAndTrain();
                    AddItemToListBox(this.lstResult, "训练数据完成");
                    AddItemToListBox(this.lstResult, "===========================================");
                }
                catch (Exception ex)
                {
                    AddItemToListBox(this.lstResult, $"出现异常，异常信息:{ex.Message}");
                    AddItemToListBox(this.lstResult, "===========================================");
                }
            });

        }

        private void SetControlText()
        {

        }

        private void AddItemToListBox(ListBox box, string content)
        {
            this.Invoke(new Action(() => {
                box.Items.Add(content);
                box.SelectedIndex = box.Items.Count - 1;
            }));
        }


        private void CreateAndTrain()
        {
            //ModelBuilder.CreateModel(trainPath);
            MyAutoML.TrainAndSave("mspj", trainPath, experimentTime);
        }

        private ModelOutput Predict(ModelInput sampleData)
        {
            var predictionResult = MyAutoML.LoadAndPrediction(sampleData);
            return predictionResult;
        }

        private void DeleteAll()
        {
            string[] exten = new string[] { ".csv", ".zip" };
            int count = 0;
            FileHelper.DeleteFiles(AppDomain.CurrentDomain.BaseDirectory, exten, false, true, ref count);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            ThreadPool.QueueUserWorkItem((object obj) =>
            {
                try
                {
                    DeleteAll();

                    AddItemToListBox(this.lstResult, "数据下载中");
                    string gpCode = txtGPCode.Text.Trim();
                    DownLoadData(gpCode);
                    AddItemToListBox(this.lstResult, "数据下载完成");
                    AddItemToListBox(this.lstResult, "===========================================");

                    AddItemToListBox(this.lstResult, "训练数据中");
                    CreateAndTrain();
                    AddItemToListBox(this.lstResult, "训练数据完成");
                    AddItemToListBox(this.lstResult, "===========================================");

                    ModelInput sampleData = MyAutoML.CreateSingleDataSample(predictFilePath);
                    var result = Predict(sampleData);
                    var day = GetNextDataStr(sampleData.Riqi, 1);
                    AddItemToListBox(this.lstResult, $"股票{this.txtGPCode.Text} {day}预测股价：{result.Score}；昨收盘：{sampleData.Spj}");
                }
                catch (Exception ex)
                {
                    AddItemToListBox(this.lstResult, $"出现异常，异常信息:{ex.Message}");
                    AddItemToListBox(this.lstResult, "===========================================");
                }
            });
        }

        private string GetNextDataStr(string dateStr, int addDay)
        {
            DateTime today = Convert.ToDateTime(dateStr);
            return today.AddDays(addDay).ToString("yyyy-MM-dd");
        }


        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string gpdm = txtGPCode.Text;
                string file = FileHelper.Search(gpdm);
                if (!string.IsNullOrEmpty(file) && file.Contains(gpdm))
                {
                    this.filePath = file;
                    ModelInput sampleData = MyAutoML.CreateSingleDataSample(filePath);
                    var result = Predict(sampleData);
                    AddItemToListBox(this.lstResult, $"股票{this.txtGPCode.Text}预测股价：{result.Score}");
                }
                else
                {
                    button2_Click(sender, e);
                }

            }
            catch (Exception ex)
            {
                AddItemToListBox(this.lstResult, $"出现异常，异常信息:{ex.Message}");
                AddItemToListBox(this.lstResult, "===========================================");
            }
        }

        private void numTime_ValueChanged(object sender, EventArgs e)
        {
            this.experimentTime = Convert.ToUInt32(this.numTime.Value);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FrmChart frmChart = new FrmChart(this.txtGPCode.Text, this.isSH,this.experimentTime,this.numTestNumber.Value);
            frmChart.ShowDialog();
        }
    }
}
