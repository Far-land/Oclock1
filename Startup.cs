using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    // ��Inspector������������������
    public string onboardingSceneName = "OnboardingScene";
    public string mainSceneName = "MainScene";

    // Start������������Awake����ִ����Ϻ�ִ��
    void Start()
    {
        // ��ʱ��ͬһ��GameObject�ϵ�UserManager��AlarmManager��Awake���Ѿ�ִ�����
        // ���ǿ��԰�ȫ�ط������ǵ�Instance

        if (UserManager.Instance == null)
        {
            Debug.LogError("���ش���UserManagerʵ��δ�ܴ�����");
            return;
        }

        // ����û����ݣ��ж��Ƿ����������
        if (UserManager.Instance.CurrentUser.hasCompletedOnboarding)
        {
            // �������ɣ�ֱ����ת��������
            Debug.Log("�û������������ֱ�ӽ�����������");
            SceneManager.LoadScene(mainSceneName);
        }
        else
        {
            // ���δ��ɣ�����ת����������
            Debug.Log("�״ν��룬��ʼ�������̡�");
            SceneManager.LoadScene(onboardingSceneName);
        }
    }
}