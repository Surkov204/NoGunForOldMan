using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private DataWeapon currentWeapon;

    private void OnEnable()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon.imageWeaponGameObject != null)
                currentWeapon.imageWeaponGameObject.SetActive(true);

            if (currentWeapon.bulletBarWeapon != null)
                currentWeapon.bulletBarWeapon.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon.imageWeaponGameObject != null)
                currentWeapon.imageWeaponGameObject.SetActive(false);

            if (currentWeapon.bulletBarWeapon != null)
                currentWeapon.bulletBarWeapon.SetActive(false);
        }
    }

}
