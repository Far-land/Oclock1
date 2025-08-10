using UnityEngine;
using System.IO;

public class UserManager : MonoBehaviour
{
    // 【修改点】将公开的Instance getter变得更智能
    private static UserManager _instance;
    public static UserManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 如果实例不存在，就在场景中查找
                _instance = FindObjectOfType<UserManager>();
                if (_instance == null)
                {
                    // 如果还是找不到，就创建一个新的
                    GameObject singletonObject = new GameObject("UserManager");
                    _instance = singletonObject.AddComponent<UserManager>();
                }
            }
            return _instance;
        }
    }

    public UserData CurrentUser { get; private set; }

    private string saveFileName = "userdata.json";
    private string savePath;

    // Awake现在只负责初始化自己
    void Awake()
    {
        // 检查是否已经有一个实例存在（比如从启动场景过来的）
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化数据
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadUserData();
    }

    public void LoadUserData()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                CurrentUser = JsonUtility.FromJson<UserData>(json);
                Debug.Log("用户数据加载成功。");

                // 【关键修复】如果加载出的数据是空的，也要创建一个新的
                if (CurrentUser == null)
                {
                    Debug.LogWarning("加载的用户数据为空，将创建新的用户数据。");
                    CurrentUser = new UserData();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载用户数据失败: {e.Message}. 将创建新的用户数据。");
                CurrentUser = new UserData();
            }
        }
        else
        {
            Debug.Log("未发现用户数据，将创建新的用户数据。");
            CurrentUser = new UserData();
        }
    }

    public void SaveUserData()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentUser, true);
            File.WriteAllText(savePath, json);
            Debug.Log("用户数据保存成功。");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存用户数据失败: {e.Message}");
        }
    }
}