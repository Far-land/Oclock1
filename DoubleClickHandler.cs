using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
{
    // ��Inspector�У�������򿪵��Ǹ�����ϵ�����
    public AlarmSetupScreen targetSetupScreen;

    public void OnPointerClick(PointerEventData eventData)
    {
        // ֱ��ʹ��Unity�Դ���˫�����
        if (eventData.clickCount == 2)
        {
            Debug.Log("˫���ɹ�������Ŀ�������ʾ��");
            if (targetSetupScreen != null)
            {
                // ֱ�ӵ���Ŀ������Show����
                targetSetupScreen.Show(null); // �����ﴫ��һ��nullֵ
            }
        }
    }
}