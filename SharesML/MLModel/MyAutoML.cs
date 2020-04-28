using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using MLModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesML.MLModel
{
   public class MyAutoML
    {
        static readonly string ModelFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}MLModel.zip";
       /* static readonly string TrainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "winequality-data-train.csv");
        static readonly string TestDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "winequality-data-test.csv");*/
        public static void TrainAndSave(string label,string trainDataPath,uint experimentTime)
        {
            MLContext mlContext = new MLContext(seed: 0);

            // 准备数据 
            var trainData = mlContext.Data.LoadFromTextFile<ModelInput>(path: trainDataPath, separatorChar: ',', hasHeader: true);
            //var testData = mlContext.Data.LoadFromTextFile<ModelInput>(path: TestDataPath, separatorChar: ',', hasHeader: true);
           
            var testData = mlContext.Data.TrainTestSplit(trainData, testFraction: 0.2).TestSet;
            var progressHandler = new RegressionExperimentProgressHandler();
            //uint ExperimentTime = 200;

            ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
               .CreateRegressionExperiment(experimentTime)
               .Execute(trainData,label, progressHandler: progressHandler);

            //Debugger.PrintTopModels(experimentResult);

            RunDetail<RegressionMetrics> best = experimentResult.BestRun;
            ITransformer trainedModel = best.Model;

            // 评估 BestRun
            var predictions = trainedModel.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: label, scoreColumnName: "Score");
            //Debugger.PrintRegressionMetrics(best.TrainerName, metrics);

            // 保存模型
            using (var stream = System.IO.File.Create(ModelFilePath))
            {
                mlContext.Model.Save(trainedModel, trainData.Schema, stream);
            }
        }

        /// <summary>
        /// 获取验证数据
        /// </summary>
        /// <param name="dataFilePath"></param>
        /// <returns></returns>
        public static ModelInput CreateSingleDataSample(string dataFilePath)
        {
            // Create MLContext
            MLContext mlContext = new MLContext(seed:0);

            // Load dataset
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: dataFilePath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            allowSparse: false);

            // Use first line of dataset as model input
            // You can replace this with new test data (hardcoded or from end-user application)
            ModelInput sampleForPrediction = mlContext.Data.CreateEnumerable<ModelInput>(dataView, false)
                                                                        .First();
            return sampleForPrediction;
        }

        public static List<ModelInput> CreateDataSampleList(string dataFilePath)
        {
            // Create MLContext
            MLContext mlContext = new MLContext(seed:0);

            // Load dataset
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: dataFilePath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            allowSparse: false);

            // Use first line of dataset as model input
            // You can replace this with new test data (hardcoded or from end-user application)
            List<ModelInput> sampleForPrediction = mlContext.Data.CreateEnumerable<ModelInput>(dataView, false)
                                                                        .ToList();
            return sampleForPrediction;
        }




        public static ModelOutput LoadAndPrediction(ModelInput input)
        {
            MLContext mlContext = new MLContext(seed: 0);

            ITransformer loadedModel = mlContext.Model.Load(ModelFilePath, out var modelInputSchema);
            var predictor = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(loadedModel);

            return predictor.Predict(input);
           
        }
    }
}
