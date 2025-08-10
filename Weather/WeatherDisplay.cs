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
        // ����ʱҲ���ѱ�������ݸ���һ��
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
            displayText.text = "���ڻ�ȡλ�ú�������Ϣ...";
            return;
        }

        // ��ȡ���������Ԥ�� (�б��еĵ�һ��)
        var todayForecast = weatherData.forecasts[0];

        displayText.text = $"�ص�: {locData.city}\n" +
                           $"����: {todayForecast.dayweather} / {todayForecast.nightweather}\n" +
                           $"�¶�: {todayForecast.nighttemp}��C ~ {todayForecast.daytemp}��C";
    }
}