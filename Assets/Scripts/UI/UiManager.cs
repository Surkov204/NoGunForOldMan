using System;
using System.Collections.Generic;
using UnityEngine;
using JS.Utils;
using System.Buffers.Text;
using System.Xml.Linq;
using UnityEditor.Overlays;

namespace JS
{
    public enum CanvasType
    {
        FullScreen,
        HUD,
        Popup,
        Loading
    }

    public class UiManager : ManualSingletonMono<UiManager>
    {
        [Header("UI Root")]
        [SerializeField] private Transform uiTransform;

        [Header("UI Config (ScriptableObject)")]
        [SerializeField] private UIConfig uiConfig;

        private BaseUI activeNormalUI;
        private readonly Stack<BaseUI> overlayStack = new();
        private Dictionary<UIName, BaseUI> spawnedUI = new();

        protected override void Awake()
        {
            base.Awake();
        }

        private BaseUI GetOrSpawnUI(UIName ui)
        {
            if (spawnedUI.TryGetValue(ui, out var instance))
            {
                if (instance != null)
                    return instance;
                spawnedUI.Remove(ui);
            }

            GameObject prefab = uiConfig.GetPrefab(ui);
            if (prefab == null)
            {
                return null;
            }

            GameObject newUI = Instantiate(prefab, uiTransform);
            BaseUI baseUI = newUI.GetComponent<BaseUI>();
            if (baseUI == null)
            {
                return null;
            }

            spawnedUI[ui] = baseUI;
            return baseUI;
        }

        public void ShowUI(UIName ui)
        {
            BaseUI baseUI = GetOrSpawnUI(ui);
            if (baseUI == null) return;
            if (baseUI.PopupType == PopupType.Normal)
            {
                if (activeNormalUI != null)
                {
                    activeNormalUI.OnCloseScreen();
                    activeNormalUI.gameObject.SetActive(false);
                    activeNormalUI = null;
                }

                while (overlayStack.Count > 0)
                {
                    var overlay = overlayStack.Pop();
                    overlay.OnCloseScreen();
                    overlay.gameObject.SetActive(false);
                }
                activeNormalUI = baseUI;
                baseUI.gameObject.SetActive(true);
                baseUI.OnShowScreen();
            }
            else if (baseUI.PopupType == PopupType.Overlay) {
                overlayStack.Push(baseUI);
                baseUI.gameObject.SetActive(true);
                baseUI.OnShowScreen();
            }
        }

        public void HideUI(UIName uiName)
        {
            if (!spawnedUI.TryGetValue(uiName, out var ui)) return;
            if (ui.PopupType == PopupType.Normal)
            {
                if (activeNormalUI == ui) activeNormalUI = null;
                ui.OnCloseScreen();
                ui.gameObject.SetActive(false);
            }
            else 
            if (ui.PopupType == PopupType.Overlay) {
                if (overlayStack.Count > 0 && overlayStack.Peek() == ui)
                {
                    overlayStack.Pop();
                    ui.OnCloseScreen();
                    ui.gameObject.SetActive(false);
                }
                else {
                    Debug.Log("overlay by other UI");
                }
            }
        }

        public BaseUI GetUI(UIName uiName)
        {
            if (spawnedUI.TryGetValue(uiName, out var ui))
            {
                return ui;
            }
            return null;
        }

        public T GetUI<T>(UIName uiName) where T : BaseUI
        {
            return GetUI(uiName) as T;
        }

        public bool IsUIShowing(UIName uiName)
        {
            if (spawnedUI.TryGetValue(uiName, out var ui))
            {
                return ui.gameObject.activeSelf;
            }
            return false;
        }
        // Optional
        public void HideAll()
        {
            foreach (var go in spawnedUI.Values)
            {
                go.gameObject.SetActive(false);
            }
        }
    }
}
