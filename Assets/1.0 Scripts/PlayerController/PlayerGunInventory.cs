using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerGunInventoryState {
    public List<string> gunIds = new();
    public int currentIndex;
    public int grenadeCount;
}

public class PlayerGunInventory : MonoBehaviour, ISaveable
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

    private string uniqueID;

    private void Start()
    {
        uniqueID = $"{gameObject.name}_gunID";
        SaveManager.Instance?.Registry(this);
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.UnRegistry(this);
    }

    public string GetUniqueId() => uniqueID;

    public Type GetSaveType() => typeof(PlayerGunInventoryState);

    public object CaptureState() {
        return new PlayerGunInventoryState
        {
            gunIds = new List<string>(ownedIds), 
            currentIndex = CurrentIndex,
            grenadeCount = grenadeCount
        };
    }

    public void RestoreState(object state)
    {
        var s = (PlayerGunInventoryState)state;

        spawned.RemoveAll(go => go == null);
        ownedIds.RemoveAll(id => string.IsNullOrWhiteSpace(id));

        var gunHandle = FindObjectOfType<GunPrefabHandle>();

        foreach (var id in s.gunIds)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[PlayerGunInventory] Skipped empty or null weapon ID in save file.");
                continue;
            }

            if (id == "EmptyHand")
            {
                if (!ownedIds.Contains("EmptyHand") && emptyHandPrefab)
                {
                    var hand = Instantiate(emptyHandPrefab, weaponHolder);
                    hand.SetActive(false);
                    ownedIds.Add("EmptyHand");
                    spawned.Add(hand);
                }
                continue;
            }

            int existIndex = ownedIds.IndexOf(id);
            if (existIndex >= 0)
            {
                var gfa = spawned[existIndex]?.GetComponent<GunFireAttack>();
                if (gfa != null)
                {
                    Debug.Log($"[PlayerGunInventory] Restoring existing gun {id}");
                }
            }
            else
            {
                if (gunHandle != null)
                {
                    bool ok = gunHandle.TransferToInventory(id, this);
                    if (!ok)
                        Debug.LogWarning($"[PlayerGunInventory] Failed to restore weapon '{id}' — prefab not found in GunPrefabHandle.");
                }
            }
        }

        grenadeCount = s.grenadeCount;
        if (spawned.Count > 0 && s.currentIndex >= 0 && s.currentIndex < spawned.Count)
            SelectGun(s.currentIndex);
        else if (spawned.Count > 0)
            SelectGun(0);
    }

    public void ResetState()
    {
        foreach (var g in spawned)
        {
            if (g) Destroy(g);
        }
        spawned.Clear();
        ownedIds.Clear();

        if (emptyHandPrefab)
        {
            GameObject hand = Instantiate(emptyHandPrefab, weaponHolder);
            hand.SetActive(false);
            ownedIds.Add("EmptyHand");
            spawned.Add(hand);
        }

        grenadeCount = 0;
        SelectGun(0);
    }

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

        if (exist >= 0)
        {
            var gfaExist = spawned[exist]?.GetComponent<GunFireAttack>();
            if (gfaExist != null && SaveManager.HasInstance)
            {
                SaveManager.Instance.Registry(gfaExist);
                Debug.Log($"[Inventory] Re-registered existing gun: {gfaExist.GetUniqueId()}");
            }
            SelectGun(exist);
            return true;
        }  

        if (spawned.Count >= maxSlots) return false;

        GameObject inst = Instantiate(gunPrefab, weaponHolder);
        inst.SetActive(false);

        ownedIds.Add(id);
        spawned.Add(inst);

        var gfa = inst.GetComponent<GunFireAttack>();
        if (gfa != null && SaveManager.HasInstance)
        {
            SaveManager.Instance.Registry(gfa);
            Debug.Log($"[Inventory] Registered spawned gun: {gfa.GetUniqueId()}");
        }

        if (CurrentIndex == -1) SelectGun(0);
        return true;
    }

    public bool AddGrenade(GameObject grenadePrefab, int amount)
    {
        if (grenadeCount >= maxGrenade) return false;
        grenadeCount = Mathf.Min(grenadeCount + amount, maxGrenade);

        AddGunPrefab(grenadePrefab);

        Debug.Log($"Grenade picked up: {grenadeCount}/{maxGrenade}");
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
        {
            spawned[i].SetActive(i == index);
        }    
        
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
            GameObject currentGun = spawned[CurrentIndex];

            if (currentGun != null && currentGun.name != "EmptyHand") {

                var gfa = currentGun.GetComponent<GunFireAttack>();
                if (gfa != null && SaveManager.HasInstance)
                {
                    SaveManager.Instance.UnRegistry(gfa);
                    Debug.Log($"[Inventory] Unregistered gun: {gfa.GetUniqueId()}");
                }

                Destroy(currentGun);
            }

            spawned.RemoveAt(CurrentIndex);
            ownedIds.RemoveAt(CurrentIndex);
            CurrentIndex = Mathf.Clamp(CurrentIndex - 1, 0, spawned.Count - 1);

            if (spawned.Count > 0)
                SelectGun(CurrentIndex);
            else
                CurrentIndex = -1;
        }
    }

    public bool UseGrenade()
    {
        if (grenadeCount <= 0) return false;
        grenadeCount--;
        Debug.Log($"Grenade thrown, remaining: {grenadeCount}");
        return true;
    }

    public int FindIndexById(string id) => ownedIds.IndexOf(id);
    
    public GameObject CurrentGO() =>
    (CurrentIndex >= 0 && CurrentIndex < spawned.Count) ? spawned[CurrentIndex] : null;

    public T CurrentComponent<T>() where T : Component =>
        CurrentGO() ? CurrentGO().GetComponent<T>() : null;
}