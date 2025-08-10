using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

// ȷ�������������Animator��Canvas Group���
[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class AlarmRingingPanel : MonoBehaviour
{
    [Header("UI ����")]
    public TextMeshProUGUI alarmLabelText;
    public TextMeshProUGUI currentTimeText;
    public Button dismissButton;
    public Button snoozeButton;

    private AudioSource audioSource;
    private Alarm currentRingingAlarm;
    private Animator animator;

    void Awake()
    {
        // ��ȡ�Լ����ϵ����
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // ���Լ��ڲ���ť���¼�
        dismissButton.onClick.AddListener(OnDismissClicked);
        snoozeButton.onClick.AddListener(OnSnoozeClicked);
    }

    /// <summary>
    /// �������޸���������UIManager���õĹ���������������ʾ���������
    /// </summary>
    public void Show(Alarm alarm)
    {
        currentRingingAlarm = alarm;

        // ����UI�ı�
        if (alarmLabelText != null)
        {
            alarmLabelText.text = currentRingingAlarm.label;
        }

        // ������󲢲��š�Show������
        gameObject.SetActive(true);
        animator.SetTrigger("Show");

        // ��������
        PlayRingtone();
    }

    /// <summary>
    /// һ��˽�еķ��������ڴ����رն���
    /// </summary>
    private void Hide()
    {
        if (audioSource != null) audioSource.Stop();
        animator.SetTrigger("Hide");
    }

    // ��Hide���������һ֡������¼��������������
    public void OnHideAnimationComplete()
    {
        gameObject.SetActive(false);
    }

    private void PlayRingtone()
    {
        if (MusicManager.Instance == null || LocationWeatherManager.Instance == null)
        {
            Debug.LogError("MusicManager �� LocationWeatherManager δ׼��������");
            return;
        }

        var weatherData = LocationWeatherManager.Instance.LoadedWeatherData;
        AudioClip clipToPlay = MusicManager.Instance.GetContextualRingtone(currentRingingAlarm.GetTriggerTime(), weatherData);

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("������������Ҳ�����Ӧ��������");
        }
    }

    void Update()
    {
        // ֻ��Ҫ���Լ��ɼ�ʱ����ʱ��
        if (currentTimeText != null)
        {
            currentTimeText.text = DateTime.Now.ToString("HH:mm");
        }
    }

    // --- ��ť��Ӧ ---
    private void OnDismissClicked()
    {
        Hide();
    }

    private void OnSnoozeClicked()
    {
        if (currentRingingAlarm != null)
        {
            DateTime snoozeTime = AlarmManager.Instance.CurrentTime.AddMinutes(9);
            Alarm snoozeAlarm = new Alarm(snoozeTime.Hour, snoozeTime.Minute, currentRingingAlarm.label + " (�Ժ�����)");
            snoozeAlarm.specificDate = snoozeTime.ToString("yyyy-MM-dd");
            // �Ժ�����Ҳʹ���龳����
            // snoozeAlarm.ringtoneName = currentRingingAlarm.ringtoneName; 

            AlarmManager.Instance.AddAlarm(snoozeAlarm);
        }

        Hide();
    }
}