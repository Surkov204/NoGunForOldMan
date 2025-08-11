using UnityEngine;

public class EnemyGunController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform target;              
    [SerializeField] private Transform body;              
    [SerializeField] private SpriteRenderer bodySprite;   
    [SerializeField] private Transform gunPivot;           

    [Header("Aim")]
    [SerializeField] private float turnSpeed = 12f;        
    [SerializeField] private float maxAimAngle = 80f;   
    [SerializeField] private Vector2 pivotOffset = new Vector2(0.0f, 0.0f); 
    [SerializeField] private bool useLineOfSight = false;
    [SerializeField] private LayerMask losBlockers;      

    private bool facingRight = true;

    private void Reset()
    {
        gunPivot = transform;
        body = transform;
        bodySprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!target || !gunPivot || !body) return;

        float dx = target.position.x - body.position.x;
        bool wantFacingRight = dx >= 0f;

        if (wantFacingRight != facingRight)
            SetFacing(wantFacingRight);

        Vector2 pivotPos = (Vector2)gunPivot.position + pivotOffset;
        Vector2 toTarget = (Vector2)target.position - pivotPos;

        if (useLineOfSight)
        {
            float dist = toTarget.magnitude;
            if (Physics2D.Raycast(pivotPos, toTarget.normalized, dist, losBlockers))
                return; 
        }

        float desiredAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        if (facingRight)
        {
            float local = Mathf.DeltaAngle(0f, desiredAngle);
            local = Mathf.Clamp(local, -maxAimAngle, maxAimAngle);
            desiredAngle = local;
        }
        else
        {
            float local = Mathf.DeltaAngle(180f, desiredAngle);
            local = Mathf.Clamp(local, -maxAimAngle, maxAimAngle);
            desiredAngle = 180f + local; 
        }

        float current = gunPivot.eulerAngles.z;
        float next = Mathf.LerpAngle(current, desiredAngle, Time.deltaTime * turnSpeed);
        gunPivot.rotation = Quaternion.Euler(0f, 0f, next);

        Vector3 gs = gunPivot.localScale;
        gs.y = facingRight ? Mathf.Abs(gs.y) : -Mathf.Abs(gs.y);
        gunPivot.localScale = gs;
    }

    private void SetFacing(bool right)
    {
        facingRight = right;

        if (bodySprite != null)
        {
            bodySprite.flipX = !right;
        }
        else
        {
            Vector3 s = body.localScale;
            s.x = right ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            body.localScale = s;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!gunPivot) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(gunPivot.position + (Vector3)pivotOffset, 0.05f);
    }

}
