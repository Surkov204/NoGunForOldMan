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


    [Header("Dash Value")]
    [SerializeField] private float dashBoost;   
    [SerializeField] public float dashTime;
    [SerializeField] public float coolDownBoosting; 
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
            MoveForward();
            CheckOnWall = true;
    
        }
    
        // Flip //
        

        // jumping  //
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

        // Dashing //
        if (dashBoost > 0 && dashTime > 0 && coolDownBoosting > 0)
        {
            _coolDownBoosting = Mathf.Clamp(_coolDownBoosting, 0f, coolDownBoosting);

            if (Input.GetKey(KeyCode.LeftShift) && _dashTime <= 0 && isDashing == false && _coolDownBoosting >= coolDownBoosting)
            {
               
                speed += dashBoost;
                _dashTime = dashTime;
                isDashing = true;
   
                _coolDownBoosting = 0;
            }

            if (_dashTime <= 0 && isDashing == true)
            {
                speed -= dashBoost;
                isDashing = false;
           
            }
            else
            {
                _dashTime -= Time.deltaTime;
            }
            _coolDownBoosting += Time.deltaTime;
        }
     
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

