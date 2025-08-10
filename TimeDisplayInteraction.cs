using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 引入Image组件

public class TimeDisplayInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("UI 引用")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI dayOfWeekText;

    [Header("长按设置")]
    public float longPressDuration = 0.8f;

    // 内部状态变量
    private bool isPointerDown = false;
    private bool isLongPress = false;
    private Coroutine longPressCoroutine;
    private DateTime lastDateDisplayed;

    // 【已移除】不再需要本地变量来存储时间格式
    // private bool use24HourFormat = true;
    // private const string TimeFormatKey = "TimeFormat24H";

    // 【新增】12小时制上午/下午图标
    public GameObject amIcon;
    public GameObject pmIcon;

    private bool colonVisible = true; // 控制冒号闪烁状态

    void Start()
    {
        // 【已修改】不再需要从PlayerPrefs加载，因为我们会实时从UserManager获取
        StartCoroutine(UpdateTimeRoutine());
        if (amIcon != null) amIcon.SetActive(false);
        if (pmIcon != null) pmIcon.SetActive(false);
    }

    // --- 交互逻辑 ---

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        longPressCoroutine = StartCoroutine(LongPressCheckRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLongPress)
        {
            isLongPress = false;
            return;
        }
        HandleShortClick();
    }

    private IEnumerator LongPressCheckRoutine()
    {
        yield return new WaitForSeconds(longPressDuration);
        if (isPointerDown)
        {
            isLongPress = true;
            HandleLongPress();
        }
    }

    /// <summary>
    /// 【核心修改】短按切换功能已被移除
    /// </summary>
    private void HandleShortClick()
    {
        // 这个方法现在什么也不做，或者您可以放一个音效
        Debug.Log("时间文本被点击，当前无预设功能。");
    }

    // 长按功能依然是为未来的主题设置占位
    private void HandleLongPress()
    {
        Debug.Log("长按“时间”面板！准备打开主题设置界面... (功能待实现)");
        // 例如: ThemeManager.Instance.OpenThemePanel();
    }


    // --- 时间和日期显示逻辑 ---

    private IEnumerator UpdateTimeRoutine()
    {
        while (AlarmManager.Instance == null || UserManager.Instance == null)
        {
            // 等待所有管理器都准备就绪
            yield return null;
        }

        lastDateDisplayed = DateTime.MinValue;

        while (true)
        {
            UpdateTimeDisplay();

            DateTime trustedLocalTime = AlarmManager.Instance.CurrentTime.ToLocalTime();
            if (trustedLocalTime.Date != lastDateDisplayed)
            {
                UpdateDateDisplay();
                UpdateDayOfWeekDisplay();
                lastDateDisplayed = trustedLocalTime.Date;
            }

            yield return new WaitForSeconds(1f);
            colonVisible = !colonVisible;

        }
    }

    /// <summary>
    /// 【核心修改】更新时间显示时，直接从UserManager读取设置
    /// </summary>
    private void UpdateTimeDisplay()
    {
        // 增加对UserManager的检查
        if (timeText == null || AlarmManager.Instance == null || UserManager.Instance == null) return;

        DateTime trustedLocalTime = AlarmManager.Instance.CurrentTime.ToLocalTime();
        string timeFormat;

        // 直接使用 UserManager 中存储的用户偏好来判断
        if (UserManager.Instance.CurrentUser.use24HourFormat)
        {
            timeFormat = "HH" + (colonVisible ? ":" : " ") + "mm"; // 不显示秒，冒号闪烁
                                                                   // 隐藏12小时制图标
            if (amIcon != null) amIcon.SetActive(false);
            if (pmIcon != null) pmIcon.SetActive(false);
        }
        else
        {
            timeFormat = "hh" + (colonVisible ? ":" : " ") + "mm"; // 不显示秒，冒号闪烁
                                                                   // 显示对应的AM/PM图标
            if (trustedLocalTime.Hour < 12)
            {
                if (amIcon != null) amIcon.SetActive(true);
                if (pmIcon != null) pmIcon.SetActive(false);
            }
            else
            {
                if (amIcon != null) amIcon.SetActive(false);
                if (pmIcon != null) pmIcon.SetActive(true);
            }
        }
        timeText.text = trustedLocalTime.ToString(timeFormat);
    }

    // 以下方法保持不变
    private void UpdateDateDisplay()
    {
        if (dateText == null || AlarmManager.Instance == null) return;
        DateTime trustedLocalTime = AlarmManager.Instance.CurrentTime.ToLocalTime();
        dateText.text = trustedLocalTime.ToString("D");
    }

    private void UpdateDayOfWeekDisplay()
    {
        if (dayOfWeekText == null || AlarmManager.Instance == null) return;
        DateTime trustedLocalTime = AlarmManager.Instance.CurrentTime.ToLocalTime();
        dayOfWeekText.text = trustedLocalTime.ToString("dddd");
    }
}