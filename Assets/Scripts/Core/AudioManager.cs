using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance {  get; private set; }

    [SerializeField] private AudioSource setSfxVolume;
    [SerializeField] private AudioSource setMusicVolume;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        setSfxVolume = GetComponent<AudioSource>();

        float savedMusicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float savedSfxVol = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (setMusicVolume != null) setMusicVolume.volume = savedMusicVol;
        if (setSfxVolume != null) setSfxVolume.volume = savedSfxVol;

        VolumeController.OnMusicVolumeChanged += SetMusicVolume;
        VolumeController.OnSfxVolumechanged += SetSFXVolume;
    }


    private void OnDestroy()
    {
        VolumeController.OnMusicVolumeChanged -= SetMusicVolume;
        VolumeController.OnSfxVolumechanged -= SetSFXVolume;
    }

    public void PlaySound(AudioClip _sound)
    {
        setSfxVolume.PlayOneShot(_sound);  
    }

    public void SetSFXVolume(float value) => setSfxVolume.volume = value;
    public void SetMusicVolume(float value) => setMusicVolume.volume = value;

}
