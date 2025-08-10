using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class NextAlarmInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 引用")]
    public TextMeshProUGUI nextAlarmText;

    // 自定义双击检测变量
    //private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f; // 双击间隔阈值（秒）

    void Start()
    {
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmListChanged += UpdateNextAlarmDisplay;
        }
        UpdateNextAlarmDisplay();
    }

    void OnDestroy()
    {
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmListChanged -= UpdateNextAlarmDisplay;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("--- 1. Click Detected ---");
        if (eventData.clickCount == 2)
        {
            Debug.Log("--- 2. Double-click Condition MET! ---");
            if (UIManager.Instance != null)
            {
                Debug.Log("--- 3. UIManager.Instance is VALID. Calling ShowAlarmSetupScreen... ---");
                UIManager.Instance.ShowAlarmSetupScreen(null);
            }
            else
            {
                Debug.LogError("--- ERROR! UIManager.Instance was NULL here! ---");
            }
        }
    }

    private void UpdateNextAlarmDisplay()
    {
        if (nextAlarmText == null || AlarmManager.Instance == null || UserManager.Instance == null) return;

        var nextAlarmInfo = AlarmManager.Instance.GetNextUpcomingAlarm();

        if (nextAlarmInfo == null)
        {
            nextAlarmText.text = "暂未设置闹钟";
        }
        else
        {
            DateTime triggerTime = nextAlarmInfo.Value.triggerTime.ToLocalTime();
            DateTime today = AlarmManager.Instance.CurrentTime.ToLocalTime().Date;
            DateTime tomorrow = today.AddDays(1);

            string prefix = "";
            if (triggerTime.Date == today) prefix = "今天";
            else if (triggerTime.Date == tomorrow) prefix = "明天";
            else prefix = triggerTime.ToString("M月d日");

            string timeString;
            if (UserManager.Instance.CurrentUser.use24HourFormat)
            {
                timeString = triggerTime.ToString("HH:mm");
            }
            else
            {
                timeString = triggerTime.ToString("hh:mm tt");
            }

            nextAlarmText.text = $"下一个闹钟: {prefix} {timeString}";
        }
    }
}