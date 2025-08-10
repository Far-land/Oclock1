using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// ��ҪButton������ܹ���
[RequireComponent(typeof(Button))]
public class DayToggleButton : MonoBehaviour
{
    public DayOfWeek day; // ��Inspector��Ϊÿ����ť���ö�Ӧ������

    [Header("��ɫ����")]
    public Color selectedColor = Color.cyan;
    public Color deselectedColor = Color.white;

    private Button button;
    private Image buttonImage;
    private TextMeshProUGUI buttonText;

    private bool _isSelected = false;

    // �������ԣ������ⲿ��ȡ������״̬
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            _isSelected = value;
            UpdateVisuals();
        }
    }

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // ��ӵ���¼�����
        button.onClick.AddListener(ToggleState);

        // ��ʼ���Ӿ�Ч��
        UpdateVisuals();
    }

    private void ToggleState()
    {
        IsSelected = !IsSelected;
        // ��������������Ч
        // SoundManager.PlayClickSound();

    }

    private void UpdateVisuals()
    {
        if (buttonImage != null)
        {
            buttonImage.color = IsSelected ? selectedColor : deselectedColor;
        }
    }
}