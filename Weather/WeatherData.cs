using System.Collections.Generic;

[System.Serializable]
public class WeatherData
{
    public string city;
    public string reporttime;
    public string lastUpdateDate;

    public List<DailyForecast> forecasts;

    public WeatherData()
    {
        forecasts = new List<DailyForecast>();
        lastUpdateDate = "1970-01-01"; // ��һ�������Ĭ��ֵ
    }
}

[System.Serializable]
public class DailyForecast
{
    public string date;
    public string week;
    public string dayweather;
    public string nightweather;
    public string daytemp;
    public string nighttemp;
}