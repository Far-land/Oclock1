using System;
using System.Collections.Generic;
using UnityEngine;
public enum RingtoneMode { System, Custom } // 在文件顶部添加这个枚举

[Serializable] // 【修改点】将 [Serializable] 移到类的上方
public class Alarm : ISerializationCallbackReceiver
{
    public string id;
    public string label;
    public int hour;
    public int minute;
    public bool isEnabled;
    public string specificDate;

    public RingtoneMode ringtoneMode; // 【新增】
    public string ringtoneName; // 这个现在只在Custom模式下使用

    // 这个字段在运行时使用，不直接序列化
    [NonSerialized] // 【修改点】明确告诉Unity不要序列化这个字段
    public HashSet<DayOfWeek> repeatDays;

    // 这个字段专门用于JSON序列化
    [SerializeField]
    private List<int> repeatDaysData;

    // 构造函数
    public Alarm(int hour, int minute, string label = "闹钟", bool isEnabled = true)
    {
        this.id = Guid.NewGuid().ToString();
        this.hour = hour;
        this.minute = minute;
        this.label = label;
        this.isEnabled = isEnabled;
        this.repeatDays = new HashSet<DayOfWeek>(); // 确保新创建的实例有初始化的集合
        this.specificDate = DateTime.Today.ToString("yyyy-MM-dd");
        this.ringtoneMode = RingtoneMode.System; // 默认使用系统音乐
        this.ringtoneName = "默认";
    }
    public DateTime GetTriggerTime()
    {
        // 尝试从specificDate解析出基础日期
        if (DateTime.TryParse(specificDate, out DateTime baseDate))
        {
            // 将小时和分钟信息组合进去
            return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, this.hour, this.minute, 0);
        }

        // 如果解析失败，返回一个默认的过去时间，避免出错
        return DateTime.MinValue;
    }
    /// <summary>
    /// 在序列化（保存）之前调用
    /// </summary>
    public void OnBeforeSerialize()
    {
        // 总是初始化列表，以防万一
        if (repeatDaysData == null)
        {
            repeatDaysData = new List<int>();
        }
        repeatDaysData.Clear(); // 先清空

        if (repeatDays != null && repeatDays.Count > 0)
        {
            foreach (var day in repeatDays)
            {
                repeatDaysData.Add((int)day);
            }
        }
    }

    /// <summary>
    /// 在反序列化（加载）之后调用
    /// </summary>
    public void OnAfterDeserialize()
    {
        // 【核心修复】总是初始化HashSet
        if (repeatDays == null)
        {
            repeatDays = new HashSet<DayOfWeek>();
        }
        repeatDays.Clear(); // 先清空

        if (repeatDaysData != null && repeatDaysData.Count > 0)
        {
            foreach (var dayInt in repeatDaysData)
            {
                repeatDays.Add((DayOfWeek)dayInt);
            }
        }
    }

    // IsScheduledForToday 方法保持不变
    public bool IsScheduledFor(DateTime currentTime)
    {
        // 1. 如果是周期性闹钟
        if (repeatDays != null && repeatDays.Count > 0)
        {
            return repeatDays.Contains(currentTime.DayOfWeek);
        }

        // 2. 如果是一次性闹钟
        if (!string.IsNullOrEmpty(specificDate))
        {
            if (DateTime.TryParse(specificDate, out DateTime scheduledDate))
            {
                // 仅当计划日期与权威时间的日期完全相同时，才认为匹配
                return scheduledDate.Date == currentTime.Date;
            }
        }

        return false;
    }
}