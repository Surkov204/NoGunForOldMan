using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneMusicConfig", menuName = "Scriptable Objects/SceneMusicConfig")]
public class SceneMusicConfig : ScriptableObject
{
    [Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;      
        public AudioClip musicClip;  
    }

    public List<SceneMusicEntry> sceneMusicList = new List<SceneMusicEntry>();

    public AudioClip GetMusicForScene(string sceneName)
    {
        var entry = sceneMusicList.Find(e => e.sceneName == sceneName);
        return entry != null ? entry.musicClip : null;
    }
}
