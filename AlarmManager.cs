using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq; // ����Linq������ FirstOrDefault �ȷ���

public class AlarmManager : MonoBehaviour
{
    // ����ʵ����ȷ��ȫ��Ψһ
    public static AlarmManager Instance { get; private set; }

    // �����Ŀ���ʱ������ (UTCʱ��)
    public DateTime CurrentTime { get; private set; }

    // �����¼�������֪ͨ����ϵͳ
    public event Action<Alarm> OnAlarmTriggered;
    public event Action OnAlarmListChanged;

    // ˽�б���
    private List<Alarm> alarms = new List<Alarm>();
    private string saveFileName = "alarms.json";
    private string savePath;

    void Awake()
    {
        // --- ��ʼ������ ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- ��ʼ���������� ---
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        // --- ��ʼ���浵·������������ ---
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"�������ݽ����浽: {savePath}");
        LoadAlarms();
    }

    void Start()
    {
        // ��������������Э�̣��ֱ���ʱ����º����Ӽ��
        StartCoroutine(UpdateTimePropertyRoutine()); // ��Ƶ�ʸ���ʱ�䣬��UIʹ��
        StartCoroutine(CheckAlarmsRoutine());      // ��Ƶ�ʼ�����ӣ���ʡ����
    }

    /// <summary>
    /// ���Э��ר�Ÿ���ÿ�����CurrentTime����
    /// </summary>
    private IEnumerator UpdateTimePropertyRoutine()
    {
        // (��δ���İ汾�У������������������ʱ��ͬ���߼�)
        while (true)
        {
            CurrentTime = DateTime.UtcNow; // ʹ���豸�ı�׼UTCʱ��
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// ���Э��ֻ�����������߼�����Ƶ������
    /// </summary>
    private IEnumerator CheckAlarmsRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (true)
        {
            DateTime localCurrentTime = CurrentTime.ToLocalTime();

            foreach (var alarm in alarms)
            {
                // �������޸ġ�����������µ�IsScheduledFor���������������ǵ�Ȩ��ʱ��
                if (alarm.isEnabled &&
                    alarm.hour == localCurrentTime.Hour &&
                    alarm.minute == localCurrentTime.Minute &&
                    alarm.IsScheduledFor(localCurrentTime)) // <-- �޸�������
                {
                    string triggerKey = $"triggered_{alarm.id}_{localCurrentTime:yyyyMMdd}";
                    if (PlayerPrefs.GetInt(triggerKey, 0) == 0)
                    {
                        // �ҵ���һ����ȫƥ������ӣ������¼���
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
    /// �����߼������㲢������һ��������������Ӽ�����崥��ʱ��
    /// </summary>
    public (Alarm alarm, DateTime triggerTime)? GetNextUpcomingAlarm()
    {
        (Alarm alarm, DateTime triggerTime)? nextUpcoming = null;

        // �������޸����������еļ��㶼�����û��ı���ʱ�䣬����ʱ������
        DateTime now = this.CurrentTime.ToLocalTime();

        foreach (var alarm in alarms)
        {
            if (!alarm.isEnabled) continue;

            // 1. ��������������
            if (alarm.repeatDays != null && alarm.repeatDays.Count > 0)
            {
                // �ӽ��쿪ʼ�����δ��8��
                for (int i = 0; i < 8; i++)
                {
                    DateTime checkDay = now.Date.AddDays(i);
                    if (alarm.repeatDays.Contains(checkDay.DayOfWeek))
                    {
                        DateTime potentialTriggerTime = new DateTime(
                            checkDay.Year, checkDay.Month, checkDay.Day,
                            alarm.hour, alarm.minute, 0
                        );

                        // ������Ǳ��ʱ����������
                        if (potentialTriggerTime > now)
                        {
                            // ת��ΪUTC����ͳһ�洢�ͱȽ�
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
            // 2. ����һ��������
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
    /// ��JSON�ļ����������б�
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
                Debug.Log($"�ɹ����� {alarms.Count} �����ӡ�");
            }
            catch (Exception e)
            {
                Debug.LogError($"�����ļ�ʧ��: {e.Message}. ��ʹ�ÿ��б�");
                alarms = new List<Alarm>();
            }
        }
        else
        {
            alarms = new List<Alarm>();
        }
    }

    /// <summary>
    /// �������б��浽JSON�ļ�
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
            Debug.LogError($"�����ļ�ʧ��: {e.Message}");
        }
    }
    public bool IsAlarmDuplicate(Alarm newAlarm)
    {
        foreach (var existingAlarm in alarms)
        {
            // ��������Ҫ�أ�Сʱ�����ӡ��ظ�����
            if (existingAlarm.hour == newAlarm.hour &&
                existingAlarm.minute == newAlarm.minute)
            {
                // �����������һ�������ӣ������������
                if (existingAlarm.repeatDays.Count == 0 && newAlarm.repeatDays.Count == 0)
                {
                    if (existingAlarm.specificDate == newAlarm.specificDate)
                    {
                        return true; // ���ں�ʱ�䶼��ͬ���ظ�
                    }
                }
                // ��������������������ӣ������ظ������Ƿ���ȫһ��
                else if (existingAlarm.repeatDays.Count > 0 && newAlarm.repeatDays.Count > 0)
                {
                    if (existingAlarm.repeatDays.SetEquals(newAlarm.repeatDays))
                    {
                        return true; // �ظ����ں�ʱ�䶼��ͬ���ظ�
                    }
                }
            }
        }
        return false; // û���ҵ��κ��ظ���
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