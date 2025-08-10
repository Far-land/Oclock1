using UnityEngine;
using UnityEngine.Events; // 引入Unity事件
using UnityEngine.EventSystems; // 引入事件系统

public class UIInteractionHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("双击设置")]
    [Tooltip("多快的连续点击被视为一次双击（秒）")]
    public float doubleClickThreshold = 0.3f;

    [Header("要触发的事件")]
    public UnityEvent onDoubleClick; // 我们将在Inspector中设置双击后做什么

    private float lastClickTime = -1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.time;

        // 检查距离上次点击的时间
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            // 如果时间间隔足够短，这就是一次双击
            Debug.Log("双击事件触发！");

            // 触发我们在Inspector中设置好的事件
            onDoubleClick.Invoke();

            // 重置时间，防止连续触发
            lastClickTime = -1f;
        }
        else
        {
            // 如果是第一次点击，或者距离上次点击时间太长
            lastClickTime = currentTime;
        }
    }
}