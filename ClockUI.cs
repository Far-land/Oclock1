using UnityEngine;
using TMPro;
using System;

/// <summary>
/// 负责更新UI时钟显示。
/// 订阅TimeManager的事件，实现高效更新。
/// </summary>
public class ClockUI : MonoBehaviour
{
    public TextMeshProUGUI clockText;
    public bool convertToLocalTime = true; // 是否将UTC时间转换为本地时区显示

    void Start()
    {
        if (clockText == null)
        {
            clockText = GetComponent<TextMeshProUGUI>();
        }

        // 检查TimeManager是否存在
        if (TimeManager.Instance == null)
        {
            Debug.LogError("场景中找不到 TimeManager!");
            return;
        }

        // 订阅每秒更新事件
        TimeManager.Instance.OnSecondTick += UpdateClockText;
    }

    void OnDestroy()
    {
        // 在对象销毁时取消订阅，防止内存泄漏
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnSecondTick -= UpdateClockText;
        }
    }

    private void UpdateClockText(DateTime currentTimeUtc)
    {
        DateTime displayTime = convertToLocalTime ? currentTimeUtc.ToLocalTime() : currentTimeUtc;
        clockText.text = $"{displayTime:yyyy/MM/dd (HH:mm:ss)}";
    }
}