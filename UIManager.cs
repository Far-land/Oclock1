using UnityEngine;
using System.Collections;

/// <summary>
/// UI�ܹ� (����)��
/// ����Ӧ����Ψһ��UI�������ģ�����������UI������ʾ�����ء�
/// </summary>
public class UIManager : MonoBehaviour
{
    // ����һ��ȫ��Ψһ�ľ�̬ʵ�������������κνű�ͨ�� UIManager.Instance ��������
    public static UIManager Instance { get; private set; }

    [Header("UI���ű�������")]
    // ��Inspector�У���AlarmSetupPanel������ק������
    public AlarmSetupScreen alarmSetupScreen;

    // ��Inspector�У���AlarmRingingPanel������ק������
    public AlarmRingingPanel alarmRingingPanel;

    void Awake()
    {
        // ���õ���ģʽ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ϊȷ����ȫ��������ʱǿ�����������ܿ����
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
        // UI�ܹ��Լ������������Ӵ������¼�
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered += OnAlarmTriggered;
        }
    }

    void OnDestroy()
    {
        // ���Լ�������ʱ��ȡ���¼����ģ���ֹ�ڴ�й©
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered -= OnAlarmTriggered;
        }
    }

    /// <summary>
    /// ����AlarmManager���յ������Ӵ������ź�ʱ��ִ�д˷���
    /// </summary>
    private void OnAlarmTriggered(Alarm triggeredAlarm)
    {
        if (alarmRingingPanel != null)
        {
            // �������������ʾ�����������������ݴ��ݸ���
            alarmRingingPanel.Show(triggeredAlarm);
        }
        else
        {
            Debug.LogError("UIManager�޷��ҵ�AlarmRingingPanel�����ã�����Inspector�����á�");
        }
    }

    /// <summary>
    /// һ��������ָ����ڴ��������ý��棨�������½���༭��
    /// </summary>
    public void ShowAlarmSetupScreen(Alarm alarmToEdit)
    {
        if (alarmSetupScreen != null)
        {
            // �������������ʾ�����������������ݴ��ݸ���
            alarmSetupScreen.Show(alarmToEdit);
        }
        else
        {
            Debug.LogError("UIManager�޷��ҵ�AlarmSetupScreen�����ã�����Inspector�����á�");
        }
    }
}