using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AlarmRingingController : MonoBehaviour
{
    // 创建全局唯一的静态实例（单例模式）
    public static AlarmRingingController Instance { get; private set; }

    [Header("核心管理器引用")]
    public MusicManager musicManager; // 需要在Inspector中拖入MusicManager实例

    // 私有变量，用于存储从MainScene“登记”过来的UI组件
    private GameObject _alarmRingingPanel;
    private TextMeshProUGUI _alarmLabelText;
    private TextMeshProUGUI _currentTimeText;
    private Button _dismissButton;
    private Button _snoozeButton;
    private AudioSource _audioSource;

    private Alarm currentRingingAlarm;

    void Awake()
    {
        // 设置单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 这个脚本所在的AppManagers对象已经被设置为DontDestroyOnLoad

        // 订阅闹钟触发事件
        // 我们把订阅移到Awake，确保它在第一时间就准备好监听
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered += HandleAlarmTrigger;
        }
    }

    void OnDestroy()
    {
        // 在自己被销毁时取消订阅，防止内存泄漏
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered -= HandleAlarmTrigger;
        }
    }

    /// <summary>
    /// 这是由“场景联络员”(MainSceneController)调用的“登记”方法
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
            // 获取面板上的AudioSource并绑定按钮事件
            _audioSource = _alarmRingingPanel.GetComponent<AudioSource>();
            _dismissButton.onClick.AddListener(Dismiss);
            _snoozeButton.onClick.AddListener(Snooze);
            _alarmRingingPanel.SetActive(false); // 确保登记后初始是隐藏的
        }
        Debug.Log("响铃UI面板已成功登记到AlarmRingingController。");
    }

    /// <summary>
    /// 当闹钟事件被触发时，执行此方法
    /// </summary>
    private void HandleAlarmTrigger(Alarm triggeredAlarm)
    {
        if (_alarmRingingPanel == null)
        {
            Debug.LogError("闹钟触发，但响铃UI面板尚未登记！请检查MainSceneController的配置。");
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
            Debug.LogError("MusicManager 或 LocationWeatherManager 未准备就绪！");
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
        // 在面板激活时，更新时间显示
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

        // 创建稍后提醒闹钟的逻辑...
        if (currentRingingAlarm != null)
        {
            DateTime snoozeTime = AlarmManager.Instance.CurrentTime.AddMinutes(9);
            Alarm snoozeAlarm = new Alarm(snoozeTime.Hour, snoozeTime.Minute, currentRingingAlarm.label + " (稍后提醒)");
            snoozeAlarm.specificDate = snoozeTime.ToString("yyyy-MM-dd");
            AlarmManager.Instance.AddAlarm(snoozeAlarm);
        }
    }
}
