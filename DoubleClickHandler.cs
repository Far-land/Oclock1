using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
{
    // 在Inspector中，把您想打开的那个面板拖到这里
    public AlarmSetupScreen targetSetupScreen;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 直接使用Unity自带的双击检测
        if (eventData.clickCount == 2)
        {
            Debug.Log("双击成功，命令目标面板显示！");
            if (targetSetupScreen != null)
            {
                // 直接调用目标面板的Show方法
                targetSetupScreen.Show(null); // 在这里传递一个null值
            }
        }
    }
}