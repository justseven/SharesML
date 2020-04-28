// This file was auto-generated by ML.NET Model Builder. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using MLNETML.Model;

namespace MLNETML.Model
{

    public static class ModelBuilder
    {
        //private static string TRAIN_DATA_FILEPATH = @"F:\dotnet\MLNET\MLNET\bin\Debug\netcoreapp3.1\601857.csv";
        //private static string MODEL_FILEPATH = @"C:\Users\Administrator\AppData\Local\Temp\MLVSTools\MLNETML\MLNETML.Model\MLModel.zip";
        // Create MLContext to be shared across the model creation workflow objects 
        // Set a random seed for repeatable/deterministic results across multiple trainings.
        private static MLContext mlContext = new MLContext(seed: 1);
        private static string MODEL_FILEPATH = $"{AppDomain.CurrentDomain.BaseDirectory}MLModel.zip";
        public static void CreateModel(string TRAIN_DATA_FILEPATH)
        {
            // Load Data
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: TRAIN_DATA_FILEPATH,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            allowSparse: false);

            // Build training pipeline
            IEstimator<ITransformer> trainingPipeline = BuildTrainingPipeline(mlContext);

            // Evaluate quality of Model
            Evaluate(mlContext, trainingDataView, trainingPipeline);

            // Train Model
            ITransformer mlModel = TrainModel(mlContext, trainingDataView, trainingPipeline);
            //string MODEL_FILEPATH = AppDomain.CurrentDomain.BaseDirectory + ModelName;
            // Save model
            SaveModel(mlContext, mlModel, MODEL_FILEPATH, trainingDataView.Schema);
            //ConsumeModel.ModelfilePath = MODEL_FILEPATH;
        }

        public static IEstimator<ITransformer> BuildTrainingPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations 
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", new[] { "zgj", "zdj", "kpj", "qsp", "hsl", "cjl", "cjje", "zsz", "ltsz" });
            // Set the training algorithm 
            var trainer = mlContext.Regression.Trainers.FastTree(labelColumnName: "mspj", featureColumnName: "Features");

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            return trainingPipeline;
        }

        public static ITransformer TrainModel(MLContext mlContext, IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
        {
            ITransformer model = trainingPipeline.Fit(trainingDataView);
            return model;
        }

        private static IEnumerable<TrainCatalogBase.CrossValidationResult<RegressionMetrics>> Evaluate(MLContext mlContext, IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
        {
            // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            var crossValidationResults = mlContext.Regression.CrossValidate(trainingDataView, trainingPipeline, numberOfFolds: 5, labelColumnName: "spj");
            //PrintRegressionFoldsAverageMetrics(crossValidationResults);
            return crossValidationResults;
        }

        private static void SaveModel(MLContext mlContext, ITransformer mlModel, string modelRelativePath, DataViewSchema modelInputSchema)
        {
            // Save/persist the trained model to a .ZIP file
            
            if(File.Exists(modelRelativePath))
            {
                FileInfo file = new FileInfo(modelRelativePath);
                file.Delete();
            }
            mlContext.Model.Save(mlModel, modelInputSchema, modelRelativePath);//GetAbsolutePath(modelRelativePath
            //Console.WriteLine("The model is saved to {0}", modelRelativePath);// GetAbsolutePath(modelRelativePath)
        }

      
    }
}