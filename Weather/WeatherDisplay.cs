using UnityEngine;
using TMPro;

public class WeatherDisplay : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    void OnEnable()
    {
        if (LocationWeatherManager.Instance != null)
        {
            LocationWeatherManager.Instance.OnDataUpdated += UpdateDisplay;
        }
        // 启动时也用已保存的数据更新一次
        UpdateDisplay();
    }

    void OnDisable()
    {
        if (LocationWeatherManager.Instance != null)
        {
            LocationWeatherManager.Instance.OnDataUpdated -= UpdateDisplay;
        }
    }

    void UpdateDisplay()
    {
        var locData = LocationWeatherManager.Instance.LoadedLocationData;
        var weatherData = LocationWeatherManager.Instance.LoadedWeatherData;

        if (locData == null || weatherData == null || weatherData.forecasts == null || weatherData.forecasts.Count == 0)
        {
            displayText.text = "正在获取位置和天气信息...";
            return;
        }

        // 获取今天的天气预报 (列表中的第一个)
        var todayForecast = weatherData.forecasts[0];

        displayText.text = $"地点: {locData.city}\n" +
                           $"天气: {todayForecast.dayweather} / {todayForecast.nightweather}\n" +
                           $"温度: {todayForecast.nighttemp}°C ~ {todayForecast.daytemp}°C";
    }
}