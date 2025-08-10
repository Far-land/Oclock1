using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

// 确保这个对象上有Animator和Canvas Group组件
[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class AlarmRingingPanel : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI alarmLabelText;
    public TextMeshProUGUI currentTimeText;
    public Button dismissButton;
    public Button snoozeButton;

    private AudioSource audioSource;
    private Alarm currentRingingAlarm;
    private Animator animator;

    void Awake()
    {
        // 获取自己身上的组件
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // 绑定自己内部按钮的事件
        dismissButton.onClick.AddListener(OnDismissClicked);
        snoozeButton.onClick.AddListener(OnSnoozeClicked);
    }

    /// <summary>
    /// 【核心修复】这是由UIManager调用的公开方法，用于显示和设置面板
    /// </summary>
    public void Show(Alarm alarm)
    {
        currentRingingAlarm = alarm;

        // 更新UI文本
        if (alarmLabelText != null)
        {
            alarmLabelText.text = currentRingingAlarm.label;
        }

        // 激活对象并播放“Show”动画
        gameObject.SetActive(true);
        animator.SetTrigger("Show");

        // 播放铃声
        PlayRingtone();
    }

    /// <summary>
    /// 一个私有的方法，用于触发关闭动画
    /// </summary>
    private void Hide()
    {
        if (audioSource != null) audioSource.Stop();
        animator.SetTrigger("Hide");
    }

    // 在Hide动画的最后一帧，添加事件来调用这个方法
    public void OnHideAnimationComplete()
    {
        gameObject.SetActive(false);
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

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("在响铃面板中找不到对应的铃声。");
        }
    }

    void Update()
    {
        // 只需要在自己可见时更新时间
        if (currentTimeText != null)
        {
            currentTimeText.text = DateTime.Now.ToString("HH:mm");
        }
    }

    // --- 按钮响应 ---
    private void OnDismissClicked()
    {
        Hide();
    }

    private void OnSnoozeClicked()
    {
        if (currentRingingAlarm != null)
        {
            DateTime snoozeTime = AlarmManager.Instance.CurrentTime.AddMinutes(9);
            Alarm snoozeAlarm = new Alarm(snoozeTime.Hour, snoozeTime.Minute, currentRingingAlarm.label + " (稍后提醒)");
            snoozeAlarm.specificDate = snoozeTime.ToString("yyyy-MM-dd");
            // 稍后提醒也使用情境音乐
            // snoozeAlarm.ringtoneName = currentRingingAlarm.ringtoneName; 

            AlarmManager.Instance.AddAlarm(snoozeAlarm);
        }

        Hide();
    }
}