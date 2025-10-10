using System.Collections;
using System.Collections.Generic;
using GameHealthSystem;
using UnityEngine;
using System;
public class PlayerMoverment : MonoBehaviour, ISaveable
{
    [Header("Basic Movement Value")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float extraJump;
    private float movementvalue;
    private float extraJumpCounter;
    private bool CheckOnWall;

    [Header("Layer Mark")]
    [SerializeField] private LayerMask groundlayer;
    [SerializeField] private LayerMask groundDecay;
    [SerializeField] private LayerMask wallLayer;

    [Header("Power Grounded")]
    [SerializeField] private float slamForce = -25f;
    [SerializeField] private GameObject superGroundedDamageZone;
    [SerializeField] private float timeToResetPowerGrounded;
    private bool isSlamming = false;

    [Header("Power Grounded Damage Scale")]
    [SerializeField] private float baseSlamDamage = 10f;   
    [SerializeField] private float damagePerUnitHeight = 2f; 
    private float slamStartHeight;

    [Header("Power Grounded Effect")]
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private Transform shockwavePoint;
    [SerializeField] private float shockwaveDuration = 1.5f;

    [Header("Dash Value")]
    [SerializeField] private float dashBoost;   
    [SerializeField] public float dashTime;
    [SerializeField] public float coolDownBoosting;

    [Header("Ghost Trail Effect")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float ghostSpawnInterval = 0.05f;
    [SerializeField] private Color slamGhostColor = Color.red;
    private float ghostSpawnTimer;

    [Header("Animation")]
    [SerializeField] private Animator superGroundedShaking;
    [SerializeField] private string animationValue;

    [Header("Point")]
    [SerializeField] private Vector3 spawnPoint;

    private float _dashTime;
    public float _coolDownBoosting { get; private set; }
    private bool isDashing = false;
    private bool isUnderWater = false;

    [Header("Under water")]
    [SerializeField] private float underwaterTimeLimit = 10f;
    [SerializeField] private float damagePerSecond = 50f;   
    private float underwaterTimer = 0f;

    [SerializeField] private float swimSpeed = 4f;

    [SerializeField] private health playerHealth;
    /// Body player ///
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;

    [HideInInspector] public bool isGrappling = false;

    private void Start()
    {
        SaveManager.Instance.Registry(this);
    }

    private void Awake()
    {
        CheckOnWall = false;
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        _coolDownBoosting = coolDownBoosting;
    }

    private void OnDestroy()
    {
        if (SaveManager.HasInstance)
        SaveManager.Instance.UnRegistry(this);
    }

    [System.Serializable]
    public class PlayerData
    {
        public float x, y, z;
    }

    public object CaptureState()
    {
        return new PlayerData
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z
        };
    }

    public void RestoreState(object state)
    {
        PlayerData data = (PlayerData)state;
        transform.position = new Vector3(data.x, data.y, data.z);
    }

    public Type GetSaveType() => typeof(PlayerData);

    public string GetUniqueId()
    {
        return "Player";   
    }

    public void ResetState()
    {
        transform.position = spawnPoint;
    }

    private void Update()
    {
        if (isUnderWater)
        {
            underwaterTimer += Time.deltaTime;

            if (underwaterTimer >= underwaterTimeLimit)
            {
                playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
        else
        {
            underwaterTimer = 0f;
        }
        if (!isGrappling) {
            if (!isDashing)
                MoveForward();
            if (!isDashing)
                JumpAndDoubleJump();
            if (!isSlamming)
                Dashing();
            if (!isUnderWater)
            PowerGrounded();
        }
    }

    private void JumpAndDoubleJump()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            jump();
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space) && body.linearVelocity.y > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y / 2);
        }

