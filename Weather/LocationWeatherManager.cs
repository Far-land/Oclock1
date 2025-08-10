using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class LocationWeatherManager : MonoBehaviour
{
    // �ڲ��࣬���ڽ���IP��ѯ��JSON�����ⴴ�������ļ�
    [Serializable]
    private class IpPlusResponse
    {
        public bool success;
        public string data;
    }

    public static LocationWeatherManager Instance { get; private set; }

    // �������ݣ���UI�ű���ȡ
    public LocationData LoadedLocationData { get; private set; }
    public WeatherData LoadedWeatherData { get; private set; }

    // �¼�������֪ͨUIˢ��
    public event Action OnDataUpdated;

    // --- ˽�б��� ---
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

        // �ڵ�һʱ����ر�������
        LoadLocationData();
        LoadWeatherData();
    }

    void Start()
    {
        // �����س��ĵ���λ�������Ƿ���Ч
        // �����Ч������adcodeΪ�գ���˵�����״������������ѱ�ɾ��
        if (LoadedLocationData == null || string.IsNullOrEmpty(LoadedLocationData.adcode))
        {
            Debug.Log("δ������Ч�ĵ���λ�����ݣ����Զ���ʼ��ȡ...");
            StartFetchingData();
        }
        // �������޸ġ��������λ����Ч���ټ�����������Ƿ��ѹ�ʱ
        else if (LoadedWeatherData == null || LoadedWeatherData.lastUpdateDate != System.DateTime.UtcNow.ToString("yyyy-MM-dd"))
        {
            Debug.Log("���������ѹ�ʱ�򲻴��ڣ����Զ�����...");
            StartFetchingData();
        }
        else
        {
            Debug.Log("��������Ϊ���£�������¡�");
        }
    }

    /// <summary>
    /// ���������������ֶ�����һ������ˢ��
    /// </summary>
    public void StartFetchingData()
    {
        StartCoroutine(FetchDataRoutine());
    }

    /// <summary>
    /// ִ��������������ĺ���Э��
    /// </summary>
    private IEnumerator FetchDataRoutine()
    {
        // 1. ��ȡ����IP
        Debug.Log("��ʼ��ȡ����IP (ʹ�� ipplus360)...");
        UnityWebRequest ipRequest = UnityWebRequest.Get("https://www.ipplus360.com/getIP");
        ipRequest.certificateHandler = new BypassCertificateHandler();
        yield return ipRequest.SendWebRequest();

        if (ipRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("��ȡ����IPʧ��: " + ipRequest.error);
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
                Debug.LogError("IP��ѯAPI���ش���: " + ipRequest.downloadHandler.text);
                yield break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("��������IP��JSONʱʧ��: " + e.Message);
            yield break;
        }
        Debug.Log("��ȡ������IP: " + publicIp);

        // 2. ʹ��IP��ȡ����λ��
        string locationUrl = $"https://restapi.amap.com/v3/ip?ip={publicIp}&output=json&key={amapWebKey}";
        UnityWebRequest locationRequest = UnityWebRequest.Get(locationUrl);
        locationRequest.certificateHandler = new BypassCertificateHandler();
        yield return locationRequest.SendWebRequest();

        if (locationRequest.result != UnityWebRequest.Result.Success) { Debug.LogError("��ȡ����λ��ʧ��: " + locationRequest.error); yield break; }
        IpLocationResponse locResponse = JsonUtility.FromJson<IpLocationResponse>(locationRequest.downloadHandler.text);
        if (locResponse.status != "1") { Debug.LogError("�ߵ�API���ص���λ�ô���"); yield break; }

        LoadedLocationData = new LocationData { province = locResponse.province, city = locResponse.city, adcode = locResponse.adcode };
        SaveLocationData();
        Debug.Log($"��ȡ������λ��: {locResponse.city}");

        // 3. ʹ��adcode��ȡ����Ԥ��
        string weatherUrl = $"https://restapi.amap.com/v3/weather/weatherInfo?city={locResponse.adcode}&extensions=all&key={amapWebKey}";
        UnityWebRequest weatherRequest = UnityWebRequest.Get(weatherUrl);
        weatherRequest.certificateHandler = new BypassCertificateHandler();
        yield return weatherRequest.SendWebRequest();

        if (weatherRequest.result != UnityWebRequest.Result.Success) { Debug.LogError("��ȡ����ʧ��: " + weatherRequest.error); yield break; }
        WeatherResponse weatherResponse = JsonUtility.FromJson<WeatherResponse>(weatherRequest.downloadHandler.text);
        if (weatherResponse.status != "1" || weatherResponse.forecasts.Length == 0) { Debug.LogError("�ߵ�API������������"); yield break; }

        Forecast forecast = weatherResponse.forecasts[0];

        LoadedWeatherData = new WeatherData
        {
            city = forecast.city,
            reporttime = forecast.reporttime,
            // ����������¼���������
            lastUpdateDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
            forecasts = new System.Collections.Generic.List<DailyForecast>()
        };

        foreach (var cast in forecast.casts) { LoadedWeatherData.forecasts.Add(new DailyForecast { date = cast.date, week = cast.week, dayweather = cast.dayweather, nightweather = cast.nightweather, daytemp = cast.daytemp, nighttemp = cast.nighttemp }); }
        SaveWeatherData();
        Debug.Log($"��ȡ������Ԥ��: {forecast.city}");


        // 4. �������ݶ��ѻ�ȡ�����棬����֪ͨ
        OnDataUpdated?.Invoke();
    }

    // --- �浵��������� ---
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
            catch (Exception e) { Debug.LogError($"���� location.json ʧ��: {e.Message}"); }
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
            catch (Exception e) { Debug.LogError($"���� weather.json ʧ��: {e.Message}"); }
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