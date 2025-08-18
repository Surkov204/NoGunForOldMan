using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class PlayerMoverment : MonoBehaviour
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

    [Header("Animation")]
    [SerializeField] private Animator superGroundedShaking;
    [SerializeField] private string animationValue;

    private float _dashTime;
    public float _coolDownBoosting { get; private set; }
    private bool isDashing = false;

    /// Body player ///
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
  
    private void Awake()
    {
        //VARIABLE
        CheckOnWall = false;
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        _coolDownBoosting = coolDownBoosting;

    }

    private void Update()
    {
        if (!isOnWall() || CheckOnWall == false)
        {
            if (!isDashing)  
                MoveForward();
            CheckOnWall = true;
        }
        // jumping  //
        if (!isDashing)
            JumpAndDoubleJump();
        // Dashing //
        if (!isSlamming)
            Dashing();
        // slam Ground //
        PowerGrounded();
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

                body.velocity = new Vector2(dirX * dashBoost, 0f);

                _dashTime = dashTime;
                isDashing = true;
                _coolDownBoosting = 0;
            }

            if (isDashing)
            {
                _dashTime -= Time.deltaTime;
                if (_dashTime <= 0f)
                {
                    isDashing = false;
                }
            }

            _coolDownBoosting += Time.deltaTime;
        }
    }

    private void PowerGrounded()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isGrounded())
        {
            isSlamming = true;
            superGroundedDamageZone.SetActive(true);
            slamStartHeight = transform.position.y;
            body.velocity = new Vector2(0, slamForce); 
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

            StartCoroutine(InActivatedSuperGroundedZone(timeToResetPowerGrounded));
            body.velocity = Vector2.zero;
            Debug.Log("Power Slam Landed!");
        }
    }

    private IEnumerator InActivatedSuperGroundedZone(float timeToResetPowerGrounded) {
        yield return new WaitForSeconds(timeToResetPowerGrounded);
        superGroundedDamageZone.SetActive(false);
    }


    private void MoveForward() {
        movementvalue = Input.GetAxis("Horizontal");
        body.linearVelocity = new Vector2(movementvalue * speed, body.linearVelocity.y);
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

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundlayer);
        return raycastHit.collider != null;
    }

    private bool isGroundDecay()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundDecay);
        return raycastHit.collider != null;
    }

    private bool isOnWall() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack()
    {
        return movementvalue == 0 && isGrounded();
    }
    
}

