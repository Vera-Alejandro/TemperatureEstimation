using Microsoft.ML.Data;

namespace TemperatureEstimation.DataStructure
{
    public class CityTempPrediction
    {
        [VectorType(3)]
        public double[] Prediction { get; set; }
    }
}