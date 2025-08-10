using UnityEngine;

public class AlarmReaction : MonoBehaviour
{
    public GameObject alarmPopupUI; // ָ��������ӵ���UI���
    public AudioSource alarmAudioSource; // ���ڲ�������������AudioSource���

    void Start()
    {
        // ����AlarmManager���¼�
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered += HandleAlarmTrigger;
        }

        if (alarmPopupUI != null)
        {
            alarmPopupUI.SetActive(false); // ȷ������һ��ʼ�����ص�
        }
    }

    void OnDestroy()
    {
        // ȡ�����ģ���ֹ�ڴ�й©
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmTriggered -= HandleAlarmTrigger;
        }
    }

    private void HandleAlarmTrigger(Alarm triggeredAlarm)
    {
        Debug.Log($"������Ӧ����: {triggeredAlarm.label}");

        // 1. ��ʾUI����
        if (alarmPopupUI != null)
        {
            // ������������������Ϣ���ݸ�UI
            // var popupController = alarmPopupUI.GetComponent<AlarmPopupController>();
            // popupController.Setup(triggeredAlarm);
            alarmPopupUI.SetActive(true);
        }

        // 2. ��������
        if (alarmAudioSource != null && alarmAudioSource.clip != null)
        {
            alarmAudioSource.Play();
        }
    }
}