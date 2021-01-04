using Microsoft.ML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TemperatureEstimation
{
    public class Program
    {
        private static string BaseDatasetPath = @"../../../../Data";
        private static string DatasetRelativePath = $"{BaseDatasetPath}/Product-sales.csv";
        private static string DatasetPath = GetAbsolutePath(DatasetRelativePath);
        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath1 = $"{BaseModelsRelativePath}/ProductSalesSpikeModel.zip";
        private static string ModelRelativePath2 = $"{BaseModelsRelativePath}/ProductSalesChangePointModel.zip";
        private static string SpikeModelPath = GetAbsolutePath(ModelRelativePath1);
        private static string ChangePointModelPath = GetAbsolutePath(ModelRelativePath2);

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();

            const int size = 0; // to do determined. Don't know if I will be using this variable

            IDataView dataView = mlContext.Data.LoadFromTextFile<CityTempData>(path: DatasetPath, hasHeader: true, separatorChar: ',');

            ITransformer trainedSpikeModel = DetectSpike(size, dataView);

            ITransformer trainedChangePointModel = DetectChangepoint(size, dataView);

            SaveModel(mlContext, trainedSpikeModel, ChangePointModelPath, dataView);
            SaveModel(mlContext, trainedChangePointModel, ChangePointModelPath, dataView);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadLine();
        }

        private static void SaveModel(MLContext mlContext, ITransformer trainedModel, string modelPath, IDataView dataView)
        {
            Console.WriteLine("=============== Saving model ===============");

            mlContext.Model.Save(trainedModel, dataView.Schema, modelPath);

            Console.WriteLine($"The model is saved to {modelPath}");
        }

        private static ITransformer DetectChangepoint(int size, IDataView dataView)
        {
            throw new NotImplementedException();
        }

        private static ITransformer DetectSpike(int size, IDataView dataView)
        {
            throw new NotImplementedException();
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
