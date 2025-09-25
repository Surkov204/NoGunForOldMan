using JS.Utils;
using JS;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    private void Start()
    {
        foreach (var pair in uiConfig.GetAll())
        {
            if (pair.preload && pair.prefab != null)
            {
                var instance = Instantiate(pair.prefab, uiRoot);
                var baseUI = instance.GetComponent<UIBase>();
                baseUI.SetUIName(pair.key);
                spawned[pair.key] = baseUI;
                baseUI.gameObject.SetActive(false);
            }
        }
    }

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
        var pair = uiConfig.Get(name);
        switch (canvasType)
        {
            case CanvasType.FullScreen:
                if (currentFullScreen != null) currentFullScreen.OnHide(pair.animationType);
                currentFullScreen = ui;
                break;

            case CanvasType.HUD:
                if (currentHUD != null) currentHUD.OnHide(pair.animationType);
                currentHUD = ui;
                break;

            case CanvasType.Popup:
                popupStack.Push(ui);
                break;

            case CanvasType.Loading:
                break;
        }

        ui.OnShow(pair.animationType);
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

        ui.OnHide(pair.animationType);

        if (pair.destroyAfterHide)
        {
            DOVirtual.DelayedCall(0.35f, () =>
            {
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                    spawned.Remove(name);
                }
            }, ignoreTimeScale: true);
        }
        UpdateTimeScale();
    }

    public void HideAll()
    {
        if (currentFullScreen != null)
        {
            var pair = uiConfig.Get(currentFullScreen.UIName);
            var type = pair != null ? pair.animationType : UIAnimationType.FadeScale;
            currentFullScreen.OnHide(type);

            if (pair != null && pair.destroyAfterHide)
            {
                Destroy(currentFullScreen.gameObject);
                spawned.Remove(currentFullScreen.UIName);
            }

            currentFullScreen = null;
        }

        if (currentHUD != null)
        {
            var pair = uiConfig.Get(currentHUD.UIName);
            var type = pair != null ? pair.animationType : UIAnimationType.FadeScale;
            currentHUD.OnHide(type);

            if (pair != null && pair.destroyAfterHide)
            {
                Destroy(currentHUD.gameObject);
                spawned.Remove(currentHUD.UIName);
            }

            currentHUD = null;
        }

        while (popupStack.Count > 0)
        {
            var popup = popupStack.Pop();
            var pair = uiConfig.Get(popup.UIName);
            var type = pair != null ? pair.animationType : UIAnimationType.FadeScale;
            popup.OnHide(type);

            if (pair != null && pair.destroyAfterHide)
            {
                Destroy(popup.gameObject);
                spawned.Remove(popup.UIName);
            }
        }

        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        bool shouldPause = false;

        foreach (var kvp in spawned)
        {
            var pair = uiConfig.Get(kvp.Key);
            if (pair != null && pair.pauseOnShowing && kvp.Value.IsVisible)
            {
                shouldPause = true;
                break;
            }
        }

        if (shouldPause)
            PauseManager.Pause();
        else
            PauseManager.Resume();

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
            var pair = uiConfig.Get(top.UIName); 
            var type = pair != null ? pair.animationType : UIAnimationType.FadeScale;
            top.OnHide();
            UpdateTimeScale();
        }
    }
}
