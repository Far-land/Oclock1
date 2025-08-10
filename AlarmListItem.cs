using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))] // ȷ�����������һ����Toggle���
public class AlarmListItem : MonoBehaviour
{
    [Header("UI ����")]
    public TMP_Text timeText;
    public TMP_Text labelText;
    public TMP_Text repeatDaysText;
    public Toggle alarmToggle; // ������ұߵ�����/���ÿ���

    // ˽�б���
    private Alarm associatedAlarm;
    private Toggle itemToggle; // ���Լ��������ϵ�Toggle���������
    private Action<Alarm> onItemSelectedForEditing;
    private bool isDuringSetup = false; // һ����ȫ��ǣ���ֹ�ڳ�ʼ��ʱ�����¼�

    void Awake()
    {
        itemToggle = GetComponent<Toggle>();
        // ����������ͬ��Toggle�¼�
        itemToggle.onValueChanged.AddListener(OnSelectionStateChanged);
        alarmToggle.onValueChanged.AddListener(OnEnableStateChanged);
    }

    /// <summary>
    /// �������޸ġ��µ�Setup���������ڿ��Խ���3������
    /// </summary>
    public void Setup(Alarm alarm, ToggleGroup toggleGroup, Action<Alarm> itemSelectedCallback)
    {
        isDuringSetup = true; // ��ʼ���ã��򿪰�ȫ���

        associatedAlarm = alarm;
        onItemSelectedForEditing = itemSelectedCallback;

        // ���Լ���Toggle������뵽��������ToggleGroup�У�ʵ�ֵ�ѡ
        itemToggle.group = toggleGroup;

        // ���UI
        timeText.text = $"{alarm.hour:00}:{alarm.minute:00}";
        labelText.text = alarm.label;
        repeatDaysText.text = FormatScheduleInfo(alarm);

        // ��������/���ÿ��ص�״̬
        alarmToggle.isOn = alarm.isEnabled;

        isDuringSetup = false; // ���ý������رհ�ȫ���
    }

    /// <summary>
    /// �������б����ѡ�С���ȡ��ѡ�С�ʱ����
    /// </summary>
    private void OnSelectionStateChanged(bool isOn)
    {
        if (isDuringSetup) return; // ������ڳ�ʼ������ִ���κβ���

        // ֻ�е�����ѡ�С�ʱ�����ǲ�ִ�в���
        if (isOn)
        {
            // ���ûص���֪ͨ���ܵ��ݡ�(AlarmListScreen)ȥ�򿪱༭ҳ��
            onItemSelectedForEditing?.Invoke(associatedAlarm);
        }
    }

    /// <summary>
    /// ���Ҳ�ġ�����/���á�����״̬�仯ʱ����
    /// </summary>
    private void OnEnableStateChanged(bool isOn)
    {
        if (isDuringSetup) return; // ������ڳ�ʼ������ִ���κβ���

        if (associatedAlarm != null)
        {
            associatedAlarm.isEnabled = isOn;
            AlarmManager.Instance.NotifyDataChanged();
        }
    }

    // FormatScheduleInfo �������ֲ���
    private string FormatScheduleInfo(Alarm alarm)
    {
        if (alarm.repeatDays != null && alarm.repeatDays.Count > 0)
        {
            if (alarm.repeatDays.Count == 7) return "ÿ��";
            var dayNames = new SortedDictionary<DayOfWeek, string> {
                {DayOfWeek.Monday, "һ"}, {DayOfWeek.Tuesday, "��"}, {DayOfWeek.Wednesday, "��"},
                {DayOfWeek.Thursday, "��"}, {DayOfWeek.Friday, "��"}, {DayOfWeek.Saturday, "��"},
                {DayOfWeek.Sunday, "��"}
            };
            StringBuilder sb = new StringBuilder("ÿ�� ");
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
                if (scheduledDate.Date == DateTime.Today.Date) return "����";
                if (scheduledDate.Date == DateTime.Today.AddDays(1).Date) return "����";
                return scheduledDate.ToString("M��d�� dddd");
            }
        }
        return "��һ��";
    }
}