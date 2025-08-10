using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class OnboardingController : MonoBehaviour
{
    [Header("���������")]
    public string mainSceneName = "MainScene"; // ������ʱ�ӳ�����
    public GameObject introductionPanel;
    public GameObject videoPlayerLayer;
    public GameObject registerPanel;

    [Header("�������")]
    public Button playVideoButton;
    public VideoPlayer videoPlayer;

    [Header("ע��UI����")]
    public TMP_InputField usernameInput;
    public TMP_Text tempWeather;
    //public TMP_Dropdown regionDropdown;
    public Button format12hButton;
    public Button format24hButton;
    public Button completeButton;
    // ... δ������ѡ���UI���� ...

    private bool use24HourFormat = true;

    void Start()
    {
       // LocationWeatherManager.Instance.StartFetchingData();

        // ���ó�ʼUI״̬
        introductionPanel.SetActive(true);
        videoPlayerLayer.SetActive(false);
        registerPanel.SetActive(false);
        // �󶨰�ť�¼�
        playVideoButton.onClick.AddListener(PlayVideo);
        videoPlayer.loopPointReached += OnVideoFinished;

        format12hButton.onClick.AddListener(() => SelectTimeFormat(false));
        format24hButton.onClick.AddListener(() => SelectTimeFormat(true));
        completeButton.onClick.AddListener(CompleteOnboarding);
    }

    /// <summary>
    /// �����ť����ʼ������Ƶ
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
        // 1. �ռ��������û�����
        UserManager.Instance.CurrentUser.username = usernameInput.text;
        LocationWeatherManager.Instance.LoadedLocationData.city = tempWeather.text;
        UserManager.Instance.CurrentUser.use24HourFormat = use24HourFormat;
        // ... �����������Ի����� ...

        UserManager.Instance.CurrentUser.hasCompletedOnboarding = true;
        UserManager.Instance.SaveUserData();

        // 2. ֱ���л���������
        SceneManager.LoadScene(mainSceneName);
    }
}
