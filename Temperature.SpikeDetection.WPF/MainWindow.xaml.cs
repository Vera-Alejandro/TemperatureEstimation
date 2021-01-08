using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TemperatureEstimation;
using TemperatureEstimation.DataStructure;

namespace Temperature.SpikeDetection.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
