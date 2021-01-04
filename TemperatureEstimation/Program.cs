using Microsoft.ML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TemperatureEstimation.DataStructure;

namespace TemperatureEstimation
{
    public class Program
    {
        private static string BaseDatasetPath = @"../../../Data";
        private static string DatasetRelativePath = $"{BaseDatasetPath}/dataset.csv";
        private static string DatasetPath = GetAbsolutePath(DatasetRelativePath);
        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string dataPath = GetAbsolutePath(BaseDatasetPath);
        private static string zipDataset = Path.Combine(dataPath, "input", "data.zip");
        private static string fullDatasetPath = Path.Combine(dataPath, "input", "data", "alldata.csv");
        private static string ModelRelativePath1 = $"{BaseModelsRelativePath}/SalesSpikeModel.zip";
        private static string ModelRelativePath2 = $"{BaseModelsRelativePath}/SalesChangePointModel.zip";
        private static string SpikeModelPath = GetAbsolutePath(ModelRelativePath1);
        private static string ChangePointModelPath = GetAbsolutePath(ModelRelativePath2);

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();

            UnZipDataset(zipDataset, fullDatasetPath);

            const int size = 0; // to do determined. Don't know if I will be using this variable

            IDataView dataView = mlContext.Data.LoadFromTextFile<CityTempData>(path: DatasetPath, hasHeader: true, separatorChar: ',');

            ITransformer trainedSpikeModel = DetectSpike(mlContext, size, dataView);

            ITransformer trainedChangePointModel = DetectChangepoint(mlContext, size, dataView);

            SaveModel(mlContext, trainedSpikeModel, ChangePointModelPath, dataView);
            SaveModel(mlContext, trainedChangePointModel, ChangePointModelPath, dataView);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadLine();
        }

        private static void UnZipDataset(string zipDataset, string destinationFile)
        {
            if (!File.Exists(destinationFile))
            {
                string destinationDir = Path.GetDirectoryName(destinationFile);
                ZipFile.ExtractToDirectory(zipDataset, $"{destinationDir}");
            }
        }

        private static void SaveModel(MLContext mlContext, ITransformer trainedModel, string modelPath, IDataView dataView)
        {
            Console.WriteLine("=============== Saving model ===============");

            mlContext.Model.Save(trainedModel, dataView.Schema, modelPath);

            Console.WriteLine($"The model is saved to {modelPath}");
        }

        private static ITransformer DetectChangepoint(MLContext mlContext, int size, IDataView dataView)
        {
            Console.WriteLine("===============Detect Persistent changes in pattern===============");

            var estimator = mlContext.Transforms.DetectIidChangePoint(outputColumnName: nameof(CityTempPrediction.),
                                                                    inputColumnName: nameof(CityTempPrediction.),
                                                                    confidence: 95,
                                                                    changeHistoryLength: size / 4);

            ITransformer transformedModel = estimator.Fit(CreateEmptyDataView(mlContext));

            IDataView transformedData = transformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<CityTempPrediction>(transformedData, reuseRowObject: false);

            Console.WriteLine($"{nameof(CityTempPrediction.Prediction)} column obtained post-transformation.");
            Console.WriteLine("Alert\tScore\tP-Value\tMarginale Value");

            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}  <-- alert is on, predicted changepoint", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
                }
                else
                {
                    Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}\t{3:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2], p.Prediction[3]);
                }
            }
            Console.WriteLine();

            return transformedModel;
        }

        private static ITransformer DetectSpike(MLContext mlContext, int size, IDataView dataView)
        {
            Console.WriteLine("===============Detect temporary changes in pattern===============");

            var estimator = mlContext.Transforms.DetectIidSpike(outputColumnName: nameof(CityTempData.),
                                                                inputColumnName: nameof(CityTempData.),
                                                                confidence: 95,
                                                                pvalueHistoryLength: size / 4);

            ITransformer transformedModel = estimator.Fit(CreateEmptyDataView(mlContext));

            IDataView transformedData = transformedModel.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<CityTempPrediction>(transformedData, reuseRowObject: false);

            Console.WriteLine("Alert\tScore\tP-Value");

            foreach (var p in predictions)
            {
                if (p.Prediction[0] == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine("{0}\t{1:0.00}\t{2:0.00}", p.Prediction[0], p.Prediction[1], p.Prediction[2]);
                Console.ResetColor();
            }

            Console.WriteLine();

            return transformedModel;
        }

        private static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        private static IDataView CreateEmptyDataView(MLContext mLContext)
        {
            IEnumerable<CityTempData> enumerableData = new List<CityTempData>();
            var dv = mLContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }
    }
}
