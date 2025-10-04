using UnityEngine;
using System.Collections.Generic;
using System;

public class GunPrefabHandle : MonoBehaviour
{
    [SerializeField] private List<GameObject> gunPrefabs = new();
    private Dictionary<string, GameObject> byId = new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        byId.Clear();
        foreach (var gunPrefab in gunPrefabs) {
            if (!gunPrefab) continue;

            var name = gunPrefab.name;
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning($"[GunPrefabHandle] Skipped prefab with null/empty name in list: {gunPrefab}");
                continue;
            }

            if (!byId.ContainsKey(name))
            {
                byId.Add(name, gunPrefab);
                Debug.Log($"[GunPrefabHandle] Registered prefab '{name}'");
            }
            else
            {
                Debug.LogWarning($"[GunPrefabHandle] Duplicate prefab name '{name}' skipped.");
            }
        }
    }

    private GameObject FindGunPrefabById(string weaponID) {
        if (string.IsNullOrWhiteSpace(weaponID)) return null;
        else
            return byId.TryGetValue(weaponID, out var prefab) ? prefab : null;
    }

    public bool TransferToInventory(string weaponID, PlayerGunInventory inventory) {
        var prefab = FindGunPrefabById(weaponID);
        if (!prefab || !inventory) return false;
        return inventory.AddGunPrefab(prefab);
    }
    public bool TransferGrenadeToInventory(GameObject grenadePrefab, PlayerGunInventory inventory, int amount = 1)
    {
        if (!grenadePrefab || !inventory) return false;
        return inventory.AddGrenade(grenadePrefab, amount);
    }

}
