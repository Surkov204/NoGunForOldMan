using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunInventory : MonoBehaviour
{
    [SerializeField, Range(1, 8)] private int maxSlots = 4;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private GameObject emptyHandPrefab;
    [Header("Grenade Settings")]
    [SerializeField] private int grenadeCount;
    [SerializeField] private int maxGrenade = 10;
    public int GrenadeCount => grenadeCount;
    private readonly List<string> ownedIds = new();
    private readonly List<GameObject> spawned = new();
    public int CurrentIndex { get; private set; } = -1;

    public event Action<GameObject, GameObject> OnWeaponChanged;

    private void Awake()
    {
        if (emptyHandPrefab)
        {
            GameObject hand = Instantiate(emptyHandPrefab, weaponHolder);
            hand.SetActive(false);
            ownedIds.Add("EmptyHand");
            spawned.Add(hand);
        }
        SelectGun(0);
    }

    public bool AddGunPrefab(GameObject gunPrefab)
    {
        if (!gunPrefab) return false;

        string id = ExtractId(gunPrefab);
        int exist = ownedIds.IndexOf(id);
        if (exist >= 0) { SelectGun(exist); return true; }  

        if (spawned.Count >= maxSlots) return false;

        GameObject inst = Instantiate(gunPrefab, weaponHolder);
        inst.SetActive(false);

        ownedIds.Add(id);
        spawned.Add(inst);

        if (CurrentIndex == -1) SelectGun(0);
        return true;
    }

    private string ExtractId(GameObject prefab)
    {
        return prefab.name;
    }

    public void SwitchToEmptyHand()
    {
        SelectGun(0); 
    }

    public void SelectGun(int index)
    {
        if (index < 0 || index >= spawned.Count) return;
        for (int i = 0; i < spawned.Count; i++)
            spawned[i].SetActive(i == index);
        CurrentIndex = index;
    }

    public void SelectNext(int dir)
    {
        if (spawned.Count == 0) return;
        int next = (CurrentIndex + dir + spawned.Count) % spawned.Count;
        SelectGun(next);
    }

    public void RemoveCurrentGun()
    {
        if (CurrentIndex >= 0 && CurrentIndex < spawned.Count)
        {
            spawned.RemoveAt(CurrentIndex);
            ownedIds.RemoveAt(CurrentIndex);
            CurrentIndex = Mathf.Clamp(CurrentIndex - 1, 0, spawned.Count - 1);

            if (spawned.Count > 0)
                SelectGun(CurrentIndex);
            else
                CurrentIndex = -1;
        }
    }

    public bool AddGrenade(GameObject grenadePrefab, int amount)
    {
        if (grenadeCount >= maxGrenade) return false;
        grenadeCount = Mathf.Min(grenadeCount + amount, maxGrenade);

        AddGunPrefab(grenadePrefab);

        Debug.Log($"Grenade picked up: {grenadeCount}/{maxGrenade}");
        return true;
    }

    public bool UseGrenade()
    {
        if (grenadeCount <= 0) return false;
        grenadeCount--;
        Debug.Log($"Grenade thrown, remaining: {grenadeCount}");
        return true;
    }

    public GameObject CurrentGO() =>
    (CurrentIndex >= 0 && CurrentIndex < spawned.Count) ? spawned[CurrentIndex] : null;

    public T CurrentComponent<T>() where T : Component =>
        CurrentGO() ? CurrentGO().GetComponent<T>() : null;
}