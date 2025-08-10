using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 全局时间管理器 (单例)
/// 负责从网络同步时间，并提供可靠的当前时间。
/// 使用事件驱动机制通知其他系统时间变化。
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    // 公共事件，供其他脚本订阅
    public event Action OnTimeSynced; // 当网络时间同步成功时触发
    public event Action<DateTime> OnSecondTick; // 每秒触发
    public event Action<DateTime> OnDateChanged; // 日期变化时触发

    // 公开属性，获取当前可信时间
    public DateTime CurrentTime { get; private set; }
    public bool IsTimeSynced { get; private set; } = false;

    private const string ApiUrl = "https://worldtimeapi.org/api/timezone/Etc/UTC";
    private DateTime _syncedUtcTime;
    private float _timeAtSync;
    private int _lastCheckedDay = -1;

    void Awake()
    {
        // 设置单例模式，确保全局唯一
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 切换场景时保留
    }

    void Start()
    {
        // 立即开始计时，先使用本地时间，然后尝试网络同步
        CurrentTime = DateTime.UtcNow;
        StartCoroutine(SyncTimeRoutine());
    }

    // 核心计时协程
    IEnumerator SyncTimeRoutine()
    {
        // 1. 获取网络时间
        yield return StartCoroutine(GetNetworkTime());

        // 2. 无论同步成功与否，都开始本地计时循环
        while (true)
        {
            // 基于同步结果或本地时间来更新当前时间
            if (IsTimeSynced)
            {
                // 计算自同步以来经过的时间
                float elapsedSeconds = Time.realtimeSinceStartup - _timeAtSync;
                CurrentTime = _syncedUtcTime.AddSeconds(elapsedSeconds);
            }
            else
            {
                // 网络同步失败，回退到设备UTC时间
                CurrentTime = DateTime.UtcNow;
            }

            // 触发每秒事件
            OnSecondTick?.Invoke(CurrentTime);

            // 检查日期是否变化
            if (_lastCheckedDay != CurrentTime.Day)
            {
                _lastCheckedDay = CurrentTime.Day;
                OnDateChanged?.Invoke(CurrentTime);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator GetNetworkTime()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ApiUrl))
        {
            Debug.Log("正在同步网络时间...");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    TimeResponse response = JsonUtility.FromJson<TimeResponse>(webRequest.downloadHandler.text);
                    // 使用Rfc3339/ISO-8601格式进行精确解析
                    _syncedUtcTime = DateTime.Parse(response.datetime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    _timeAtSync = Time.realtimeSinceStartup;
                    IsTimeSynced = true;
                    _lastCheckedDay = _syncedUtcTime.Day;

                    Debug.Log($"网络时间同步成功: {_syncedUtcTime}");
                    OnTimeSynced?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"时间解析失败: {e.Message}. 将使用本地时间。");
                    IsTimeSynced = false;
                }
            }
            else
            {
                Debug.LogWarning("网络请求失败，将使用本地时间。");
                IsTimeSynced = false;
            }
        }
    }

    // 用于JSON反序列化的辅助类
    [System.Serializable]
    private class TimeResponse
    {
        public string datetime;
    }
}