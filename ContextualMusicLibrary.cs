using UnityEngine;
using System;
using System.Collections.Generic;

public enum WeatherType { Sunny, Rainy, Snowy, Default }
public enum TimeOfDay { AM, PM, Noon, Midnight }

[Serializable]
public class ContextualMusicTrack
{
    public string trackName;
    public WeatherType weather;
    public TimeOfDay timeOfDay;
    [Range(1, 11)] public int hour;
    public AudioClip audioClip;
}

[CreateAssetMenu(fileName = "ContextualMusicLibrary", menuName = "AlarmApp/Contextual Music Library", order = 2)]
public class ContextualMusicLibrary : ScriptableObject
{
    public List<ContextualMusicTrack> contextualTracks = new List<ContextualMusicTrack>();
    public AudioClip defaultRingtone;
}