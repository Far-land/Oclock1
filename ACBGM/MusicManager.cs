using UnityEngine;
using System;
using System.Linq;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    // ��Inspector�����������������ֿ��ʲ�
    public ContextualMusicLibrary musicLibrary;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// �����ķ�������������ʱ�����������ȡ��Ӧ���龳����
    /// </summary>
    public AudioClip GetContextualRingtone(DateTime alarmTime, WeatherData weatherData)
    {
        if (musicLibrary == null)
        {
            Debug.LogError("MusicLibraryδ��MusicManager��Inspector�����ã�");
            return null;
        }

        // 1. ��������
        WeatherType currentActivity = ParseWeather(weatherData);

        // 2. ����ʱ��
        TimeOfDay timeOfDay;
        int hour12 = alarmTime.Hour;

        if (alarmTime.Hour == 12 && alarmTime.Minute >= 0 && alarmTime.Minute < 30)
        { // ����12:00 - 12:29 ���� Noon
            timeOfDay = TimeOfDay.Noon;
        }
        else if (alarmTime.Hour == 0 && alarmTime.Minute < 30)
        { // �賿 0:00 - 0:29 ���� Midnight
            timeOfDay = TimeOfDay.Midnight;
        }
        else if (alarmTime.Hour >= 12)
        {
            timeOfDay = TimeOfDay.PM;
            if (hour12 > 12) hour12 -= 12; // ת��Ϊ12Сʱ�� (13�� -> 1��)
        }
        else
        {
            timeOfDay = TimeOfDay.AM;
            if (hour12 == 0) hour12 = 12; // 0����12 AM
        }

        Debug.Log($"���ڲ�������: ����={currentActivity}, ʱ���={timeOfDay}, Сʱ={hour12}");

        // 3. �����ֿ��в������ƥ��
        ContextualMusicTrack bestMatch = musicLibrary.contextualTracks
            .FirstOrDefault(track => track.weather == currentActivity &&
                                      track.timeOfDay == timeOfDay &&
                                      // ֻ��AM/PMģʽ�²���Ҫƥ��Сʱ
                                      (timeOfDay == TimeOfDay.AM || timeOfDay == TimeOfDay.PM ? track.hour == hour12 : true));

        // 4. ���û�ҵ�����ƥ�䣬����ֻƥ��ʱ����������� (Default)
        if (bestMatch == null)
        {
            Debug.Log("δ�ҵ�����ƥ�䣬���Բ���Ĭ������...");
            bestMatch = musicLibrary.contextualTracks
                .FirstOrDefault(track => track.weather == WeatherType.Default &&
                                          track.timeOfDay == timeOfDay &&
                                          (timeOfDay == TimeOfDay.AM || timeOfDay == TimeOfDay.PM ? track.hour == hour12 : true));
        }

        // 5. ��������Ҳ���������Ĭ������
        if (bestMatch != null && bestMatch.audioClip != null)
        {
            Debug.Log($"�ɹ�ƥ�䵽����: {bestMatch.trackName}");
            return bestMatch.audioClip;
        }
        else
        {
            Debug.LogWarning("δ�ҵ��κ�ƥ����龳���֣���ʹ��Ĭ��������");
            return musicLibrary.defaultRingtone;
        }
    }

    // һ�������������������ַ�������Ϊ���ǵ�ö��
    private WeatherType ParseWeather(WeatherData data)
    {
        // ���û���κ��������ݣ�����Ĭ��
        if (data == null || data.forecasts == null || data.forecasts.Count == 0)
        {
            return WeatherType.Default;
        }

        // ����ͨ�����İ�������������Ի�ȡ����(����0)��dayweather
        string weatherString = data.forecasts[0].dayweather;

        // --- �������޸ġ������������·������ ---

        // 1. ���ȼ���Ƿ������ѩ������Ϊ�����ѩ��Ҳ��ѩ��
        if (weatherString.Contains("ѩ"))
        {
            return WeatherType.Snowy;
        }

        // 2. ��μ���Ƿ�������ꡱ����صĶ�������
        if (weatherString.Contains("��") || weatherString.Contains("��") || weatherString.Contains("��") || weatherString.Contains("��"))
        {
            return WeatherType.Rainy;
        }

        // 3. ���޸ĵ㡿�����ơ������硱����Ϊ���硱��һ����
        if (weatherString.Contains("��") || weatherString.Contains("��") || weatherString.Contains("��") || weatherString.Contains("��"))
        {
            return WeatherType.Sunny;
        }

        // 4. ʣ�µ�������������ɳ���ȣ���ΪĬ��
        return WeatherType.Default;
    }
}