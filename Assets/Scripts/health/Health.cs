using JS;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public float startingHealth;
    public float currentHealth { get; private set; }
    private Animator anim;
    private bool dead;

    [Header("Iframe")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private float numberOfFlashes;
    private SpriteRenderer spriteRender;

    [Header("component")]
    [SerializeField] private Behaviour[] components;

    [Header("SoundMangager")]
    [SerializeField] private AudioClip SoundHurt;
    [SerializeField] private AudioClip SoundDie;
    [Header("Decay")]
    [SerializeField] private GameObject DecayObject;

    [Header("ScreenDamaged")]
    [SerializeField] private Animator screenDamage;
    [SerializeField] private Animator shakingCamera;

    [Header("Auto Regen")]
    [SerializeField] private float regenDelay = 5f;
    [SerializeField] private float regenRate = 2f;

    [Header("Alert Bolder")]
    [SerializeField] private Image BoderAlertDamagedTaken;
    [SerializeField] private float fadeSpeed = 2f;
    private Color alertColor;

    [Header("Auto Gen UI Bolder")]
    [SerializeField] private Image BoderAutoReGen;
    [SerializeField] private float fadeAutoGenSpeed;
    private Color autoGenColor;

    private float lastDamageTime;
    private static readonly int Hit = Animator.StringToHash("damageScreen");
    private static readonly int Shaking = Animator.StringToHash("Saking");

    public bool isPlayer;

    private void Awake()
    {
        currentHealth = startingHealth;
        spriteRender = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (BoderAlertDamagedTaken != null)
            alertColor = BoderAlertDamagedTaken.color;
        if (BoderAutoReGen != null)
            autoGenColor = BoderAutoReGen.color;
    }

    private void Update()
    {
        AutoRegen();
        HandleDamagedAlerfFade();
    }

    private void AutoGenHandleBoderFade() {
        if (BoderAutoReGen != null) {
            if (isPlayer && !dead && currentHealth < startingHealth)
            {
                float alpha = Mathf.PingPong(Time.time * fadeAutoGenSpeed, 1f);
                autoGenColor.a = alpha;
                BoderAutoReGen.color = autoGenColor;
            }
            else
            {
                autoGenColor.a = 0;
                BoderAutoReGen.color = autoGenColor;
            }
            
        }
    }

    private void HandleDamagedAlerfFade() {
        if (BoderAlertDamagedTaken != null) {
            if (alertColor.a > 0) {
                alertColor.a -= fadeSpeed * Time.deltaTime;
                alertColor.a = Mathf.Clamp01(alertColor.a);
                BoderAlertDamagedTaken.color = alertColor;
            }
        }
    }

    private void AutoRegen(){
        if (isPlayer && !dead) {
            if ((Time.time - lastDamageTime >= regenDelay) && currentHealth < startingHealth) {
                currentHealth += regenDelay * Time.deltaTime;
                currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);
                AutoGenHandleBoderFade();
            }
        }
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);
        lastDamageTime = Time.time;

        if (BoderAlertDamagedTaken != null) {
            autoGenColor.a = 0;
            BoderAutoReGen.color = autoGenColor;
            alertColor = BoderAlertDamagedTaken.color;
            alertColor.a = 1;
            BoderAlertDamagedTaken.color = alertColor;
        }

        if (currentHealth > 0)
        {
            if (isPlayer) {
                if (screenDamage != null && shakingCamera != null) {
                    screenDamage.ResetTrigger(Hit);
                    screenDamage.SetTrigger(Hit);

                    shakingCamera.ResetTrigger(Shaking);
                    shakingCamera.SetTrigger(Shaking);
                }
            }
            StartCoroutine(Invunerability());
        }
        else
        if (!dead)
        {
            foreach (Behaviour component in components)
            {
                component.enabled = false;
                Deactivate();
            }      
                DecayObject.transform.SetParent(null);
                DecayObject.SetActive(true);

            if (isPlayer)
            {
                Debug.Log("Game Over on");
                UiManager.Instance.ShowUI(UIName.GameOverScreen);
            }
            dead = true;
        }

    }
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, startingHealth);
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }

    private IEnumerator Invunerability()
    {
        Physics2D.IgnoreLayerCollision(8, 9, true);
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRender.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRender.color = new Color(0, 0.860742f, 0.8679245f, 1f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }

        Physics2D.IgnoreLayerCollision(8, 9, false);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);

    }
    public void Respawn()
    {
        dead = false;
        AddHealth(startingHealth);
 
        StartCoroutine(Invunerability());

        //Activate all attached component classes
        foreach (Behaviour component in components)
            component.enabled = true;
        // GetComponent<PlayerMoverment>().enabled = true;
    }
}