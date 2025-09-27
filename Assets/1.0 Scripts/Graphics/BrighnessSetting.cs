using System;
using UnityEngine;
using UnityEngine.UI;

public class BrightnessSetting : MonoBehaviour
{
    [SerializeField] private Slider brightnessSlider;

    private const string BrightnessKey = "BrightnessValue";

    public static event Action<float> OnBrightnessChanged;

    private void Start()
    {
        float savedValue = PlayerPrefs.GetFloat(BrightnessKey, 1f);

        brightnessSlider.minValue = 0f;
        brightnessSlider.maxValue = 1f;
        brightnessSlider.value = savedValue;

        brightnessSlider.onValueChanged.AddListener(ChangeBrightness);

        OnBrightnessChanged?.Invoke(savedValue);
    }

    private void ChangeBrightness(float value)
    {
        OnBrightnessChanged?.Invoke(value);
    }
}
