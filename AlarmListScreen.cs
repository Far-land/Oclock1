using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AlarmListScreen : MonoBehaviour
{
    [Header("UI 引用")]
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
        // 【关键修复】在清空列表前，先禁用ToggleGroup，防止触发事件
        if (listToggleGroup != null) listToggleGroup.allowSwitchOff = true; // 允许全部关闭

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
            // 命令UI总管打开“编辑”模式的设置页面
            UIManager.Instance.ShowAlarmSetupScreen(alarmToEdit);
        }
    }
}