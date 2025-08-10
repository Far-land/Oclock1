using UnityEngine;
using System.Collections;

/// <summary>
/// UI总管 (单例)。
/// 整个应用中唯一的UI调度中心，负责所有主UI面板的显示和隐藏。
/// </summary>
public class UIManager : MonoBehaviour
{
    // 创建一个全局唯一的静态实例，方便其他任何脚本通过 UIManager.Instance 来访问它
    public static UIManager Instance { get; private set; }

    [Header("UI面板脚本的引用")]
    // 在Inspector中，将AlarmSetupPanel对象拖拽到这里
    public AlarmSetupScreen alarmSetupScreen;

    // 在Inspector中，将AlarmRingingPanel对象拖拽到这里
    public AlarmRingingPanel alarmRingingPanel;

    void Awake()
    {
        // 设置单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 为确保安全，在启动时强制隐藏所有受控面板
        if (alarmSetupScreen != null)
        {
            alarmSetupScreen.gameObject.SetActive(false);
        }
        if (alarmRingingPanel != null)
        {
            alarmRingingPanel.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // UI总管自己来监听“闹钟触发”事件
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered += OnAlarmTriggered;
        }
    }

    void OnDestroy()
    {
        // 在自己被销毁时，取消事件订阅，防止内存泄漏
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered -= OnAlarmTriggered;
        }
    }

    /// <summary>
    /// 当从AlarmManager接收到“闹钟触发”信号时，执行此方法
    /// </summary>
    private void OnAlarmTriggered(Alarm triggeredAlarm)
    {
        if (alarmRingingPanel != null)
        {
            // 命令响铃面板显示出来，并把闹钟数据传递给它
            alarmRingingPanel.Show(triggeredAlarm);
        }
        else
        {
            Debug.LogError("UIManager无法找到AlarmRingingPanel的引用！请在Inspector中设置。");
        }
    }

    /// <summary>
    /// 一个公开的指令，用于打开闹钟设置界面（可用于新建或编辑）
    /// </summary>
    public void ShowAlarmSetupScreen(Alarm alarmToEdit)
    {
        if (alarmSetupScreen != null)
        {
            // 命令设置面板显示出来，并把闹钟数据传递给它
            alarmSetupScreen.Show(alarmToEdit);
        }
        else
        {
            Debug.LogError("UIManager无法找到AlarmSetupScreen的引用！请在Inspector中设置。");
        }
    }
}