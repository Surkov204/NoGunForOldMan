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

    private void OnEnable()
    {
        // Sync slider theo giá trị hiện tại trong AudioManager
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSFXVolume();

        musicSlider.onValueChanged.AddListener(ChangeVolumeMusic);
        sfxSlider.onValueChanged.AddListener(ChangeVolumeSfx);
    }

    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(ChangeVolumeMusic);
        sfxSlider.onValueChanged.RemoveListener(ChangeVolumeSfx);
    }

    public void ChangeVolumeMusic(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void ChangeVolumeSfx(float value)
    { 
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();
        AudioManager.Instance.SetSFXVolume(value);
    }

}
