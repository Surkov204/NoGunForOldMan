using JS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : ManualSingletonMono<AudioManager>
{

    [SerializeField] private AudioSource setSfxVolume;
    [SerializeField] private AudioSource setMusicVolume;

    [Header("Scene → Music Config")]
    [SerializeField] private SceneMusicConfig musicConfig;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private void Awake()
    {
        base.Awake(); 

        if (Instance != this) return;

        float savedMusicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float savedSfxVol = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        SetMusicVolume(savedMusicVol);
        SetSFXVolume(savedSfxVol);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (setMusicVolume == null || setMusicVolume == null) return;

        AudioClip clip = musicConfig.GetMusicForScene(sceneName);

        if (clip != null && setMusicVolume.clip != clip)
        {
            setMusicVolume.clip = clip;
            setMusicVolume.loop = true;
            setMusicVolume.Play();
        }
    }

    public void PlaySound(AudioClip _sound)
    {
        setSfxVolume.PlayOneShot(_sound);  
    }

    public void SetMusicVolume(float value)
    {
        if (setMusicVolume != null) setMusicVolume.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        if (setSfxVolume != null) setSfxVolume.volume = value;
    }

    public float GetMusicVolume() => setMusicVolume != null ? setMusicVolume.volume : 1f;
    public float GetSFXVolume() => setSfxVolume != null ? setSfxVolume.volume : 1f;

}
