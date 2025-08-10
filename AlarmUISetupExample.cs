using UnityEngine;
using System;

public class AlarmUISetupExample : MonoBehaviour
{
    // 在Unity Editor中通过按钮的OnClick()事件来调用这个函数
    public void CreateTestAlarm()
    {
        // 创建一个2分钟后响起的单次闹钟
        DateTime triggerTime = DateTime.Now.AddMinutes(2);

        Alarm newAlarm = new Alarm(triggerTime.Hour, triggerTime.Minute, "测试闹钟");

        // 如果想设置重复，可以这样做：
        // newAlarm.repeatDays.Add(DayOfWeek.Monday);
        // newAlarm.repeatDays.Add(DayOfWeek.Friday);

        // 通过单例添加闹钟
        AlarmManager.Instance.AddAlarm(newAlarm);
    }
}