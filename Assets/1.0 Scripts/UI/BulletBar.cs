using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BulletBar : MonoBehaviour
{

    [SerializeField] private PlayerGunInventory inventory; 
    [SerializeField] private Image fill;                  
    [SerializeField] private bool smooth = true;
    [SerializeField] private float lerpSpeed = 10f;

    private GunFireAttack gun;   

    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<PlayerGunInventory>();
        if (!fill)
        {
            var imgs = GetComponentsInChildren<Image>(true);
            foreach (var img in imgs) if (img.type == Image.Type.Filled) { fill = img; break; }
        }
    }

    void OnEnable()
    {
        TryBindToCurrentGun(true);
        try { inventory.OnWeaponChanged += OnWeaponChanged; } catch { /* không có event cũng ok */ }
    }

    void OnDisable()
    {
        try { inventory.OnWeaponChanged -= OnWeaponChanged; } catch { }
        gun = null;
    }

    void Update()
    {
        if (gun == null || gun.gameObject != inventory.CurrentGO())
            TryBindToCurrentGun(false);

        UpdateFill(Time.deltaTime);
    }

    private void OnWeaponChanged(GameObject oldGO, GameObject newGO)
    {
        gun = newGO ? newGO.GetComponent<GunFireAttack>() : null;
        UpdateFill(0f, instant: true);
    }

    private void TryBindToCurrentGun(bool instant)
    {
        var go = inventory ? inventory.CurrentGO() : null;
        gun = go ? go.GetComponent<GunFireAttack>() : null;
        UpdateFill(0f, instant: instant);
    }

    private void UpdateFill(float dt, bool instant = false)
    {
        if (!fill) return;

        float cur = (gun ? gun.currentBullet : 0f);
        float max = (gun ? Mathf.Max(1f, gun.maxBullet) : 1f);
        float target = Mathf.Clamp01(cur / max);

        if (instant || !smooth) fill.fillAmount = target;
        else fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, lerpSpeed * dt);
    }
}
