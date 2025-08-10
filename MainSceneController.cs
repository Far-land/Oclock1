using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ����ű��� MainScene �ġ�����Ա���򡰳����ܹܡ���
/// ����Ψһְ���ǣ��ڳ�������ʱ���ҵ���StartupScene����Խ��������ȫ�ֹ�������
/// Ȼ��ѱ������еĹؼ�UI������Ǽǡ������ǡ�
/// </summary>
public class MainSceneController : MonoBehaviour
{
    [Header("--- ��Ҫ�Ǽǵ�UI��� ---")]

    [Header("����UI��弰���ڲ����")]
    public GameObject alarmRingingPanel;
    public TextMeshProUGUI alarmLabelText;
    public TextMeshProUGUI currentTimeText;
    public Button dismissButton;
    public Button snoozeButton;

    // δ�������԰�AlarmSetupScreen��������Ҫ�糡��ͨ�ŵ����Ҳ�ӵ�����
    // public AlarmSetupScreen alarmSetupScreen;


    void Start()
    {
        // ��MainScene����ʱ������ִ�����ӣ��Ǽǣ�����
        RegisterPanelsToManagers();
    }

    private void RegisterPanelsToManagers()
    {
        // ���AlarmRingingController�ĵ����Ƿ����
        if (AlarmRingingController.Instance != null)
        {
            // ���á��Ǽǡ����������Լ��������UI��������ܡ�����
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
            Debug.LogError("��MainScene���Ҳ���AlarmRingingController��ʵ�����޷��Ǽ�������塣");
        }

        // δ��������Ǽ��������...
        /*
        if (UIManager.Instance != null)
        {
            UIManager.Instance.alarmSetupScreen = this.alarmSetupScreen;
        }
        */
    }
}