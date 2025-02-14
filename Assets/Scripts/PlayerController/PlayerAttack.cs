using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCoolDown;
    [SerializeField] private Transform firepoint;
    [SerializeField] private Transform Player;
    [SerializeField] private Transform GunScale;
    [SerializeField] private GameObject[] Bullets;
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] Transform shellEjectPoint;
    [SerializeField] float ejectForce =50f;
    [SerializeField] public Vector3 ejectDirection = new Vector3(-0.5f, 1f, 0f);
    [SerializeField] private TextMeshProUGUI currentBulletText;
    [SerializeField] private TextMeshProUGUI currentBulletHavingText;
    [SerializeField] private GameObject ReloadText;
    [SerializeField] private GameObject ReloadingText;
    [SerializeField] private float BulletHaving;
    [SerializeField] private DataWeapon[] weapons;
    [SerializeField] private float reloadTime = 1;
    [SerializeField] public float maxBullet = 12f;
    [SerializeField] private AudioClip GunSoundEffect;
    [SerializeField] private AudioClip ReloadSound;

    private float _BulletHaving;
    private float BulletCounting = 0;
    private bool isReloading = false;
    public float currentBullet { get; private set; }
    private int currentWeaponIndex = 0;
    private int weaponSelect = 0;
    private DataWeapon currentWeapon;


    private PlayerMoverment playerMoverment;
    private float coolDownTimer = Mathf.Infinity;

    private void Awake()
    {
        playerMoverment = GetComponent<PlayerMoverment>();
    }
    private void Start()
    {
        currentBullet = maxBullet;
        SwitchWeapon(weaponSelect);
        _BulletHaving = currentWeapon.HavingAmmo;

    }
    private void Update()
    {

        if (Input.GetKey(KeyCode.Alpha1))
        {
            weaponSelect = 0;
            SwitchWeapon(weaponSelect);
            _BulletHaving = currentWeapon.HavingAmmo;
        }
        if (Input.GetKey(KeyCode.Alpha2) && weapons.Length > 1)
        {
            weaponSelect = 1;
            SwitchWeapon(weaponSelect);
            _BulletHaving = currentWeapon.HavingAmmo;
        }

      //  UpdateUI();
        if (currentWeapon.HavingAmmo != 0 || currentWeapon.currentAmmor != 0)
        {
            if (Input.GetKeyDown(KeyCode.R) && currentWeapon.currentAmmor >= 0 && currentWeapon.currentAmmor < currentWeapon.maxAmmo && !isReloading && currentWeapon.HavingAmmo!= 0)
            {
                UpdateUI();
                StartCoroutine(Reload());
            }

            if (Input.GetMouseButton(0) && coolDownTimer > currentWeapon.fireRate && !isReloading)
            {
                if (currentWeapon.currentAmmor > 0)
                {
                    
                    attack();
                    currentWeapon.currentAmmor--;
                    currentBullet = currentWeapon.currentAmmor;              
                    UpdateUI();
                    coolDownTimer = 0;
                }
                else
                {
                    ReloadText.SetActive(true);
                }
            }
            coolDownTimer += Time.deltaTime;
        }
        else
        {
           
        }
      
    }
    IEnumerator Reload()
    {
        isReloading = true;
        // waiting for reloading //
        ReloadingText.SetActive(true);
        Debug.Log(currentWeapon.weaponName);

        AudioManager.instance.PlaySound(ReloadSound);
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        if (currentWeapon.currentAmmor >= 0 && currentWeapon.currentAmmor < currentWeapon.maxAmmo && currentWeapon.HavingAmmo >= currentWeapon.maxAmmo)
        {
            float remainding = currentWeapon.maxAmmo - currentWeapon.currentAmmor;
            currentWeapon.HavingAmmo = currentWeapon.HavingAmmo - remainding;
            currentWeapon.currentAmmor = currentWeapon.currentAmmor + remainding;
            currentBullet = currentWeapon.currentAmmor;
        } 
        else
        {
            currentWeapon.currentAmmor = currentWeapon.currentAmmor + currentWeapon.HavingAmmo;
            currentBullet = currentWeapon.currentAmmor;
            if (currentWeapon.currentAmmor > currentWeapon.maxAmmo)
            {
                currentWeapon.HavingAmmo = currentWeapon.currentAmmor - maxBullet;
                currentWeapon.currentAmmor = currentWeapon.maxAmmo;
                currentBullet = currentWeapon.currentAmmor;
            } else currentWeapon.HavingAmmo = 0;

        }



        UpdateUI();
        ReloadText.SetActive(false);
        //  reload successufull // 
   
        ReloadingText.SetActive(false);
        isReloading = false;
    
    }

    private void SwitchWeapon(int weaponIndex)
    {
      //  UpdateUI();

        if (currentWeapon != null && currentWeapon.weaponGameObject.activeSelf)
        {
           
            currentWeapon.weaponGameObject.SetActive(false);
            currentWeapon.imageWeaponGameObject.SetActive(false);
            currentWeapon.bulletBarWeapon.SetActive(false);
            if (!currentWeapon.weaponGameObject.activeInHierarchy)
            {
           
            }

        } 
        
        // Gán vũ khí mới
        currentWeaponIndex = weaponIndex;
        currentWeapon = weapons[currentWeaponIndex];
        currentWeapon.weaponGameObject.SetActive(true);
        currentWeapon.imageWeaponGameObject.SetActive(true);
        currentWeapon.bulletBarWeapon.SetActive(true);
        UpdateUI();
    }
    private void UpdateUI()
    {
        currentBulletText.text = currentWeapon.currentAmmor.ToString();
        currentBulletHavingText.text = currentWeapon.HavingAmmo.ToString();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Armor")
        {
     
            currentWeapon = weapons[0];
            currentWeapon.HavingAmmo += 12;
   
            if (currentWeapon != null && currentWeapon.weaponName == "Revolver" && currentWeapon.weaponGameObject.activeInHierarchy)
            {
                Debug.Log(currentWeapon.weaponName);
                currentBulletHavingText.text = currentWeapon.HavingAmmo.ToString();
            }
            else
            {
                Debug.Log("Object null");
            }
            collision.gameObject.SetActive(false);
        }

        if (collision.tag == "ArmorAk47")
        {

            currentWeapon = weapons[1];
            currentWeapon.HavingAmmo += 42;

            if (currentWeapon != null && currentWeapon.weaponName == "ak47" && currentWeapon.weaponGameObject.activeInHierarchy)
            {
                Debug.Log(currentWeapon.weaponName);
                currentBulletHavingText.text = currentWeapon.HavingAmmo.ToString();
            }
            else
            {
                Debug.Log("Object null");
            }
            collision.gameObject.SetActive(false);
        }
    }

    private void attack()
    {
        AudioManager.instance.PlaySound(GunSoundEffect);
        GameObject shell = Instantiate(shellPrefab, shellEjectPoint.position, shellEjectPoint.rotation);

        Rigidbody2D shellRb = shell.GetComponent<Rigidbody2D>();
        
        if (shellRb != null)
        {
          
            Vector3 forceDirection = shellEjectPoint.TransformDirection(ejectDirection.normalized);
            shellRb.AddForce(forceDirection * ejectForce, ForceMode2D.Impulse);

            float randomTorque = Random.Range(-200f, 200f); // Xoay ngẫu nhiên
            shellRb.AddTorque(randomTorque);
        }


        Destroy(shell, 20f);

        Bullets[FindFireball()].transform.position = firepoint.position;
        Bullets[FindFireball()].GetComponent<ProjectTile>().SetDirection(Mathf.Sign(Player.localScale.x), GunScale);
    }

    private int FindFireball()
    {
        for (int i = 0; i < Bullets.Length; i++)
        {
            if (!Bullets[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
