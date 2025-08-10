using System;
using System.Collections.Generic;
using UnityEngine;
public enum RingtoneMode { System, Custom } // ���ļ�����������ö��

[Serializable] // ���޸ĵ㡿�� [Serializable] �Ƶ�����Ϸ�
public class Alarm : ISerializationCallbackReceiver
{
    public string id;
    public string label;
    public int hour;
    public int minute;
    public bool isEnabled;
    public string specificDate;

    public RingtoneMode ringtoneMode; // ��������
    public string ringtoneName; // �������ֻ��Customģʽ��ʹ��

    // ����ֶ�������ʱʹ�ã���ֱ�����л�
    [NonSerialized] // ���޸ĵ㡿��ȷ����Unity��Ҫ���л�����ֶ�
    public HashSet<DayOfWeek> repeatDays;

    // ����ֶ�ר������JSON���л�
    [SerializeField]
    private List<int> repeatDaysData;

    // ���캯��
    public Alarm(int hour, int minute, string label = "����", bool isEnabled = true)
    {
        this.id = Guid.NewGuid().ToString();
        this.hour = hour;
        this.minute = minute;
        this.label = label;
        this.isEnabled = isEnabled;
        this.repeatDays = new HashSet<DayOfWeek>(); // ȷ���´�����ʵ���г�ʼ���ļ���
        this.specificDate = DateTime.Today.ToString("yyyy-MM-dd");
        this.ringtoneMode = RingtoneMode.System; // Ĭ��ʹ��ϵͳ����
        this.ringtoneName = "Ĭ��";
    }
    public DateTime GetTriggerTime()
    {
        // ���Դ�specificDate��������������
        if (DateTime.TryParse(specificDate, out DateTime baseDate))
        {
            // ��Сʱ�ͷ�����Ϣ��Ͻ�ȥ
            return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, this.hour, this.minute, 0);
        }

        // �������ʧ�ܣ�����һ��Ĭ�ϵĹ�ȥʱ�䣬�������
        return DateTime.MinValue;
    }
    /// <summary>
    /// �����л������棩֮ǰ����
    /// </summary>
    public void OnBeforeSerialize()
    {
        // ���ǳ�ʼ���б��Է���һ
        if (repeatDaysData == null)
        {
            repeatDaysData = new List<int>();
        }
        repeatDaysData.Clear(); // �����

        if (repeatDays != null && repeatDays.Count > 0)
        {
            foreach (var day in repeatDays)
            {
                repeatDaysData.Add((int)day);
            }
        }
    }

    /// <summary>
    /// �ڷ����л������أ�֮�����
    /// </summary>
    public void OnAfterDeserialize()
    {
        // �������޸������ǳ�ʼ��HashSet
        if (repeatDays == null)
        {
            repeatDays = new HashSet<DayOfWeek>();
        }
        repeatDays.Clear(); // �����

        if (repeatDaysData != null && repeatDaysData.Count > 0)
        {
            foreach (var dayInt in repeatDaysData)
            {
                repeatDays.Add((DayOfWeek)dayInt);
            }
        }
    }

    // IsScheduledForToday �������ֲ���
    public bool IsScheduledFor(DateTime currentTime)
    {
        // 1. ���������������
        if (repeatDays != null && repeatDays.Count > 0)
        {
            return repeatDays.Contains(currentTime.DayOfWeek);
        }

        // 2. �����һ��������
        if (!string.IsNullOrEmpty(specificDate))
        {
            if (DateTime.TryParse(specificDate, out DateTime scheduledDate))
            {
                // �����ƻ�������Ȩ��ʱ���������ȫ��ͬʱ������Ϊƥ��
                return scheduledDate.Date == currentTime.Date;
            }
        }

        return false;
    }
}