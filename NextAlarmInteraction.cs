using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class NextAlarmInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("UI ����")]
    public TextMeshProUGUI nextAlarmText;

    // �Զ���˫��������
    //private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f; // ˫�������ֵ���룩

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
            nextAlarmText.text = "��δ��������";
        }
        else
        {
            DateTime triggerTime = nextAlarmInfo.Value.triggerTime.ToLocalTime();
            DateTime today = AlarmManager.Instance.CurrentTime.ToLocalTime().Date;
            DateTime tomorrow = today.AddDays(1);

            string prefix = "";
            if (triggerTime.Date == today) prefix = "����";
            else if (triggerTime.Date == tomorrow) prefix = "����";
            else prefix = triggerTime.ToString("M��d��");

            string timeString;
            if (UserManager.Instance.CurrentUser.use24HourFormat)
            {
                timeString = triggerTime.ToString("HH:mm");
            }
            else
            {
                timeString = triggerTime.ToString("hh:mm tt");
            }

            nextAlarmText.text = $"��һ������: {prefix} {timeString}";
        }
    }
}