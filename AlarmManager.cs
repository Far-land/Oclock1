using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq; // 引入Linq，用于 FirstOrDefault 等方法

public class AlarmManager : MonoBehaviour
{
    // 单例实例，确保全局唯一
    public static AlarmManager Instance { get; private set; }

    // 公开的可信时间属性 (UTC时间)
    public DateTime CurrentTime { get; private set; }

    // 公开事件，用于通知其他系统
    public event Action<Alarm> OnAlarmTriggered;
    public event Action OnAlarmListChanged;

    // 私有变量
    private List<Alarm> alarms = new List<Alarm>();
    private string saveFileName = "alarms.json";
    private string savePath;

    void Awake()
    {
        // --- 初始化单例 ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- 初始化性能设置 ---
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        // --- 初始化存档路径并加载数据 ---
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"闹钟数据将保存到: {savePath}");
        LoadAlarms();
    }

    void Start()
    {
        // 启动两个独立的协程，分别负责时间更新和闹钟检查
        StartCoroutine(UpdateTimePropertyRoutine()); // 高频率更新时间，供UI使用
        StartCoroutine(CheckAlarmsRoutine());      // 低频率检查闹钟，节省性能
    }

    /// <summary>
    /// 这个协程专门负责每秒更新CurrentTime属性
    /// </summary>
    private IEnumerator UpdateTimePropertyRoutine()
    {
        // (在未来的版本中，可以在这里加入网络时间同步逻辑)
        while (true)
        {
            CurrentTime = DateTime.UtcNow; // 使用设备的标准UTC时间
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 这个协程只负责检查闹钟逻辑，低频率运行
    /// </summary>
    private IEnumerator CheckAlarmsRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (true)
        {
            DateTime localCurrentTime = CurrentTime.ToLocalTime();

            foreach (var alarm in alarms)
            {
                // 【核心修改】在这里调用新的IsScheduledFor方法，并传入我们的权威时间
                if (alarm.isEnabled &&
                    alarm.hour == localCurrentTime.Hour &&
                    alarm.minute == localCurrentTime.Minute &&
                    alarm.IsScheduledFor(localCurrentTime)) // <-- 修改了这里
                {
                    string triggerKey = $"triggered_{alarm.id}_{localCurrentTime:yyyyMMdd}";
                    if (PlayerPrefs.GetInt(triggerKey, 0) == 0)
                    {
                        // 找到了一个完全匹配的闹钟，触发事件！
                        OnAlarmTriggered?.Invoke(alarm);

                        PlayerPrefs.SetInt(triggerKey, 1);
                        if (alarm.repeatDays.Count == 0)
                        {
                            alarm.isEnabled = false;
                            NotifyDataChanged();
                        }
                    }
                }
            }

            int secondsToWait = 60 - localCurrentTime.Second;
            yield return new WaitForSeconds(Mathf.Max(1, secondsToWait));
        }
    }

    /// <summary>
    /// 核心逻辑：计算并返回下一个即将响铃的闹钟及其具体触发时间
    /// </summary>
    public (Alarm alarm, DateTime triggerTime)? GetNextUpcomingAlarm()
    {
        (Alarm alarm, DateTime triggerTime)? nextUpcoming = null;

        // 【核心修复】我们所有的计算都基于用户的本地时间，避免时区混淆
        DateTime now = this.CurrentTime.ToLocalTime();

        foreach (var alarm in alarms)
        {
            if (!alarm.isEnabled) continue;

            // 1. 处理周期性闹钟
            if (alarm.repeatDays != null && alarm.repeatDays.Count > 0)
            {
                // 从今天开始，检查未来8天
                for (int i = 0; i < 8; i++)
                {
                    DateTime checkDay = now.Date.AddDays(i);
                    if (alarm.repeatDays.Contains(checkDay.DayOfWeek))
                    {
                        DateTime potentialTriggerTime = new DateTime(
                            checkDay.Year, checkDay.Month, checkDay.Day,
                            alarm.hour, alarm.minute, 0
                        );

                        // 如果这个潜在时间点比现在晚
                        if (potentialTriggerTime > now)
                        {
                            // 转换为UTC进行统一存储和比较
                            DateTime potentialTriggerTimeUTC = potentialTriggerTime.ToUniversalTime();
                            if (nextUpcoming == null || potentialTriggerTimeUTC < nextUpcoming.Value.triggerTime)
                            {
                                nextUpcoming = (alarm, potentialTriggerTimeUTC);
                            }
                            break;
                        }
                    }
                }
            }
            // 2. 处理一次性闹钟
            else if (!string.IsNullOrEmpty(alarm.specificDate))
            {
                if (DateTime.TryParse(alarm.specificDate, out DateTime scheduledDate))
                {
                    DateTime potentialTriggerTime = new DateTime(
                        scheduledDate.Year, scheduledDate.Month, scheduledDate.Day,
                        alarm.hour, alarm.minute, 0
                    );

                    if (potentialTriggerTime > now)
                    {
                        DateTime potentialTriggerTimeUTC = potentialTriggerTime.ToUniversalTime();
                        if (nextUpcoming == null || potentialTriggerTimeUTC < nextUpcoming.Value.triggerTime)
                        {
                            nextUpcoming = (alarm, potentialTriggerTimeUTC);
                        }
                    }
                }
            }
        }

        return nextUpcoming;
    }

    /// <summary>
    /// 从JSON文件加载闹钟列表
    /// </summary>
    private void LoadAlarms()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                AlarmListData wrapper = JsonUtility.FromJson<AlarmListData>(json);
                if (wrapper != null && wrapper.Alarms != null)
                {
                    alarms = wrapper.Alarms;
                }
                else
                {
                    alarms = new List<Alarm>();
                }
                Debug.Log($"成功加载 {alarms.Count} 个闹钟。");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载文件失败: {e.Message}. 将使用空列表。");
                alarms = new List<Alarm>();
            }
        }
        else
        {
            alarms = new List<Alarm>();
        }
    }

    /// <summary>
    /// 将闹钟列表保存到JSON文件
    /// </summary>
    private void SaveAlarms()
    {
        try
        {
            AlarmListData wrapper = new AlarmListData { Alarms = alarms };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存文件失败: {e.Message}");
        }
    }
    public bool IsAlarmDuplicate(Alarm newAlarm)
    {
        foreach (var existingAlarm in alarms)
        {
            // 检查核心三要素：小时、分钟、重复设置
            if (existingAlarm.hour == newAlarm.hour &&
                existingAlarm.minute == newAlarm.minute)
            {
                // 如果两个都是一次性闹钟，则检查具体日期
                if (existingAlarm.repeatDays.Count == 0 && newAlarm.repeatDays.Count == 0)
                {
                    if (existingAlarm.specificDate == newAlarm.specificDate)
                    {
                        return true; // 日期和时间都相同，重复
                    }
                }
                // 如果两个都是周期性闹钟，则检查重复星期是否完全一样
                else if (existingAlarm.repeatDays.Count > 0 && newAlarm.repeatDays.Count > 0)
                {
                    if (existingAlarm.repeatDays.SetEquals(newAlarm.repeatDays))
                    {
                        return true; // 重复星期和时间都相同，重复
                    }
                }
            }
        }
        return false; // 没有找到任何重复项
    }
    public void AddAlarm(Alarm newAlarm)
    {
        alarms.Add(newAlarm);
        NotifyDataChanged();
    }

    public void DeleteAlarm(string id)
    {
        alarms.RemoveAll(a => a.id == id);
        NotifyDataChanged();
    }

    public List<Alarm> GetAllAlarms()
    {
        return new List<Alarm>(alarms);
    }

    public void NotifyDataChanged()
    {
        OnAlarmListChanged?.Invoke();
        SaveAlarms();
    }

    void OnApplicationQuit() => SaveAlarms();
    void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveAlarms(); }

    [Serializable]
    private class AlarmListData { public List<Alarm> Alarms; }
}