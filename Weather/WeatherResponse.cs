[System.Serializable]
public class WeatherResponse
{
    public string status;
    public Forecast[] forecasts;
}

[System.Serializable]
public class Forecast
{
    public string city;
    public string adcode;
    public string province;
    public string reporttime;
    public Cast[] casts;
}

[System.Serializable]
public class Cast
{
    public string date;
    public string week;
    public string dayweather;
    public string nightweather;
    public string daytemp;
    public string nighttemp;
}