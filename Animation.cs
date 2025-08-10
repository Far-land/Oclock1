using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimationController : MonoBehaviour
{
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float animationSpeed = 1f;
    public GameObject panel;

    // 用于跟踪当前动画状态，防止重复触发
    private bool isAnimating = false;


    // 这个方法将绑定到按钮的点击事件
    public void TogglePanel()
    {
        // 如果正在动画中，不执行新的动画
        if (isAnimating) return;

        // 根据面板当前的激活状态决定显示或隐藏
        if (panel.activeSelf)
        {
            StartCoroutine(HidePanel(panel));
        }
        else
        {
            // 确保在开始显示动画前激活面板
            panel.SetActive(true);
            StartCoroutine(ShowPanel(panel));
        }
    }

    IEnumerator ShowPanel(GameObject gameObject)
    {
        isAnimating = true;
        float timer = 0;

        while (timer <= 1)
        {
            gameObject.transform.localScale = Vector3.one * showCurve.Evaluate(timer);
            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }

        // 确保最终状态正确
        gameObject.transform.localScale = Vector3.one;
        isAnimating = false;
    }

    IEnumerator HidePanel(GameObject gameObject)
    {
        isAnimating = true;
        float timer = 0;

        while (timer <= 1)
        {
            gameObject.transform.localScale = Vector3.one * hideCurve.Evaluate(timer);
            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }

        // 隐藏面板
        gameObject.SetActive(false);
        // 重置缩放以便下次显示动画正常工作
        gameObject.transform.localScale = Vector3.zero;
        isAnimating = false;
    }
}