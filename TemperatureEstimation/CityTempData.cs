using Microsoft.ML.Data;

namespace TemperatureEstimation.DataStructure
{
    public class CityTempData
    {
        [LoadColumn(0)]
        public float AvgTemperature;

        [LoadColumn(1)]
        public float DayOfYear;
    }
}