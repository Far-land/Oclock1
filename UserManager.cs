using UnityEngine;
using System.IO;

public class UserManager : MonoBehaviour
{
    // ���޸ĵ㡿��������Instance getter��ø�����
    private static UserManager _instance;
    public static UserManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // ���ʵ�������ڣ����ڳ����в���
                _instance = FindObjectOfType<UserManager>();
                if (_instance == null)
                {
                    // ��������Ҳ������ʹ���һ���µ�
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

    // Awake����ֻ�����ʼ���Լ�
    void Awake()
    {
        // ����Ƿ��Ѿ���һ��ʵ�����ڣ�������������������ģ�
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // ��ʼ������
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
                Debug.Log("�û����ݼ��سɹ���");

                // ���ؼ��޸���������س��������ǿյģ�ҲҪ����һ���µ�
                if (CurrentUser == null)
                {
                    Debug.LogWarning("���ص��û�����Ϊ�գ��������µ��û����ݡ�");
                    CurrentUser = new UserData();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"�����û�����ʧ��: {e.Message}. �������µ��û����ݡ�");
                CurrentUser = new UserData();
            }
        }
        else
        {
            Debug.Log("δ�����û����ݣ��������µ��û����ݡ�");
            CurrentUser = new UserData();
        }
    }

    public void SaveUserData()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentUser, true);
            File.WriteAllText(savePath, json);
            Debug.Log("�û����ݱ���ɹ���");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�����û�����ʧ��: {e.Message}");
        }
    }
}