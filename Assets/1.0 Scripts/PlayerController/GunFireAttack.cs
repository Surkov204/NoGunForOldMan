using System;
using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class GunFireAttackState
{
    public string weaponName;
    public float currentAmmo;
    public float havingAmmo;
}

public class GunFireAttack : MonoBehaviour, ISaveable
{
    [Header("Weapon Config")]
    [SerializeField] private DataWeapon currentWeapon;
    [SerializeField] public float maxBullet = 12f;

    [Header("Fire Settings")]
    [SerializeField] private float attackCoolDown;
    [SerializeField] private Transform firepoint;
    [SerializeField] private Transform Player;
    [SerializeField] private Transform GunScale;
    [SerializeField] private GameObject[] Bullets;

    [Header("Shell Eject")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private Transform shellEjectPoint;
    [SerializeField] private float ejectForce = 50f;
    [SerializeField] public Vector3 ejectDirection = new Vector3(-0.5f, 1f, 0f);

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI currentBulletText;
    [SerializeField] private TextMeshProUGUI currentBulletHavingText;
    [SerializeField] private GameObject ReloadText;
    [SerializeField] private GameObject ReloadingText;

    [Header("Audio")]
    [SerializeField] private AudioClip GunSoundEffect;
    [SerializeField] private AudioClip ReloadSound;

    private float coolDownTimer = Mathf.Infinity;
    private bool isReloading = false;
    private bool isRestored = false;
    private string uniqueID;

    public float currentBullet { get; private set; }
    public float havingAmmo { get; private set; }
    public bool IsReloading => isReloading;

    private bool restoredFromSave = false;

    // ===== INIT =====
    private void Awake()
    {
        if (!isRestored && SaveManager.SkipLoad)
        {
            ResetState();
        }
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(uniqueID))
            uniqueID = BuildUniqueId();

        if (string.IsNullOrWhiteSpace(uniqueID))
            uniqueID = $"Gun_{gameObject.name}_{GetInstanceID()}";

        Debug.Log($"[GunFireAttack] Built ID: {uniqueID}");

        if (!restoredFromSave)
        {
            ResetState();
        }
        else
        {
            UpdateBulletTextUI();
        }
    }

    private void OnEnable()
    {
        if (currentWeapon.imageWeaponGameObject)
            currentWeapon.imageWeaponGameObject.SetActive(true);
        if (currentWeapon.bulletBarWeapon)
            currentWeapon.bulletBarWeapon.SetActive(true);

        UpdateBulletTextUI();
    }

    private void OnDisable()
    {
        if (currentWeapon.imageWeaponGameObject)
            currentWeapon.imageWeaponGameObject.SetActive(false);
        if (currentWeapon.bulletBarWeapon)
            currentWeapon.bulletBarWeapon.SetActive(false);
    }

    // ===== SAVE SYSTEM =====

    private string BuildUniqueId()
    {
        string weaponId = currentWeapon && !string.IsNullOrWhiteSpace(currentWeapon.weaponName)
       ? currentWeapon.weaponName
       : gameObject.name;

        return $"Gun_{weaponId}_{GetInstanceID()}";
    }

    public string GetUniqueId()
    {
        if (string.IsNullOrWhiteSpace(uniqueID))
            uniqueID = BuildUniqueId();
        return uniqueID;
    }

    public Type GetSaveType() => typeof(GunFireAttackState);

    public object CaptureState()
    {
        return new GunFireAttackState
        {
            weaponName = currentWeapon.weaponName,
            currentAmmo = currentBullet,
            havingAmmo = havingAmmo
        };
    }

    public void RestoreState(object state)
    {
        var s = (GunFireAttackState)state;
        if (s.weaponName == currentWeapon.weaponName)
        {
            currentBullet = s.currentAmmo;
            havingAmmo = s.havingAmmo;
            isRestored = true;
            restoredFromSave = true;
            Debug.Log($"[GunFireAttack] Restored {currentWeapon.weaponName}: {currentBullet}/{havingAmmo}");
            UpdateBulletTextUI();
        }
    }

    public void ResetState()
    {
        if (restoredFromSave) return;
        currentBullet = currentWeapon.maxAmmo;
        havingAmmo = currentWeapon.HavingAmmo;
        isRestored = true;
        UpdateBulletTextUI();
    }

    public void GetBullet() {
        Debug.Log($"[GunFireAttack] GetBullet {currentWeapon.weaponName}: {currentBullet}/{havingAmmo}");
    }

    // ===== GAMEPLAY =====
    private void Update()
    {
        if (Time.timeScale % 60 == 0)
        GetBullet();

        if (SimplePieMenu.PieMenuGlobals.IsChoosingWeapon) return;
        if (UIManager.Instance.IsVisible(JS.UIName.PauseGameScreen) ||
            UIManager.Instance.IsVisible(JS.UIName.GameSave)) return;

        if (Input.GetKeyDown(KeyCode.R) &&
            currentBullet < currentWeapon.maxAmmo &&
            !isReloading && havingAmmo > 0)
        {
            StartCoroutine(Reload());
        }

        if (Input.GetMouseButton(0) && coolDownTimer > currentWeapon.fireRate && !isReloading)
        {
            if (currentBullet > 0)
            {
                Attack();
                currentBullet--;
                UpdateBulletTextUI();
                coolDownTimer = 0;
            }
            else
            {
                ReloadText?.SetActive(true);
            }
        }

        coolDownTimer += Time.deltaTime;
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        ReloadingText?.SetActive(true);
        AudioManager.Instance.PlaySound(ReloadSound);

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        float need = currentWeapon.maxAmmo - currentBullet;
        float load = Mathf.Min(need, havingAmmo);

        currentBullet += load;
        havingAmmo -= load;

        UpdateBulletTextUI();
        ReloadText?.SetActive(false);
        ReloadingText?.SetActive(false);
        isReloading = false;
    }

    private void UpdateBulletTextUI()
    {
        if (currentBulletText) currentBulletText.text = currentBullet.ToString();
        if (currentBulletHavingText) currentBulletHavingText.text = havingAmmo.ToString();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("ArmorRevolver") && currentWeapon.weaponName == "Revolver")
            AddAmmo(col, 12);
        else if (col.CompareTag("ArmorAk47") && currentWeapon.weaponName == "ak47")
            AddAmmo(col, 42);
        else if (col.CompareTag("ArmorShotGun") && currentWeapon.weaponName == "ShotGun")
            AddAmmo(col, 6);
    }

    private void AddAmmo(Collider2D col, int amount)
    {
        col.gameObject.SetActive(false);
        havingAmmo += amount;
        UpdateBulletTextUI();
    }

    private void Attack()
    {
        AudioManager.Instance.PlaySound(GunSoundEffect);

        var shell = Instantiate(shellPrefab, shellEjectPoint.position, shellEjectPoint.rotation);
        if (shell.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector3 dir = shellEjectPoint.TransformDirection(ejectDirection.normalized);
            rb.AddForce(dir * ejectForce, ForceMode2D.Impulse);
            rb.AddTorque(UnityEngine.Random.Range(-200f, 200f));
        }
        Destroy(shell, 20f);

        int idx = FindFireball();
        Bullets[idx].transform.position = firepoint.position;
        Bullets[idx].GetComponent<ProjectTile>().SetDirection(Mathf.Sign(Player.localScale.x), GunScale);
    }

    private int FindFireball()
    {
        for (int i = 0; i < Bullets.Length; i++)
            if (!Bullets[i].activeInHierarchy) return i;
        return 0;
    }
}
