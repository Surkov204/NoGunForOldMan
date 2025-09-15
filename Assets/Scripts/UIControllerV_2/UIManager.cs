using JS.Utils;
using JS;
using System.Collections.Generic;
using UnityEngine;

public enum CanvasType
{
    FullScreen,
    HUD,
    Popup,
    Loading
}

public class UIManager : ManualSingletonMono<UIManager>
{
    [SerializeField] private UIConfigExtend uiConfig;
    [SerializeField] private Transform uiRoot;

    private UIBase currentFullScreen;
    private UIBase currentHUD;
    private Stack<UIBase> popupStack = new();
    private Dictionary<UIName, UIBase> spawned = new();

    public bool HasPopupOnTop => popupStack.Count > 0;

    private UIBase GetOrSpawn(UIName name, out CanvasType canvasType)
    {
        var pair = uiConfig.Get(name);
        if (pair == null)
        {
            Debug.LogWarning($"[UiManager] No config found for {name}");
            canvasType = CanvasType.FullScreen;
            return null;
        }

        canvasType = pair.canvasType;

        if (spawned.TryGetValue(name, out var ui) && ui != null)
            return ui;

        var instance = Instantiate(pair.prefab, uiRoot);
        var baseUI = instance.GetComponent<UIBase>();
        spawned[name] = baseUI;
        return baseUI;
    }

    public void Show(UIName name)
    {
        var ui = GetOrSpawn(name, out var canvasType);
        if (ui == null) return;

        switch (canvasType)
        {
            case CanvasType.FullScreen:
                if (currentFullScreen != null) currentFullScreen.OnHide();
                currentFullScreen = ui;
                break;

            case CanvasType.HUD:
                if (currentHUD != null) currentHUD.OnHide();
                currentHUD = ui;
                break;

            case CanvasType.Popup:
                popupStack.Push(ui);
                break;

            case CanvasType.Loading:
                break;
        }

        ui.OnShow();
        UpdateTimeScale();
    }

    public void Hide(UIName name)
    {
        if (!spawned.TryGetValue(name, out var ui)) return;

        var pair = uiConfig.Get(name);
        if (pair == null) return;

        switch (pair.canvasType)
        {
            case CanvasType.FullScreen:
                if (currentFullScreen == ui) currentFullScreen = null;
                break;

            case CanvasType.HUD:
                if (currentHUD == ui) currentHUD = null;
                break;

            case CanvasType.Popup:
                if (popupStack.Count > 0 && popupStack.Peek() == ui)
                    popupStack.Pop();
                break;
        }

        ui.OnHide();
        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        if (spawned.TryGetValue(UIName.PauseGameScreen, out var pauseUI)
            && pauseUI.IsVisible)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public bool IsVisible(UIName name)
    {
        if (spawned.TryGetValue(name, out var ui))
        {
            return ui.IsVisible;
        }
        return false;
    }

    public void Back()
    {
        if (popupStack.Count > 0)
        {
            var top = popupStack.Pop();
            top.OnHide();
            UpdateTimeScale();
        }
    }
}
