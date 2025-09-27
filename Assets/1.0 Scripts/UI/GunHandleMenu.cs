using NUnit;
using SimplePieMenu;
using UnityEngine;
using System.Collections;

public class GunHandleMenu : MonoBehaviour, IMenuItemClickHandler
{
    [SerializeField] private PieMenu pieMenu;
    [SerializeField] private PlayerGunInventory inventory;

    private readonly string[] weaponMap = { "Revolver", "Ak47", "ShotGun", "HandleReviewGrenade", "EmptyHand"};

    public void Handle()
    {
        PieMenuGlobals.IsChoosingWeapon = true;


        var cur = inventory.CurrentGO();
        var gfa = cur ? cur.GetComponent<GunFireAttack>() : null;
        if (gfa && gfa.IsReloading) return;

        PieMenuItem item = GetComponent<PieMenuItem>();
        if (item == null || inventory == null) return;

        int id = item.Id;
        if (id < 0 || id >= weaponMap.Length) return;

        string weaponID = weaponMap[id];

        int slotIndex = FindWeaponInInventory(weaponID);
        if (slotIndex >= 0)
        {
            inventory.SelectGun(slotIndex);
            Debug.Log($"{weaponID} (inventory slot {slotIndex}) and (is selection {PieMenuGlobals.IsChoosingWeapon})");
        }
        else
        {
            Debug.LogWarning($"not found {weaponID} in inventory!");
        }

        StartCoroutine(ResetFlag());
    }

    private IEnumerator ResetFlag()
    {
        yield return new WaitForSeconds(0.5f);
        PieMenuGlobals.IsChoosingWeapon = false;
    }

    private int FindWeaponInInventory(string weaponID)
    {
        var current = inventory.CurrentGO();
        for (int i = 0; i < 8; i++) 
        {
            var gun = inventory.CurrentGO();
            if (i < inventory.CurrentIndex || inventory.CurrentGO() == null) continue;
        }

        return inventory.FindIndexById(weaponID);
    }
}
