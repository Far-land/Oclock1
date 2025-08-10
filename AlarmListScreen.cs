using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AlarmListScreen : MonoBehaviour
{
    [Header("UI ����")]
    public GameObject alarmItemPrefab;
    public Transform listContainer;

    private ToggleGroup listToggleGroup;

    void Awake()
    {
        listToggleGroup = listContainer.GetComponent<ToggleGroup>();
    }

    void OnEnable()
    {
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmListChanged += RefreshList;
        }
        RefreshList();
    }

    void OnDisable()
    {
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.OnAlarmListChanged -= RefreshList;
        }
    }

    private void RefreshList()
    {
        // ���ؼ��޸���������б�ǰ���Ƚ���ToggleGroup����ֹ�����¼�
        if (listToggleGroup != null) listToggleGroup.allowSwitchOff = true; // ����ȫ���ر�

        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        List<Alarm> currentAlarms = AlarmManager.Instance.GetAllAlarms();
        if (currentAlarms == null) return;

        foreach (var alarm in currentAlarms)
        {
            if (alarm == null) continue;
            GameObject newItem = Instantiate(alarmItemPrefab, listContainer);
            newItem.transform.localScale = Vector3.one;
            AlarmListItem listItemScript = newItem.GetComponent<AlarmListItem>();
            if (listItemScript != null)
            {
                listItemScript.Setup(alarm, listToggleGroup, OnEditAlarmClicked);
            }
        }
    }

    private void OnEditAlarmClicked(Alarm alarmToEdit)
    {
        if (UIManager.Instance != null)
        {
            // ����UI�ܹܴ򿪡��༭��ģʽ������ҳ��
            UIManager.Instance.ShowAlarmSetupScreen(alarmToEdit);
        }
    }
}