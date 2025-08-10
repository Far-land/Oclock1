using UnityEngine;
using System;

public class AlarmUISetupExample : MonoBehaviour
{
    // ��Unity Editor��ͨ����ť��OnClick()�¼��������������
    public void CreateTestAlarm()
    {
        // ����һ��2���Ӻ�����ĵ�������
        DateTime triggerTime = DateTime.Now.AddMinutes(2);

        Alarm newAlarm = new Alarm(triggerTime.Hour, triggerTime.Minute, "��������");

        // ����������ظ���������������
        // newAlarm.repeatDays.Add(DayOfWeek.Monday);
        // newAlarm.repeatDays.Add(DayOfWeek.Friday);

        // ͨ�������������
        AlarmManager.Instance.AddAlarm(newAlarm);
    }
}