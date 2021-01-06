using Microsoft.ML.Data;

namespace TemperatureEstimation.DataStructure
{
    public class CityTempData
    {
        [LoadColumn(0)]
        public string Country;

        [LoadColumn(1)]
        public string City;
        
        [LoadColumn(2)]
        public float Month;
        
        [LoadColumn(3)]
        public float Day;
        
        [LoadColumn(4)]
        public float Year;
        
        [LoadColumn(5)]
        public float AvgTemperature;
    }
}