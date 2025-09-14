using JS;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "Scriptable Objects/UI Config", order = 0)]
public class UIConfig : ScriptableObject
{
    [Serializable]
    public class UIPair
    {
        public UIName key;
        public GameObject prefab;
    }

    [SerializeField] private List<UIPair> uiList = new();

    private Dictionary<UIName, GameObject> dict;

    public GameObject GetPrefab(UIName name)
    {
        if (dict == null)
        {
            dict = new Dictionary<UIName, GameObject>();
            foreach (var pair in uiList)
            {
                if (pair.prefab != null && !dict.ContainsKey(pair.key))
                    dict[pair.key] = pair.prefab;
            }
        }

        dict.TryGetValue(name, out var prefab);
        return prefab;
    }
}
