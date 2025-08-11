
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class rangedEnemy : MonoBehaviour
{
    [Header("ranged Attack Parameters")]
    [SerializeField] private float attackCoolDown;
    [SerializeField] private float range;
    [SerializeField] private float damge;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] bullets;

    [Header("Detection (Circle)")]
    [SerializeField] private Vector2 offset = new Vector2(1.5f, 0f); 
    [SerializeField] private LayerMask playerLayer;


    [Header("SoundManager")]
    [SerializeField] private AudioClip FireBulletSound;

    [Header("Gun")]
    [SerializeField] private Transform enemyGun;

    private float coolDownTimer = Mathf.Infinity;
    private int i;
 
    private Patrolling enemyPatrolling;
    private bool isChasing = false;
    [SerializeField] private Transform detectedPlayer;
    [SerializeField] private Transform enemyBody;
    [SerializeField] private float gunForwardOffset = 0f;
    [SerializeField] private float speedChasing = 0f;
    [SerializeField] private float desiredXDistance = 3f;   
    [SerializeField] private float distanceTolerance = 0.3f;
    private void Awake()
    {
        enemyPatrolling = GetComponentInParent<Patrolling>();
    }

    private void Update()
    {
        coolDownTimer += Time.deltaTime;

        if (PlayerInside())
            isChasing = true;

        if (PlayerInside())
        {
            Debug.Log("player in sight");


            enemyPatrolling.enabled = false;
            Vector2 dir = detectedPlayer.position - enemyGun.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + gunForwardOffset;

          

            enemyGun.rotation = Quaternion.Euler(0f, 0f, angle);
            bool targetOnRight = detectedPlayer.position.x >= transform.position.x;

            float dx = detectedPlayer.position.x - transform.position.x;
            float adx = Mathf.Abs(dx);

            if (adx > desiredXDistance + distanceTolerance)
            {
                transform.Translate(Mathf.Sign(dx) * speedChasing * Time.deltaTime, 0f, 0f);
            }
            else if (adx < desiredXDistance - distanceTolerance)
            {
                transform.Translate(-Mathf.Sign(dx) * speedChasing * Time.deltaTime, 0f, 0f);
            }

            if (enemyBody)
                enemyBody.localScale = new Vector3(targetOnRight ? 1f : -1f, 1f, 1f);


            float z = (targetOnRight ? angle : angle + 180f) + gunForwardOffset;
            enemyGun.rotation = Quaternion.Euler(0f, 0f, z);

            if (coolDownTimer >= attackCoolDown)
            {
                coolDownTimer = 0;
                AudioManager.instance.PlaySound(FireBulletSound);
                rangedAttack();
                i++;
            }
        }
        else
        {
            enemyGun.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        if (enemyPatrolling != null)
            enemyPatrolling.enabled = !PlayerInside();

    }
    private void rangedAttack()
    {
        coolDownTimer = 0;

        bullets[FindFireball()].transform.position = firepoint.position;
        bullets[FindFireball()].GetComponent<EnemyProjectile>().ActivateProjectile(Mathf.Sign(transform.localScale.x),enemyGun);

    }
    private int FindFireball()
    {
        for (int i = 0; i < bullets.Length; i++)
        {
            if (!bullets[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
    private Vector2 GetDetectionCenter()
    {
        float face = Mathf.Sign(transform.localScale.x);
        Vector2 worldOffset = new Vector2(offset.x * face, offset.y);
        return (Vector2)transform.position + worldOffset;
    }
    private bool PlayerInside()
    {
        Vector2 center = GetDetectionCenter();
        Collider2D hit = Physics2D.OverlapCircle(center, range, playerLayer);
        if (hit != null) {
            detectedPlayer = hit.transform;
            return true;
        }
        detectedPlayer = null;

        return false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 center = Application.isPlaying ? GetDetectionCenter()
                                               : (Vector2)transform.position + new Vector2(offset.x * Mathf.Sign(transform.localScale.x), offset.y);
        Gizmos.DrawWireSphere(center, range);

        if (Application.isPlaying && detectedPlayer) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firepoint.position, detectedPlayer.position);
        }
    }


}