using JetBrains.Annotations;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    private const string ResIndexKey = "ResolutionIndex";
    private const string FullScreenKey = "FullScreen";

    private Resolution[] resolutions;
    private int currentIndex;
    private bool isFullscreen;

    private void Awake()
    {
        resolutions = Screen.resolutions;

        currentIndex = PlayerPrefs.GetInt(ResIndexKey, resolutions.Length - 1);
        if (currentIndex < 0 || currentIndex >= resolutions.Length)
            currentIndex = resolutions.Length - 1;

        isFullscreen = PlayerPrefs.GetInt(FullScreenKey, 1) == 1;
    }

    private void OnEnable()
    {
        ResolutionSettings.ResolutionChanged += SetResolution;
        ResolutionSettings.FullScreenChanged += SetIsFullScreen;
    }

    private void OnDisable()
    {
        ResolutionSettings.ResolutionChanged -= SetResolution;
        ResolutionSettings.FullScreenChanged -= SetIsFullScreen;
    }

    public void SetResolution(int index) {
        currentIndex = index;
        SaveValue();
        ApplyResolution();
    }

    public void SetIsFullScreen(bool isOn)
    {
        isFullscreen = isOn;
        SaveValue();
        ApplyResolution();
    }

    private void ApplyResolution() {
        Resolution res = resolutions[currentIndex];
        Screen.SetResolution(res.width, res.height, isFullscreen);
    }

    private void SaveValue() {
        PlayerPrefs.SetInt(ResIndexKey, currentIndex);
        PlayerPrefs.SetInt(FullScreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

}
