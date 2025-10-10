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
            Debug.Log("saving");
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
        }
    }
}
