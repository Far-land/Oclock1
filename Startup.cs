using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    // 在Inspector中设置您场景的名字
    public string onboardingSceneName = "OnboardingScene";
    public string mainSceneName = "MainScene";

    // Start方法会在所有Awake方法执行完毕后执行
    void Start()
    {
        // 此时，同一个GameObject上的UserManager和AlarmManager的Awake都已经执行完毕
        // 我们可以安全地访问它们的Instance

        if (UserManager.Instance == null)
        {
            Debug.LogError("严重错误：UserManager实例未能创建！");
            return;
        }

        // 检查用户数据，判断是否已完成引导
        if (UserManager.Instance.CurrentUser.hasCompletedOnboarding)
        {
            // 如果已完成，直接跳转到主场景
            Debug.Log("用户已完成引导，直接进入主场景。");
            SceneManager.LoadScene(mainSceneName);
        }
        else
        {
            // 如果未完成，则跳转到引导场景
            Debug.Log("首次进入，开始引导流程。");
            SceneManager.LoadScene(onboardingSceneName);
        }
    }
}