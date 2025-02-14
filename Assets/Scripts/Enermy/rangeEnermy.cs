
using UnityEngine;

public class rangedEnemy : MonoBehaviour
{
    [Header("ranged Attack Parameters")]
    [SerializeField] private float attackCoolDown;
    [SerializeField] private float range;
    [SerializeField] private float damge;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] fireballs;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;

    [Header("SoundManager")]
    [SerializeField] private AudioClip FireBallSound;

    private float coolDownTimer = Mathf.Infinity;
    private int i;
 
    private Patrolling enemyPatrolling;
    private void Awake()
    {
        enemyPatrolling = GetComponentInParent<Patrolling>();
    }
    private void Update()
    {
        coolDownTimer += Time.deltaTime;

        if (PlayerInside())
        {
            if (coolDownTimer >= attackCoolDown)
            {
                coolDownTimer = 0;
                rangedAttack();
                i++;
            
            }
        }
        if (enemyPatrolling != null)
            enemyPatrolling.enabled = !PlayerInside();

    }
    private void rangedAttack()
    {
        coolDownTimer = 0;

        fireballs[FindFireball()].transform.position = firepoint.position;
        fireballs[FindFireball()].GetComponent<EnemyProjectile>().ActivateProjectile(Mathf.Sign(transform.localScale.x));

    }
    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    private bool PlayerInside()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
             new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z), 0, Vector2.left, 0, playerLayer);

        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance, new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }


}