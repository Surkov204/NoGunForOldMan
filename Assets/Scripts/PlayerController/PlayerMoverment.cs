using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class PlayerMoverment : MonoBehaviour
{
    [Header("Basic Movement Value")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundlayer;
    [SerializeField] private LayerMask groundDecay;
    [SerializeField] private float extraJump;
    [Header("Dash Value")]
    [SerializeField] private float dashBoost;   
    [SerializeField] public float dashTime;
    [SerializeField] public float coolDownBoosting;
    
    private float _dashTime;
    public float _coolDownBoosting { get; private set; }
    private bool isDashing = false;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private float movementvalue;
    private float extraJumpCounter;
    private float x = 0f;
    private bool checkRigth;
    private void Awake()
    {
        //VARIABLE

        checkRigth = true;
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        _coolDownBoosting = coolDownBoosting;
    }


    private void Update()
    {        
        // get movement Value //
        movementvalue = Input.GetAxis("Horizontal");
        // move x and y axis //
        body.velocity = new Vector2(movementvalue * speed, body.velocity.y);

        
        // Flip //
        if (movementvalue > 0.01f)
            transform.localScale = new Vector3(1f,1f,1f);
        else if (movementvalue < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // jumping  //
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump();
     
        }

        if (Input.GetKeyUp(KeyCode.Space) && body.velocity.y > 0)
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y / 2);
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

    private void jump()
    {
        if (isGrounded() || isGroundDecay())
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
        }
        else
        {
            if (extraJumpCounter > 0)
            {   
                body.velocity = new Vector2(body.velocity.x, jumpPower);
        
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
    public bool canAttack()
    {
        return movementvalue == 0 && isGrounded();
    }
    
}

