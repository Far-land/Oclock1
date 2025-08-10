using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 这个脚本是 MainScene 的“联络员”或“场景总管”。
/// 它的唯一职责是：在场景启动时，找到从StartupScene“穿越”过来的全局管理器，
/// 然后把本场景中的关键UI组件“登记”给它们。
/// </summary>
public class MainSceneController : MonoBehaviour
{
    [Header("--- 需要登记的UI面板 ---")]

    [Header("响铃UI面板及其内部组件")]
    public GameObject alarmRingingPanel;
    public TextMeshProUGUI alarmLabelText;
    public TextMeshProUGUI currentTimeText;
    public Button dismissButton;
    public Button snoozeButton;

    // 未来您可以把AlarmSetupScreen等其他需要跨场景通信的面板也加到这里
    // public AlarmSetupScreen alarmSetupScreen;


    void Start()
    {
        // 当MainScene启动时，立即执行连接（登记）操作
        RegisterPanelsToManagers();
    }

    private void RegisterPanelsToManagers()
    {
        // 检查AlarmRingingController的单例是否存在
        if (AlarmRingingController.Instance != null)
        {
            // 调用“登记”方法，把自己场景里的UI组件“介绍”给它
            AlarmRingingController.Instance.RegisterRingingPanel(
                alarmRingingPanel,
                alarmLabelText,
                currentTimeText,
                dismissButton,
                snoozeButton
            );
        }
        else
        {
            Debug.LogError("在MainScene中找不到AlarmRingingController的实例！无法登记响铃面板。");
        }

        // 未来在这里登记其他面板...
        /*
        if (UIManager.Instance != null)
        {
            UIManager.Instance.alarmSetupScreen = this.alarmSetupScreen;
        }
        */
    }
}