using UnityEngine;
using System;
using System.Linq;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    // 在Inspector中拖入您创建的音乐库资产
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
    /// 【核心方法】根据闹钟时间和天气，获取对应的情境铃声
    /// </summary>
    public AudioClip GetContextualRingtone(DateTime alarmTime, WeatherData weatherData)
    {
        if (musicLibrary == null)
        {
            Debug.LogError("MusicLibrary未在MusicManager的Inspector中设置！");
            return null;
        }

        // 1. 解析天气
        WeatherType currentActivity = ParseWeather(weatherData);

        // 2. 解析时间
        TimeOfDay timeOfDay;
        int hour12 = alarmTime.Hour;

        if (alarmTime.Hour == 12 && alarmTime.Minute >= 0 && alarmTime.Minute < 30)
        { // 中午12:00 - 12:29 算作 Noon
            timeOfDay = TimeOfDay.Noon;
        }
        else if (alarmTime.Hour == 0 && alarmTime.Minute < 30)
        { // 凌晨 0:00 - 0:29 算作 Midnight
            timeOfDay = TimeOfDay.Midnight;
        }
        else if (alarmTime.Hour >= 12)
        {
            timeOfDay = TimeOfDay.PM;
            if (hour12 > 12) hour12 -= 12; // 转换为12小时制 (13点 -> 1点)
        }
        else
        {
            timeOfDay = TimeOfDay.AM;
            if (hour12 == 0) hour12 = 12; // 0点是12 AM
        }

        Debug.Log($"正在查找铃声: 天气={currentActivity}, 时间段={timeOfDay}, 小时={hour12}");

        // 3. 在音乐库中查找最佳匹配
        ContextualMusicTrack bestMatch = musicLibrary.contextualTracks
            .FirstOrDefault(track => track.weather == currentActivity &&
                                      track.timeOfDay == timeOfDay &&
                                      // 只有AM/PM模式下才需要匹配小时
                                      (timeOfDay == TimeOfDay.AM || timeOfDay == TimeOfDay.PM ? track.hour == hour12 : true));

        // 4. 如果没找到完美匹配，尝试只匹配时间和任意天气 (Default)
        if (bestMatch == null)
        {
            Debug.Log("未找到完美匹配，尝试查找默认天气...");
            bestMatch = musicLibrary.contextualTracks
                .FirstOrDefault(track => track.weather == WeatherType.Default &&
                                          track.timeOfDay == timeOfDay &&
                                          (timeOfDay == TimeOfDay.AM || timeOfDay == TimeOfDay.PM ? track.hour == hour12 : true));
        }

        // 5. 如果还是找不到，返回默认铃声
        if (bestMatch != null && bestMatch.audioClip != null)
        {
            Debug.Log($"成功匹配到铃声: {bestMatch.trackName}");
            return bestMatch.audioClip;
        }
        else
        {
            Debug.LogWarning("未找到任何匹配的情境音乐，将使用默认铃声。");
            return musicLibrary.defaultRingtone;
        }
    }

    // 一个辅助方法，将天气字符串解析为我们的枚举
    private WeatherType ParseWeather(WeatherData data)
    {
        // 如果没有任何天气数据，返回默认
        if (data == null || data.forecasts == null || data.forecasts.Count == 0)
        {
            return WeatherType.Default;
        }

        // 我们通常关心白天的天气，所以获取今天(索引0)的dayweather
        string weatherString = data.forecasts[0].dayweather;

        // --- 【核心修改】采纳了您的新分类规则 ---

        // 1. 优先检查是否包含“雪”，因为“雨夹雪”也算雪天
        if (weatherString.Contains("雪"))
        {
            return WeatherType.Snowy;
        }

        // 2. 其次检查是否包含“雨”或相关的恶劣天气
        if (weatherString.Contains("雨") || weatherString.Contains("雷") || weatherString.Contains("雹") || weatherString.Contains("暴"))
        {
            return WeatherType.Rainy;
        }

        // 3. 【修改点】将“云、阴、风”都视为“晴”这一大类
        if (weatherString.Contains("晴") || weatherString.Contains("云") || weatherString.Contains("阴") || weatherString.Contains("风"))
        {
            return WeatherType.Sunny;
        }

        // 4. 剩下的天气（霾、雾、沙尘等）归为默认
        return WeatherType.Default;
    }
}