using UnityEngine;

[System.Serializable]
public class Ringtone
{
    public string ringtoneName;
    public AudioClip audioClip;
}

[CreateAssetMenu(fileName = "RingtoneLibrary", menuName = "AlarmApp/Ringtone Library", order = 1)]
public class RingtoneData : ScriptableObject
{
    public Ringtone[] ringtones;
}