using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class OnboardingController : MonoBehaviour
{
    [Header("场景与面板")]
    public string mainSceneName = "MainScene"; // 您的主时钟场景名
    public GameObject introductionPanel;
    public GameObject videoPlayerLayer;
    public GameObject registerPanel;

    [Header("核心组件")]
    public Button playVideoButton;
    public VideoPlayer videoPlayer;

    [Header("注册UI引用")]
    public TMP_InputField usernameInput;
    public TMP_Text tempWeather;
    //public TMP_Dropdown regionDropdown;
    public Button format12hButton;
    public Button format24hButton;
    public Button completeButton;
    // ... 未来主题选择的UI引用 ...

    private bool use24HourFormat = true;

    void Start()
    {
       // LocationWeatherManager.Instance.StartFetchingData();

        // 设置初始UI状态
        introductionPanel.SetActive(true);
        videoPlayerLayer.SetActive(false);
        registerPanel.SetActive(false);
        // 绑定按钮事件
        playVideoButton.onClick.AddListener(PlayVideo);
        videoPlayer.loopPointReached += OnVideoFinished;

        format12hButton.onClick.AddListener(() => SelectTimeFormat(false));
        format24hButton.onClick.AddListener(() => SelectTimeFormat(true));
        completeButton.onClick.AddListener(CompleteOnboarding);
    }

    /// <summary>
    /// 点击按钮，开始播放视频
    /// </summary>
    private void PlayVideo()
    {
        introductionPanel.SetActive(false);
        videoPlayerLayer.SetActive(true);
        videoPlayer.Play();
    }

    /// <summary>
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        videoPlayerLayer.SetActive(false);
        registerPanel.SetActive(true);
    }

    private void SelectTimeFormat(bool is24h)
    {
        use24HourFormat = is24h;
        format24hButton.GetComponent<Image>().color = is24h ? Color.cyan : Color.white;
        format12hButton.GetComponent<Image>().color = !is24h ? Color.cyan : Color.white;
    }

    private void CompleteOnboarding()
    {
        // 1. 收集并保存用户数据
        UserManager.Instance.CurrentUser.username = usernameInput.text;
        LocationWeatherManager.Instance.LoadedLocationData.city = tempWeather.text;
        UserManager.Instance.CurrentUser.use24HourFormat = use24HourFormat;
        // ... 保存其他个性化设置 ...

        UserManager.Instance.CurrentUser.hasCompletedOnboarding = true;
        UserManager.Instance.SaveUserData();

        // 2. 直接切换到主场景
        SceneManager.LoadScene(mainSceneName);
    }
}
