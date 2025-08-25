using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class VolumeController : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private Slider musicSlider;

    [Header("Sound Effect")]
    [SerializeField] private Slider sfxSlider;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSfxVolumechanged;

    void Start()
    {

        float savedMusicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 1.0f);
        float savedSfxVol = PlayerPrefs.GetFloat(SfxVolumeKey, 1.0f);

        musicSlider.value = savedMusicVol;
        sfxSlider.value = savedSfxVol;

        musicSlider.onValueChanged.AddListener(ChangeVolumeMusic);
        sfxSlider.onValueChanged.AddListener(ChangeVolumeSfx);

        OnMusicVolumeChanged?.Invoke(savedMusicVol);
        OnSfxVolumechanged?.Invoke(savedSfxVol);

    }

    public void ChangeVolumeMusic(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
        OnMusicVolumeChanged?.Invoke(value);
    }

    public void ChangeVolumeSfx(float value)
    { 
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();
        OnSfxVolumechanged?.Invoke(value);
    }

}
