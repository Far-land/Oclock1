using UnityEngine;

public class AlarmReaction : MonoBehaviour
{
    public GameObject alarmPopupUI; // 指向你的闹钟弹窗UI面板
    public AudioSource alarmAudioSource; // 用于播放闹钟声音的AudioSource组件

    void Start()
    {
        // 订阅AlarmManager的事件
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered += HandleAlarmTrigger;
        }

        if (alarmPopupUI != null)
        {
            alarmPopupUI.SetActive(false); // 确保弹窗一开始是隐藏的
        }
    }

    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered -= HandleAlarmTrigger;
        }
    }

    private void HandleAlarmTrigger(Alarm triggeredAlarm)
    {
        Debug.Log($"正在响应闹钟: {triggeredAlarm.label}");

        // 1. 显示UI弹窗
        if (alarmPopupUI != null)
        {
            // 你可以在这里把闹钟信息传递给UI
            // var popupController = alarmPopupUI.GetComponent<AlarmPopupController>();
            // popupController.Setup(triggeredAlarm);
            alarmPopupUI.SetActive(true);
        }

        // 2. 播放声音
        if (alarmAudioSource != null && alarmAudioSource.clip != null)
        {
            alarmAudioSource.Play();
        }
    }
}