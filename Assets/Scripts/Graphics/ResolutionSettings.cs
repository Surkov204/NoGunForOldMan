using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSettings : MonoBehaviour
{
    [SerializeField] private Dropdown resDropdown;
    [SerializeField] private Toggle FullScreen;

    private Resolution[] resolutions;
    private bool isFullScreen;
    private int selectResolution;

    List<Resolution> SelectedResolutionList = new List<Resolution>();

    private const string ResIndexKey = "ResolutionIndex";
    private const string FullScreenKey = "FullScreen";

    public static event Action<int> ResolutionChanged;
    public static event Action<bool> FullScreenChanged;

    private void Start()
    {
        isFullScreen = true;
        resolutions = Screen.resolutions;

        List<string> resolutionList = new List<string>();
        string newRes;
        foreach (Resolution res in resolutions) {
            newRes = res.width.ToString() + " x " + res.height.ToString();
            if (!resolutionList.Contains(newRes)) {
                resolutionList.Add(newRes);
                SelectedResolutionList.Add(res);
            }

        }

        resDropdown.ClearOptions();
        resDropdown.AddOptions(resolutionList);

        if (!PlayerPrefs.HasKey(ResIndexKey))
        {
            int defaultIndex = SelectedResolutionList.FindIndex(r => r.width == 1920 && r.height == 1080);
            if (defaultIndex == -1) defaultIndex = SelectedResolutionList.Count - 1; 

            selectResolution = defaultIndex;
            PlayerPrefs.SetInt(ResIndexKey, selectResolution);
            PlayerPrefs.Save();
        }
        else
        {
            selectResolution = PlayerPrefs.GetInt(ResIndexKey, SelectedResolutionList.Count - 1);
        }

        isFullScreen = PlayerPrefs.GetInt(FullScreenKey, 1) == 1;

        if (selectResolution < 0 || selectResolution >= SelectedResolutionList.Count)
            selectResolution = SelectedResolutionList.Count - 1;

        resDropdown.value = selectResolution;
        resDropdown.RefreshShownValue();

        FullScreen.isOn = isFullScreen;
        ApplyResolution();
    }

    public void ChangeResolution() {
        selectResolution = resDropdown.value;
        ResolutionChanged?.Invoke(selectResolution);
        PlayerPrefs.SetInt(ResIndexKey, selectResolution);
        PlayerPrefs.Save();
        ApplyResolution();
    }

    public void ChangeFullScreen() {
        isFullScreen = FullScreen.isOn;
        FullScreenChanged?.Invoke(isFullScreen);
        PlayerPrefs.SetInt(FullScreenKey, isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
        ApplyResolution();
    }

    private void ApplyResolution()
    {
        var res = SelectedResolutionList[selectResolution];
        Screen.SetResolution(res.width, res.height, isFullScreen);
    }
}
