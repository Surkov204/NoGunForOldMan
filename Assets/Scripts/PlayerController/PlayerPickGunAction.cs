using UnityEngine;

public class PlayerPickGunAction : MonoBehaviour
{
    [Header("Pick")]
    [SerializeField] string weaponId;
    [SerializeField] GunPrefabHandle handle;
    [SerializeField] KeyCode pickKey = KeyCode.E;

    [Header("Range")]
    [SerializeField] float enterRadius = 1.6f;
    [SerializeField] float exitRadius = 1.9f;
    [SerializeField] Transform rangeCenter;
    [SerializeField] LayerMask playerMask;

    [Header("UI")]
    [SerializeField] GameObject pressEHint;

    static Transform s_Player;
    static PlayerGunInventory s_Inv;
    static PlayerPickGunAction s_Focus;
    static float s_FocusDist2;

    bool inRange, hintOn;

    void Awake()
    {
        if (!rangeCenter) rangeCenter = transform;
        ResolvePlayerIfNeeded(true);
    }

    void OnDisable()
    {
        if (s_Focus == this) { s_Focus = null; }
        SetHint(false);
    }

    void Update()
    {
        if (!handle) return;

        if (!s_Player) ResolvePlayerIfNeeded(false);
        if (!s_Player || !s_Inv) return;

        if (((1 << s_Player.gameObject.layer) & playerMask) == 0)
        {
            if (s_Focus == this) s_Focus = null;
            SetHint(false);
            return;
        }

        float r = (s_Focus == this || inRange) ? exitRadius : enterRadius;
        float d2 = (rangeCenter.position - s_Player.position).sqrMagnitude;
        inRange = d2 <= r * r;

        if (inRange)
        {
            if (s_Focus == null || d2 < s_FocusDist2)
            {
                if (s_Focus && s_Focus != this) s_Focus.SetHint(false);
                s_Focus = this;
                s_FocusDist2 = d2;
                SetHint(true);
            }
            else if (s_Focus != this)
            {
                SetHint(false);
            }
        }
        else
        {
            if (s_Focus == this) s_Focus = null;
            SetHint(false);
        }

        if (s_Focus == this && Input.GetKeyDown(pickKey))
        {
            bool ok = handle.TransferToInventory(weaponId, s_Inv);
            SetHint(false);
            if (ok) Destroy(gameObject);
            else Debug.Log("Cannot find prefab by ID or inventory full");
        }
    }

    void SetHint(bool show)
    {
        if (pressEHint && hintOn != show)
        {
            pressEHint.SetActive(show);
            hintOn = show;
        }
    }

    void ResolvePlayerIfNeeded(bool immediate)
    {
#if UNITY_2023_1_OR_NEWER
        var inv = FindFirstObjectByType<PlayerGunInventory>(FindObjectsInactive.Exclude);
#else
        var inv = FindObjectOfType<PlayerGunInventory>();
#endif
        if (inv && (((1 << inv.gameObject.layer) & playerMask) != 0))
        {
            s_Player = inv.transform;
            s_Inv = inv;
            return;
        }

        const float R = 60f;
        var hits = new Collider2D[8];
        int n = Physics2D.OverlapCircleNonAlloc(transform.position, R, hits, playerMask);
        for (int i = 0; i < n; i++)
        {
            var p = hits[i] ? hits[i].GetComponentInParent<PlayerGunInventory>() : null;
            if (p)
            {
                s_Player = p.transform;
                s_Inv = p;
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        var c = rangeCenter ? rangeCenter.position : transform.position;
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(c, enterRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(c, exitRadius);
    }
}
