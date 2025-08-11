using System;
using System.Collections.Generic;
using UnityEngine;
using JS.Utils;
using System.Buffers.Text;
using System.Xml.Linq;

namespace JS
{
    [Serializable]
    public class UIPair
    {
        public UIName key;
        public string resourceName;
    }

    public class UiManager : ManualSingletonMono<UiManager>
    {
        [Header("UI Root")]
        [SerializeField] private Transform uiTransform;

        [Header("Prefab Mapping")]
        [SerializeField] private List<UIPair> prefabList;

        private Dictionary<UIName, string> prefabNameDict = new();
        private Dictionary<UIName, BaseUI> spawnedUI = new();

        protected override void Awake()
        {
            base.Awake();

            foreach (var pair in prefabList)
            {
                if (!string.IsNullOrEmpty(pair.resourceName) && !prefabNameDict.ContainsKey(pair.key))
                {
                    prefabNameDict[pair.key] = pair.resourceName;
                }
            }
        }

        public void ShowUI(UIName ui)
        {
            if (spawnedUI.TryGetValue(ui, out var instance))
            {
                if (instance != null)
                {
                    instance.gameObject.SetActive(true);
                    instance.OnShowScreen();
                    return;
                }
                else
                {
                    spawnedUI.Remove(ui);
                }
            }

            if (!prefabNameDict.TryGetValue(ui, out var prefabName))
            {
                Debug.LogWarning($"[UiManager] No resource name mapped for UI: {ui}");
                return;
            }

            string path = $"UI/{prefabName}";
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[UiManager] Could not load prefab at path: {path}");
                return;
            }

            GameObject newUI = Instantiate(prefab, uiTransform);
            BaseUI baseUI = newUI.GetComponent<BaseUI>();
            if (baseUI == null)
            {
                Debug.LogWarning($"[UiManager] UI prefab at '{path}' is missing BaseUI.");
                return;
            }

            spawnedUI[ui] = baseUI;
            baseUI.OnShowScreen();
        }

        public void HideUI(UIName uiName)
        {
            if (spawnedUI.TryGetValue(uiName, out var ui))
            {
                ui.gameObject.SetActive(false);
                ui.OnCloseScreen();
            }
        }

        // Optional
        // public void HideAll()
        // {
        //     foreach (var go in spawnedUI.Values)
        //     {
        //         go.gameObject.SetActive(false);
        //     }
        // }
    }
}
