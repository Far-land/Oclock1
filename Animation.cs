using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimationController : MonoBehaviour
{
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float animationSpeed = 1f;
    public GameObject panel;

    // ���ڸ��ٵ�ǰ����״̬����ֹ�ظ�����
    private bool isAnimating = false;


    // ����������󶨵���ť�ĵ���¼�
    public void TogglePanel()
    {
        // ������ڶ����У���ִ���µĶ���
        if (isAnimating) return;

        // ������嵱ǰ�ļ���״̬������ʾ������
        if (panel.activeSelf)
        {
            StartCoroutine(HidePanel(panel));
        }
        else
        {
            // ȷ���ڿ�ʼ��ʾ����ǰ�������
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

        // ȷ������״̬��ȷ
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

        // �������
        gameObject.SetActive(false);
        // ���������Ա��´���ʾ������������
        gameObject.transform.localScale = Vector3.zero;
        isAnimating = false;
    }
}