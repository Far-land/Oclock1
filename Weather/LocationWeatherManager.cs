using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class LocationWeatherManager : MonoBehaviour
{
    // 内部类，用于解析IP查询的JSON，避免创建多余文件
    [Serializable]
    private class IpPlusResponse
    {
        public bool success;
        public string data;
    }

    public static LocationWeatherManager Instance { get; private set; }

    // 公开数据，供UI脚本读取
    public LocationData LoadedLocationData { get; private set; }
    public WeatherData LoadedWeatherData { get; private set; }

    // 事件，用于通知UI刷新
    public event Action OnDataUpdated;

    // --- 私有变量 ---
    private string amapWebKey = "3141aceda87201690fa9953d4185cefd";
    private string locationSaveFile = "location.json";
    private string weatherSaveFile = "weather.json";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 在第一时间加载本地数据
        LoadLocationData();
        LoadWeatherData();
    }

    void Start()
    {
        // 检查加载出的地理位置数据是否有效
        // 如果无效（比如adcode为空），说明是首次启动或数据已被删除
        if (LoadedLocationData == null || string.IsNullOrEmpty(LoadedLocationData.adcode))
        {
            Debug.Log("未发现有效的地理位置数据，将自动开始获取...");
            StartFetchingData();
        }
        // 【核心修改】如果地理位置有效，再检查天气数据是否已过时
        else if (LoadedWeatherData == null || LoadedWeatherData.lastUpdateDate != System.DateTime.UtcNow.ToString("yyyy-MM-dd"))
        {
            Debug.Log("天气数据已过时或不存在，将自动更新...");
            StartFetchingData();
        }
        else
        {
            Debug.Log("天气数据为最新，无需更新。");
        }
    }

    /// <summary>
    /// 公共方法，用于手动触发一次数据刷新
    /// </summary>
    public void StartFetchingData()
    {
        StartCoroutine(FetchDataRoutine());
    }

    /// <summary>
    /// 执行所有网络请求的核心协程
    /// </summary>
    private IEnumerator FetchDataRoutine()
    {
        // 1. 获取公网IP
        Debug.Log("开始获取公网IP (使用 ipplus360)...");
        UnityWebRequest ipRequest = UnityWebRequest.Get("https://www.ipplus360.com/getIP");
        ipRequest.certificateHandler = new BypassCertificateHandler();
        yield return ipRequest.SendWebRequest();

        if (ipRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("获取公网IP失败: " + ipRequest.error);
            yield break;
        }

        string publicIp = "";
        try
        {
            IpPlusResponse ipResponse = JsonUtility.FromJson<IpPlusResponse>(ipRequest.downloadHandler.text);
            if (ipResponse != null && ipResponse.success)
            {
                publicIp = ipResponse.data;
            }
            else
            {
                Debug.LogError("IP查询API返回错误: " + ipRequest.downloadHandler.text);
                yield break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("解析公网IP的JSON时失败: " + e.Message);
            yield break;
        }
        Debug.Log("获取到公网IP: " + publicIp);

        // 2. 使用IP获取地理位置
        string locationUrl = $"https://restapi.amap.com/v3/ip?ip={publicIp}&output=json&key={amapWebKey}";
        UnityWebRequest locationRequest = UnityWebRequest.Get(locationUrl);
        locationRequest.certificateHandler = new BypassCertificateHandler();
        yield return locationRequest.SendWebRequest();

        if (locationRequest.result != UnityWebRequest.Result.Success) { Debug.LogError("获取地理位置失败: " + locationRequest.error); yield break; }
        IpLocationResponse locResponse = JsonUtility.FromJson<IpLocationResponse>(locationRequest.downloadHandler.text);
        if (locResponse.status != "1") { Debug.LogError("高德API返回地理位置错误"); yield break; }

        LoadedLocationData = new LocationData { province = locResponse.province, city = locResponse.city, adcode = locResponse.adcode };
        SaveLocationData();
        Debug.Log($"获取到地理位置: {locResponse.city}");

        // 3. 使用adcode获取天气预报
        string weatherUrl = $"https://restapi.amap.com/v3/weather/weatherInfo?city={locResponse.adcode}&extensions=all&key={amapWebKey}";
        UnityWebRequest weatherRequest = UnityWebRequest.Get(weatherUrl);
        weatherRequest.certificateHandler = new BypassCertificateHandler();
        yield return weatherRequest.SendWebRequest();

        if (weatherRequest.result != UnityWebRequest.Result.Success) { Debug.LogError("获取天气失败: " + weatherRequest.error); yield break; }
        WeatherResponse weatherResponse = JsonUtility.FromJson<WeatherResponse>(weatherRequest.downloadHandler.text);
        if (weatherResponse.status != "1" || weatherResponse.forecasts.Length == 0) { Debug.LogError("高德API返回天气错误"); yield break; }

        Forecast forecast = weatherResponse.forecasts[0];

        LoadedWeatherData = new WeatherData
        {
            city = forecast.city,
            reporttime = forecast.reporttime,
            // 【新增】记录今天的日期
            lastUpdateDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
            forecasts = new System.Collections.Generic.List<DailyForecast>()
        };

        foreach (var cast in forecast.casts) { LoadedWeatherData.forecasts.Add(new DailyForecast { date = cast.date, week = cast.week, dayweather = cast.dayweather, nightweather = cast.nightweather, daytemp = cast.daytemp, nighttemp = cast.nighttemp }); }
        SaveWeatherData();
        Debug.Log($"获取到天气预报: {forecast.city}");


        // 4. 所有数据都已获取并保存，发送通知
        OnDataUpdated?.Invoke();
    }

    // --- 存档与读档方法 ---
    private void LoadLocationData()
    {
        string path = Path.Combine(Application.persistentDataPath, locationSaveFile);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                LoadedLocationData = JsonUtility.FromJson<LocationData>(json);
            }
            catch (Exception e) { Debug.LogError($"加载 location.json 失败: {e.Message}"); }
        }
        if (LoadedLocationData == null) { LoadedLocationData = new LocationData(); }
    }

    private void LoadWeatherData()
    {
        string path = Path.Combine(Application.persistentDataPath, weatherSaveFile);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                LoadedWeatherData = JsonUtility.FromJson<WeatherData>(json);
            }
            catch (Exception e) { Debug.LogError($"加载 weather.json 失败: {e.Message}"); }
        }
        if (LoadedWeatherData == null) { LoadedWeatherData = new WeatherData(); }
    }

    private void SaveLocationData()
    {
        string path = Path.Combine(Application.persistentDataPath, locationSaveFile);
        string json = JsonUtility.ToJson(LoadedLocationData, true);
        File.WriteAllText(path, json);
    }

    private void SaveWeatherData()
    {
        string path = Path.Combine(Application.persistentDataPath, weatherSaveFile);
        string json = JsonUtility.ToJson(LoadedWeatherData, true);
        File.WriteAllText(path, json);
    }
}