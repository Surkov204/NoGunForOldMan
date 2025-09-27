using JS;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum UIAnimationType
{
    FadeScale,
    SlideLeft,
    SlideRight,
    SlideTop,
    SlideBottom
}

[CreateAssetMenu(fileName = "UIConfig", menuName = "Scriptable Objects/UI ConfigExtend", order = 0)]
public class UIConfigExtend : ScriptableObject
{
    [Serializable]
    public class UIPairExtend
    {
        public GameObject prefab;
        public UIName key;
        public CanvasType canvasType;
        public UIAnimationType animationType = UIAnimationType.FadeScale;
        public bool preload = false;
        public bool pauseOnShowing = false;
        public bool destroyAfterHide = false;
    }

    [SerializeField] private List<UIPairExtend> uiList = new();

    private Dictionary<UIName, UIPairExtend> dict;

    public UIPairExtend Get(UIName name)
    {
        if (dict == null)
        {
            dict = new Dictionary<UIName, UIPairExtend>();
            foreach (var pairExtend in uiList)
            {
                if (pairExtend.prefab != null && !dict.ContainsKey(pairExtend.key))
                    dict[pairExtend.key] = pairExtend;
            }
        }

        dict.TryGetValue(name, out var pair);
        return pair;
    }

    public List<UIPairExtend> GetAll()
    {
        if (dict == null)
            BuildDict();
        return uiList;
    }

    private void BuildDict()
    {
        dict = new Dictionary<UIName, UIPairExtend>();
        foreach (var pairExtend in uiList)
        {
            if (pairExtend.prefab != null && !dict.ContainsKey(pairExtend.key))
                dict[pairExtend.key] = pairExtend;
        }
    }
}
