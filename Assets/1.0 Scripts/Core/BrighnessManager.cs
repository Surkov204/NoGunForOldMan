using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class BrightnessManager : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;

    private const string BrightnessKey = "BrightnessValue";

    private void Awake()
    {
        float savedValue = PlayerPrefs.GetFloat(BrightnessKey, 1f);
        ApplyBrightness(savedValue);
    }

    private void OnEnable()
    {
        BrightnessSetting.OnBrightnessChanged += ApplyBrightness;
    }

    private void OnDisable()
    {
        BrightnessSetting.OnBrightnessChanged -= ApplyBrightness;
    }

    private void ApplyBrightness(float value)
    {
        if (globalLight != null)
        {
            globalLight.intensity = Mathf.Clamp01(value); 
            PlayerPrefs.SetFloat(BrightnessKey, globalLight.intensity);
            PlayerPrefs.Save();
        }
    }
}
