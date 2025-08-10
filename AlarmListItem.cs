using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))] // 确保这个对象上一定有Toggle组件
public class AlarmListItem : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text timeText;
    public TMP_Text labelText;
    public TMP_Text repeatDaysText;
    public Toggle alarmToggle; // 这个是右边的启用/禁用开关

    // 私有变量
    private Alarm associatedAlarm;
    private Toggle itemToggle; // 对自己根对象上的Toggle组件的引用
    private Action<Alarm> onItemSelectedForEditing;
    private bool isDuringSetup = false; // 一个安全标记，防止在初始化时触发事件

    void Awake()
    {
        itemToggle = GetComponent<Toggle>();
        // 监听两个不同的Toggle事件
        itemToggle.onValueChanged.AddListener(OnSelectionStateChanged);
        alarmToggle.onValueChanged.AddListener(OnEnableStateChanged);
    }

    /// <summary>
    /// 【核心修改】新的Setup方法，现在可以接收3个参数
    /// </summary>
    public void Setup(Alarm alarm, ToggleGroup toggleGroup, Action<Alarm> itemSelectedCallback)
    {
        isDuringSetup = true; // 开始设置，打开安全标记

        associatedAlarm = alarm;
        onItemSelectedForEditing = itemSelectedCallback;

        // 将自己的Toggle组件加入到父容器的ToggleGroup中，实现单选
        itemToggle.group = toggleGroup;

        // 填充UI
        timeText.text = $"{alarm.hour:00}:{alarm.minute:00}";
        labelText.text = alarm.label;
        repeatDaysText.text = FormatScheduleInfo(alarm);

        // 设置启用/禁用开关的状态
        alarmToggle.isOn = alarm.isEnabled;

        isDuringSetup = false; // 设置结束，关闭安全标记
    }

    /// <summary>
    /// 当整个列表项被“选中”或“取消选中”时调用
    /// </summary>
    private void OnSelectionStateChanged(bool isOn)
    {
        if (isDuringSetup) return; // 如果正在初始化，则不执行任何操作

        // 只有当“被选中”时，我们才执行操作
        if (isOn)
        {
            // 调用回调，通知“总导演”(AlarmListScreen)去打开编辑页面
            onItemSelectedForEditing?.Invoke(associatedAlarm);
        }
    }

    /// <summary>
    /// 当右侧的“启用/禁用”开关状态变化时调用
    /// </summary>
    private void OnEnableStateChanged(bool isOn)
    {
        if (isDuringSetup) return; // 如果正在初始化，则不执行任何操作

        if (associatedAlarm != null)
        {
            associatedAlarm.isEnabled = isOn;
            AlarmManager.Instance.NotifyDataChanged();
        }
    }

    // FormatScheduleInfo 方法保持不变
    private string FormatScheduleInfo(Alarm alarm)
    {
        if (alarm.repeatDays != null && alarm.repeatDays.Count > 0)
        {
            if (alarm.repeatDays.Count == 7) return "每天";
            var dayNames = new SortedDictionary<DayOfWeek, string> {
                {DayOfWeek.Monday, "一"}, {DayOfWeek.Tuesday, "二"}, {DayOfWeek.Wednesday, "三"},
                {DayOfWeek.Thursday, "四"}, {DayOfWeek.Friday, "五"}, {DayOfWeek.Saturday, "六"},
                {DayOfWeek.Sunday, "日"}
            };
            StringBuilder sb = new StringBuilder("每周 ");
            foreach (var day in dayNames.Keys)
            {
                if (alarm.repeatDays.Contains(day)) sb.Append(dayNames[day] + " ");
            }
            return sb.ToString().TrimEnd();
        }
        if (!string.IsNullOrEmpty(alarm.specificDate))
        {
            if (DateTime.TryParse(alarm.specificDate, out DateTime scheduledDate))
            {
                if (scheduledDate.Date == DateTime.Today.Date) return "今天";
                if (scheduledDate.Date == DateTime.Today.AddDays(1).Date) return "明天";
                return scheduledDate.ToString("M月d日 dddd");
            }
        }
        return "仅一次";
    }
}