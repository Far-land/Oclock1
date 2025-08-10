using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ȫ��ʱ������� (����)
/// ���������ͬ��ʱ�䣬���ṩ�ɿ��ĵ�ǰʱ�䡣
/// ʹ���¼���������֪ͨ����ϵͳʱ��仯��
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    // �����¼����������ű�����
    public event Action OnTimeSynced; // ������ʱ��ͬ���ɹ�ʱ����
    public event Action<DateTime> OnSecondTick; // ÿ�봥��
    public event Action<DateTime> OnDateChanged; // ���ڱ仯ʱ����

    // �������ԣ���ȡ��ǰ����ʱ��
    public DateTime CurrentTime { get; private set; }
    public bool IsTimeSynced { get; private set; } = false;

    private const string ApiUrl = "https://worldtimeapi.org/api/timezone/Etc/UTC";
    private DateTime _syncedUtcTime;
    private float _timeAtSync;
    private int _lastCheckedDay = -1;

    void Awake()
    {
        // ���õ���ģʽ��ȷ��ȫ��Ψһ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // �л�����ʱ����
    }

    void Start()
    {
        // ������ʼ��ʱ����ʹ�ñ���ʱ�䣬Ȼ��������ͬ��
        CurrentTime = DateTime.UtcNow;
        StartCoroutine(SyncTimeRoutine());
    }

    // ���ļ�ʱЭ��
    IEnumerator SyncTimeRoutine()
    {
        // 1. ��ȡ����ʱ��
        yield return StartCoroutine(GetNetworkTime());

        // 2. ����ͬ���ɹ���񣬶���ʼ���ؼ�ʱѭ��
        while (true)
        {
            // ����ͬ������򱾵�ʱ�������µ�ǰʱ��
            if (IsTimeSynced)
            {
                // ������ͬ������������ʱ��
                float elapsedSeconds = Time.realtimeSinceStartup - _timeAtSync;
                CurrentTime = _syncedUtcTime.AddSeconds(elapsedSeconds);
            }
            else
            {
                // ����ͬ��ʧ�ܣ����˵��豸UTCʱ��
                CurrentTime = DateTime.UtcNow;
            }

            // ����ÿ���¼�
            OnSecondTick?.Invoke(CurrentTime);

            // ��������Ƿ�仯
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
            Debug.Log("����ͬ������ʱ��...");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    TimeResponse response = JsonUtility.FromJson<TimeResponse>(webRequest.downloadHandler.text);
                    // ʹ��Rfc3339/ISO-8601��ʽ���о�ȷ����
                    _syncedUtcTime = DateTime.Parse(response.datetime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    _timeAtSync = Time.realtimeSinceStartup;
                    IsTimeSynced = true;
                    _lastCheckedDay = _syncedUtcTime.Day;

                    Debug.Log($"����ʱ��ͬ���ɹ�: {_syncedUtcTime}");
                    OnTimeSynced?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"ʱ�����ʧ��: {e.Message}. ��ʹ�ñ���ʱ�䡣");
                    IsTimeSynced = false;
                }
            }
            else
            {
                Debug.LogWarning("��������ʧ�ܣ���ʹ�ñ���ʱ�䡣");
                IsTimeSynced = false;
            }
        }
    }

    // ����JSON�����л��ĸ�����
    [System.Serializable]
    private class TimeResponse
    {
        public string datetime;
    }
}