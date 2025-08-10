using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class AlarmSetupScreen : MonoBehaviour
{
    [Header("主要UI引用")]
    public TMP_Text hourText;
    public TMP_Text minuteText;
    public Button hourUpButton;
    public Button hourDownButton;
    public Button minuteUpButton;
    public Button minuteDownButton;
    public TMP_InputField labelInputField;
    public DayToggleButton[] dayToggles;
    public Button confirmButton;
    public Button cancelButton;
    public Button deleteButton;
    public TMP_Text dateDisplayText;
    public Button prevDayButton;
    public Button nextDayButton;

    // 内部状态变量
    private int currentHour;
    private int currentMinute;
    private DateTime currentSelectedDate;
    private Alarm currentEditingAlarm;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        hourUpButton.onClick.AddListener(() => UpdateHour(1));
        hourDownButton.onClick.AddListener(() => UpdateHour(-1));
        minuteUpButton.onClick.AddListener(() => UpdateMinute(1));
        minuteDownButton.onClick.AddListener(() => UpdateMinute(-1));
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(Hide);
        deleteButton.onClick.AddListener(OnDelete);
        prevDayButton.onClick.AddListener(() => UpdateDate(-1));
        nextDayButton.onClick.AddListener(() => UpdateDate(1));
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 【核心修复】Show方法现在正确地接收一个Alarm类型的参数
    /// </summary>
    public void Show(Alarm alarmToEdit)
    {
        StartCoroutine(ShowRoutine(alarmToEdit));
    }

    private IEnumerator ShowRoutine(Alarm alarmToEdit)
    {
        gameObject.SetActive(true);
        yield return null;
        animator.Play("SetupPanel_Show");

        currentEditingAlarm = alarmToEdit;
        if (currentEditingAlarm == null)
        {
            ResetToNewAlarmState();
        }
        else // 编辑模式
        {
            currentHour = currentEditingAlarm.hour;
            currentMinute = currentEditingAlarm.minute;
            labelInputField.text = currentEditingAlarm.label;
            if (!DateTime.TryParse(currentEditingAlarm.specificDate, out currentSelectedDate))
            {
                currentSelectedDate = DateTime.Today;
            }
            foreach (var toggle in dayToggles)
            {
                toggle.IsSelected = currentEditingAlarm.repeatDays.Contains(toggle.day);
            }
            deleteButton.gameObject.SetActive(true);
        }
        UpdateVisuals();
    }

    public void Hide()
    {
        animator.Play("SetupPanel_Hide");
    }

    public void OnHideAnimationComplete()
    {
        gameObject.SetActive(false);
    }

    private void ResetToNewAlarmState()
    {
        DateTime now = DateTime.Now;
        currentHour = now.Hour;
        currentMinute = now.Minute;
        currentSelectedDate = DateTime.Today;
        labelInputField.text = "";
        foreach (var toggle in dayToggles) { toggle.IsSelected = false; }
        deleteButton.gameObject.SetActive(false);
    }

    private void OnConfirm()
    {
        var alarmForCheck = new Alarm(currentHour, currentMinute, labelInputField.text);
        alarmForCheck.specificDate = currentSelectedDate.ToString("yyyy-MM-dd");
        foreach (var toggle in dayToggles)
        {
            if (toggle.IsSelected) alarmForCheck.repeatDays.Add(toggle.day);
        }

        bool isDuplicate = false;
        if (currentEditingAlarm != null)
        {
            currentEditingAlarm.isEnabled = false;
            if (AlarmManager.Instance.IsAlarmDuplicate(alarmForCheck))
            {
                isDuplicate = true;
            }
            currentEditingAlarm.isEnabled = true;
        }
        else
        {
            if (AlarmManager.Instance.IsAlarmDuplicate(alarmForCheck))
            {
                isDuplicate = true;
            }
        }

        if (isDuplicate)
        {
            Debug.LogWarning("发现重复的闹钟，已阻止添加/保存！");
            return;
        }

        if (currentEditingAlarm == null) // 新建
        {
            AlarmManager.Instance.AddAlarm(alarmForCheck);
            ResetToNewAlarmState();
            UpdateVisuals();
        }
        else // 更新
        {
            currentEditingAlarm.hour = currentHour;
            currentEditingAlarm.minute = currentMinute;
            currentEditingAlarm.label = labelInputField.text;
            currentEditingAlarm.specificDate = currentSelectedDate.ToString("yyyy-MM-dd");
            currentEditingAlarm.repeatDays.Clear();
            foreach (var toggle in dayToggles) { if (toggle.IsSelected) currentEditingAlarm.repeatDays.Add(toggle.day); }
            currentEditingAlarm.ringtoneMode = RingtoneMode.System;

            AlarmManager.Instance.NotifyDataChanged();
            Hide();
        }
    }

    private void OnCancel() { Hide(); }

    private void OnDelete()
    {
        if (currentEditingAlarm != null)
        {
            AlarmManager.Instance.DeleteAlarm(currentEditingAlarm.id);
            currentEditingAlarm = null;
        }
        ResetToNewAlarmState();
        UpdateVisuals();
    }

    private void UpdateHour(int amount) { currentHour = (currentHour + amount + 24) % 24; UpdateVisuals(); }
    private void UpdateMinute(int amount) { currentMinute = (currentMinute + amount + 60) % 60; UpdateVisuals(); }
    private void UpdateDate(int daysToAdd) { currentSelectedDate = currentSelectedDate.AddDays(daysToAdd); UpdateVisuals(); }
    private void UpdateVisuals()
    {
        hourText.text = currentHour.ToString("00");
        minuteText.text = currentMinute.ToString("00");
        dateDisplayText.text = currentSelectedDate.ToString("M月d日 dddd");
    }
}