using JS;
using System.Collections;
using UnityEditor;
using UnityEngine;

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
    public bool isPlayer;


   // private UiManager uiManager;
   
    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        
        spriteRender = GetComponent<SpriteRenderer>();
 
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
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