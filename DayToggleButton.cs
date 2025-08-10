using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 需要Button组件才能工作
[RequireComponent(typeof(Button))]
public class DayToggleButton : MonoBehaviour
{
    public DayOfWeek day; // 在Inspector中为每个按钮设置对应的星期

    [Header("颜色设置")]
    public Color selectedColor = Color.cyan;
    public Color deselectedColor = Color.white;

    private Button button;
    private Image buttonImage;
    private TextMeshProUGUI buttonText;

    private bool _isSelected = false;

    // 公开属性，用于外部获取或设置状态
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

        // 添加点击事件监听
        button.onClick.AddListener(ToggleState);

        // 初始化视觉效果
        UpdateVisuals();
    }

    private void ToggleState()
    {
        IsSelected = !IsSelected;
        // 在这里可以添加音效
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