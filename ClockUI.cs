using UnityEngine;
using TMPro;
using System;

/// <summary>
/// �������UIʱ����ʾ��
/// ����TimeManager���¼���ʵ�ָ�Ч���¡�
/// </summary>
public class ClockUI : MonoBehaviour
{
    public TextMeshProUGUI clockText;
    public bool convertToLocalTime = true; // �Ƿ�UTCʱ��ת��Ϊ����ʱ����ʾ

    void Start()
    {
        if (clockText == null)
        {
            clockText = GetComponent<TextMeshProUGUI>();
        }

        // ���TimeManager�Ƿ����
        if (TimeManager.Instance == null)
        {
            Debug.LogError("�������Ҳ��� TimeManager!");
            return;
        }

        // ����ÿ������¼�
        TimeManager.Instance.OnSecondTick += UpdateClockText;
    }

    void OnDestroy()
    {
        // �ڶ�������ʱȡ�����ģ���ֹ�ڴ�й©
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