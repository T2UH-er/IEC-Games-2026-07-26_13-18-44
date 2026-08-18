using System;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType
{
    BGM,
    SFX
}

[Serializable]
public class AudioData
{
    public string audioName;
    public AudioClip clip;
    [Range(0f, 1f)] public float defaultVolume = 1f;
    public AudioType audioType;
}

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    public List<AudioData> audioList = new List<AudioData>();

    public AudioData GetAudio(string audioName)
    {
        return audioList.Find(a => a.audioName.Equals(audioName, StringComparison.OrdinalIgnoreCase));
    }
}