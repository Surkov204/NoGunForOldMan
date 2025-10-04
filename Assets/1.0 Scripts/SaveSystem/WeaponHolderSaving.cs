using UnityEngine;

public class WeaponHolderSaving : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[WeaponHolderSaving] Awake " + name);
    }

    private void Start()
    {
        Debug.Log("saving 1");
        var guns = GetComponentsInChildren<GunFireAttack>(includeInactive: true);
        foreach (var g in guns)
        {

            SaveManager.Instance?.Registry(g);
            Debug.Log("saving");
            Debug.Log($"[SaveBinder] Registered gun: {g.GetUniqueId()}");
        }
    }

    private void OnDisable()
    {
        Debug.Log("[WeaponHolderSaving] OnDisable called before Start: " + name);
    }

    private void OnDestroy()
    {
        if (!SaveManager.HasInstance) return;

        var guns = GetComponentsInChildren<GunFireAttack>(includeInactive: true);
        foreach (var g in guns)
        {
            SaveManager.Instance.UnRegistry(g);
        }
    }
}