        if (isGrounded() || isGroundDecay())
        {
            extraJumpCounter = extraJump;
        }
    }

    private void Dashing()
    {
        if (dashBoost > 0 && dashTime > 0 && coolDownBoosting > 0)
        {
            _coolDownBoosting = Mathf.Clamp(_coolDownBoosting, 0f, coolDownBoosting);

            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && _coolDownBoosting >= coolDownBoosting)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float dirX = (mouseWorld.x >= transform.position.x) ? 1f : -1f;

                body.gravityScale = 0;
                body.linearVelocity = new Vector2(dirX * dashBoost, 0f);

                _dashTime = dashTime;
                isDashing = true;
                _coolDownBoosting = 0;
            }

            if (isDashing)
            {
                _dashTime -= Time.deltaTime;

                ghostSpawnTimer -= Time.deltaTime;
                if (ghostSpawnTimer <= 0f)
                {
                    SpawnGhostDashing();
                    ghostSpawnTimer = ghostSpawnInterval;
                }

                if (_dashTime <= 0f)
                {
                    body.gravityScale = 7;
                    isDashing = false;
                }
            }

            _coolDownBoosting += Time.deltaTime;
        }
    }

    private void SpawnGhostDashing()
    {
        if (ghostPrefab != null)
        {
            GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
            SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();

            if (ghostSR != null && playerSR != null)
            {
                ghostSR.sprite = playerSR.sprite;
                ghostSR.flipX = playerSR.flipX;   
            }
        }
    }

    private void SpawnGhostTrailVertical(Vector3 startPos, Vector3 endPos, int ghostCount = 5)
    {
        if (ghostPrefab == null) return;

        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();

        for (int i = 0; i < ghostCount; i++)
        {
            float t = i / (float)(ghostCount - 1);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t); 

            GameObject ghost = Instantiate(ghostPrefab, pos, transform.rotation);
            SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();

            if (ghostSR != null && playerSR != null)
            {
                ghostSR.sprite = playerSR.sprite;
                ghostSR.flipX = playerSR.flipX;
                float alpha = 1f - t;
                ghostSR.color = slamGhostColor;
            }
        }
    }

    private void PowerGrounded()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isGrounded())
        {
            isSlamming = true;
            superGroundedDamageZone.SetActive(true);
            slamStartHeight = transform.position.y;
            body.linearVelocity = new Vector2(0, slamForce); 
        }
    
        if (isSlamming && (isGrounded() || isGroundDecay()))
        {
            isSlamming = false;
            superGroundedShaking.ResetTrigger(animationValue);
            superGroundedShaking.SetTrigger(animationValue);

            float slamEndHeight = transform.position.y;
            float fallDistance = slamStartHeight - slamEndHeight;
            float finalDamage = baseSlamDamage + Mathf.Max(0, fallDistance * damagePerUnitHeight);
            Debug.Log("final damage " + finalDamage);

            PlayerSuperGroundedDamage damage = superGroundedDamageZone.GetComponent<PlayerSuperGroundedDamage>();
            if (damage != null) damage.SetDamage(finalDamage);

            if (shockwavePrefab != null) {
                GameObject shockWave = Instantiate(shockwavePrefab, shockwavePoint);
                Destroy(shockWave, shockwaveDuration);
            }

            SpawnGhostTrailVertical(
            new Vector3(transform.position.x, slamStartHeight, transform.position.z),transform.position,12);

            StartCoroutine(InActivatedSuperGroundedZone(timeToResetPowerGrounded));
            body.linearVelocity = Vector2.zero;
            Debug.Log("Power Slam Landed!");
        }
    }

    private IEnumerator InActivatedSuperGroundedZone(float timeToResetPowerGrounded) {
        yield return new WaitForSeconds(timeToResetPowerGrounded);
        superGroundedDamageZone.SetActive(false);
    }


    private void MoveForward() {
        movementvalue = Input.GetAxis("Horizontal");
        if ((movementvalue > 0 && isOnWallRight()) || (movementvalue > 0 && isGroundedRight()))
        {
            movementvalue = 0;
        }
        if ((movementvalue < 0 && isOnWallLeft()) || (movementvalue < 0 && isGroundedLeft()))
        {
            movementvalue = 0;
        }
        float currentSpeed = isUnderWater ? speed*0.4f : speed;
        body.linearVelocity = new Vector2(movementvalue * currentSpeed, body.linearVelocity.y);
    }

    private void jump()
    {
        if (isGrounded() || isGroundDecay())
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
        }
        else
        {
            if (extraJumpCounter > 0)
            {   
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
        
                extraJumpCounter--;
            }

        }
    }

    public void ApplyBuoyancy(float surfaceY, Rigidbody2D rb)
    {
        if (transform.position.y < surfaceY)
        {
            float displacement = surfaceY - transform.position.y;

            rb.gravityScale = 0.05f;

            // Lực nổi
            float buoyancyStrength = 30f;
            rb.AddForce(Vector2.up * displacement * buoyancyStrength);

            // Lực kéo về mặt nước
            float surfacePull = 20f;
            float diff = surfaceY - rb.position.y;
            rb.AddForce(Vector2.up * diff * surfacePull);

            isUnderWater = true;
        }
        else
        {
            isUnderWater = false;
            rb.gravityScale = 7f;
        }
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundlayer);
        return raycastHit.collider != null;
    }

    private bool isGroundedLeft() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, 
                                 Vector2.left, 0.1f, groundlayer);
        return raycastHit.collider != null;
    }

    private bool isGroundedRight()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0,
                                 Vector2.right, 0.1f, groundlayer);
        return raycastHit.collider != null;
    }

    private bool isGroundDecay()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundDecay);
        return raycastHit.collider != null;
    }

    private bool isOnWallRight()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0,
                                 Vector2.right, 0.1f, wallLayer);
    }

    private bool isOnWallLeft()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0,
                                 Vector2.left, 0.1f, wallLayer);
    }

    public bool canAttack()
    {
        return movementvalue == 0 && isGrounded();
    }

    public void ResetExtraJump()
    {
        extraJumpCounter = extraJump;
    }
}

