using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AlarmRingingController : MonoBehaviour
{
    // ����ȫ��Ψһ�ľ�̬ʵ��������ģʽ��
    public static AlarmRingingController Instance { get; private set; }

    [Header("���Ĺ���������")]
    public MusicManager musicManager; // ��Ҫ��Inspector������MusicManagerʵ��

    // ˽�б��������ڴ洢��MainScene���Ǽǡ�������UI���
    private GameObject _alarmRingingPanel;
    private TextMeshProUGUI _alarmLabelText;
    private TextMeshProUGUI _currentTimeText;
    private Button _dismissButton;
    private Button _snoozeButton;
    private AudioSource _audioSource;

    private Alarm currentRingingAlarm;

    void Awake()
    {
        // ���õ���
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // ����ű����ڵ�AppManagers�����Ѿ�������ΪDontDestroyOnLoad

        // �������Ӵ����¼�
        // ���ǰѶ����Ƶ�Awake��ȷ�����ڵ�һʱ���׼���ü���
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered += HandleAlarmTrigger;
        }
    }

    void OnDestroy()
    {
        // ���Լ�������ʱȡ�����ģ���ֹ�ڴ�й©
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered -= HandleAlarmTrigger;
        }
    }

    /// <summary>
    /// �����ɡ���������Ա��(MainSceneController)���õġ��Ǽǡ�����
    /// </summary>
    public void RegisterRingingPanel(GameObject panel, TextMeshProUGUI label, TextMeshProUGUI time, Button dismiss, Button snooze)
    {
        _alarmRingingPanel = panel;
        _alarmLabelText = label;
        _currentTimeText = time;
        _dismissButton = dismiss;
        _snoozeButton = snooze;

        if (_alarmRingingPanel != null)
        {
            // ��ȡ����ϵ�AudioSource���󶨰�ť�¼�
            _audioSource = _alarmRingingPanel.GetComponent<AudioSource>();
            _dismissButton.onClick.AddListener(Dismiss);
            _snoozeButton.onClick.AddListener(Snooze);
            _alarmRingingPanel.SetActive(false); // ȷ���ǼǺ��ʼ�����ص�
        }
        Debug.Log("����UI����ѳɹ��Ǽǵ�AlarmRingingController��");
    }

    /// <summary>
    /// �������¼�������ʱ��ִ�д˷���
    /// </summary>
    private void HandleAlarmTrigger(Alarm triggeredAlarm)
    {
        if (_alarmRingingPanel == null)
        {
            Debug.LogError("���Ӵ�����������UI�����δ�Ǽǣ�����MainSceneController�����á�");
            return;
        }

        currentRingingAlarm = triggeredAlarm;

        _alarmRingingPanel.SetActive(true);
        _alarmLabelText.text = triggeredAlarm.label;

        PlayRingtone();
    }

    private void PlayRingtone()
    {
        if (MusicManager.Instance == null || LocationWeatherManager.Instance == null)
        {
            Debug.LogError("MusicManager �� LocationWeatherManager δ׼��������");
            return;
        }

        var weatherData = LocationWeatherManager.Instance.LoadedWeatherData;
        AudioClip clipToPlay = MusicManager.Instance.GetContextualRingtone(currentRingingAlarm.GetTriggerTime(), weatherData);

        if (clipToPlay != null && _audioSource != null)
        {
            _audioSource.clip = clipToPlay;
            _audioSource.Play();
        }
    }

    void Update()
    {
        // ����弤��ʱ������ʱ����ʾ
        if (_alarmRingingPanel != null && _alarmRingingPanel.activeSelf && _currentTimeText != null)
        {
            _currentTimeText.text = DateTime.Now.ToString("HH:mm");
        }
    }

    private void Dismiss()
    {
        if (_audioSource != null) _audioSource.Stop();
        if (_alarmRingingPanel != null) _alarmRingingPanel.SetActive(false);
    }

    private void Snooze()
    {
        if (_audioSource != null) _audioSource.Stop();
        if (_alarmRingingPanel != null) _alarmRingingPanel.SetActive(false);

        // �����Ժ��������ӵ��߼�...
        if (currentRingingAlarm != null)
        {
            DateTime snoozeTime = AlarmManager.Instance.CurrentTime.AddMinutes(9);
            Alarm snoozeAlarm = new Alarm(snoozeTime.Hour, snoozeTime.Minute, currentRingingAlarm.label + " (�Ժ�����)");
            snoozeAlarm.specificDate = snoozeTime.ToString("yyyy-MM-dd");
            AlarmManager.Instance.AddAlarm(snoozeAlarm);
        }
    }
}
