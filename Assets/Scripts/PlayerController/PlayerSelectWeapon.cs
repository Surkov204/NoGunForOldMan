using UnityEngine;

public class PlayerSelectWeapon : MonoBehaviour
{
    [SerializeField] private PlayerGunInventory inv;

    void Update()
    {
        var cur = inv.CurrentGO();
        var gfa = cur ? cur.GetComponent<GunFireAttack>() : null;
        if (gfa && gfa.IsReloading) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) inv.SelectGun(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inv.SelectGun(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inv.SelectGun(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inv.SelectGun(3);

        float s = Input.GetAxis("Mouse ScrollWheel");
        if (s > 0f) inv.SelectNext(+1);
        else if (s < 0f) inv.SelectNext(-1);
    }
}