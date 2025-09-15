using JS;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "Scriptable Objects/UI ConfigExtend", order = 0)]
public class UIConfigExtend : ScriptableObject
{
    [Serializable]
    public class UIPairExtend
    {
        public UIName key;
        public GameObject prefab;
        public CanvasType canvasType; 
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
}
