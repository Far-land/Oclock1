using UnityEngine;
using UnityEngine.Events; // ����Unity�¼�
using UnityEngine.EventSystems; // �����¼�ϵͳ

public class UIInteractionHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("˫������")]
    [Tooltip("���������������Ϊһ��˫�����룩")]
    public float doubleClickThreshold = 0.3f;

    [Header("Ҫ�������¼�")]
    public UnityEvent onDoubleClick; // ���ǽ���Inspector������˫������ʲô

    private float lastClickTime = -1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.time;

        // �������ϴε����ʱ��
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            // ���ʱ�����㹻�̣������һ��˫��
            Debug.Log("˫���¼�������");

            // ����������Inspector�����úõ��¼�
            onDoubleClick.Invoke();

            // ����ʱ�䣬��ֹ��������
            lastClickTime = -1f;
        }
        else
        {
            // ����ǵ�һ�ε�������߾����ϴε��ʱ��̫��
            lastClickTime = currentTime;
        }
    }
}