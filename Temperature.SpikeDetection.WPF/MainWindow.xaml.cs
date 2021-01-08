using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.ML;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using TemperatureEstimation;
using TemperatureEstimation.DataStructure;

namespace Temperature.SpikeDetection.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataTable dataTable = null;
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public string[] Labels { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public static string BaseModelsRelativePath = @"../../../../MLModels";
        public static string ModelRelativePath1 = $"{BaseModelsRelativePath}/ProductSalesSpikeModel.zip";
        public static string ModelRelativePath2 = $"{BaseModelsRelativePath}/ProductSalesChangePointModel.zip";
        private static string spikeModelPath = GetAbsolutePath(ModelRelativePath1);
        private static string changePointModelPath = GetAbsolutePath(ModelRelativePath2);


        //Don't know if i need to use this
        Tuple<string, string> tup = null;
        Dictionary<int, Tuple<string, string>> dict = new Dictionary<int, Tuple<string, string>>();


        public MainWindow()
        {
            InitializeComponent();
            PopulateGraph();
        }

        public void PopulateGraph()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 1,4,9,16,25,36,49,64,81,100,121,141,169,196 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 196,169,141,121,100,81,64,49,36,25,16,9,4,1 }
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            DataContext = this;
        }

        private void FindFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileExplorer = new OpenFileDialog();
            bool? result = openFileExplorer.ShowDialog();

            if (result == true)
            {
                DataPathBox.Text = openFileExplorer.FileName;
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        // Go button.
        private void DetectAnomalyButton_Click(object sender, RoutedEventArgs e)
        {
            FilePath = DataPathBox.Text;

            if (File.Exists(FilePath))
            {
                dict = new Dictionary<int, Tuple<string, string>>();

                if (FilePath != "")
                {
                    AnamolyText.Text = "";

                    //displayDataTableAndGraph();

                    DetectAnomalies();
                }
                else
                {
                    MessageBox.Show("Please input file path.");
                }
            }
            else
            {
                MessageBox.Show("File does not exist. Try finding the file again.");
            }
        }

        private void DetectAnomalies()
        {
            var mlcontext = new MLContext();

            IDataView dataView = mlcontext.Data.LoadFromTextFile<CityTempData>(path: FilePath, hasHeader: true, separatorChar: (bool)csvRadio.IsChecked ? ',' : '\t');

            if (SpikeCheckBox.IsChecked == true)
            {
                if (File.Exists(spikeModelPath))
                {
                    loadAndUseModel(mlcontext, dataView, spikeModelPath, "Spike", Color.DarkRed);
                }
                else
                {
                    MessageBox.Show("Spike detection model does not exist. Please run model training console app first.");
                }
            }
            if (ChangePointCheckBox.IsChecked == true)
            {

                if (File.Exists(changePointModelPath))
                {
                    loadAndUseModel(mlcontext, dataView, changePointModelPath, "Change point", Color.DarkBlue);
                }
                else
                {
                    MessageBox.Show("Change point detection model does not exist. Please run model training console app first.");
                }
            }
        }

        private void displayDataTableAndGraph()
        {
            dataTable = new DataTable();
            string[] dataCol = null;
            int a = 0;
            string xAxis = "";
            string yAxis = "";

            string[] dataset = File.ReadAllLines(FilePath);
            dataCol = (bool)csvRadio.IsChecked ? dataset[0].Split(',') : dataset[0].Split('\t');

            dataTable.Columns.Add(dataCol[0]);
            dataTable.Columns.Add(dataCol[1]);
            xAxis = dataCol[0];
            yAxis = dataCol[1];

            foreach (string line in dataset.Skip(1))
            {
                // Add next row of data.
                dataCol = (bool)csvRadio.IsChecked ? line.Split(',') : line.Split('\t');
                dataTable.Rows.Add(dataCol);

                tup = new Tuple<string, string>(dataCol[0], dataCol[1]);
                dict.Add(a, tup);

                a++;
            }

            // Set data view preview source.
            DataPreview.DataContext = dataTable;

            // Update y axis min and max values.
            double yMax = Convert.ToDouble(dataTable.Compute($"max([{yAxis}])", string.Empty));
            double yMin = Convert.ToDouble(dataTable.Compute($"min([{yAxis}])", string.Empty));

            // Set graph source.
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 1,4,9,16,25,36,49,64,81,100,121,141,169,196 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 196,169,141,121,100,81,64,49,36,25,16,9,4,1 }
                }
            };


            //// Set graph options.
            //graph.Series["Series1"].ChartType = SeriesChartType.Line;

            //graph.Series["Series1"].XValueMember = xAxis;
            //graph.Series["Series1"].YValueMembers = yAxis;

            //graph.Legends["Legend1"].Enabled = true;

            //graph.ChartAreas["ChartArea1"].AxisX.MajorGrid.LineWidth = 0;
            //graph.ChartAreas["ChartArea1"].AxisX.Interval = a / 10;

            //graph.ChartAreas["ChartArea1"].AxisY.Maximum = yMax;
            //graph.ChartAreas["ChartArea1"].AxisY.Minimum = yMin;
            //graph.ChartAreas["ChartArea1"].AxisY.Interval = yMax / 10;


            //graph.DataBind();

        }

        #region Functions from Microsoft Sample

        private void loadAndUseModel(MLContext mlcontext, IDataView dataView, String modelPath, String type, Color color)
        {
            ITransformer tansformedModel = mlcontext.Model.Load(modelPath, out var modelInputSchema);

            // Step 3: Apply data transformation to create predictions.
            IDataView transformedData = tansformedModel.Transform(dataView);
            var predictions = mlcontext.Data.CreateEnumerable<CityTempPrediction>(transformedData, reuseRowObject: false);

            // Index key for dictionary (date, sales).
            int a = 0;

            //foreach (var prediction in predictions)
            //{
            //    // Check if anomaly is predicted (indicated by an alert).
            //    if (prediction.Prediction[0] == 1)
            //    {
            //        // Get the date (year-month) where spike is detected.
            //        var xAxisDate = dict[a].Item1;
            //        // Get the number of sales which was detected to be a spike.
            //        var yAxisSalesNum = dict[a].Item2;

            //        // Add anomaly points to graph
            //        // and set point/marker options.
            //        graph.Series["Series1"].Points[a].SetValueXY(a, yAxisSalesNum);
            //        graph.Series["Series1"].Points[a].MarkerStyle = MarkerStyle.Star4;
            //        graph.Series["Series1"].Points[a].MarkerSize = 10;
            //        graph.Series["Series1"].Points[a].MarkerColor = color;

            //        // Print out anomalies as text for user &
            //        // change color of text accordingly.
            //        string text = type + " detected in " + xAxisDate + ": " + yAxisSalesNum + "\n";
            //        anomalyText.SelectionColor = color;
            //        anomalyText.AppendText(text);

            //        // Change row color in table where anomalies occur.
            //        DataGridViewRow row = DataPreview.Rows[a];
            //        row.DefaultCellStyle.BackColor = color;
            //        row.DefaultCellStyle.ForeColor = Color.White;
            //    }
            //    a++;
            //}
        }

        #endregion

    }
}