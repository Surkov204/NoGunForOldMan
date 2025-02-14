
using UnityEngine;

public class Patrolling : MonoBehaviour
{
    [Header("Patrol posit")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header("Enemy")]
    [SerializeField] private Transform enemy;

    [Header("Movement parameters")]
    [SerializeField] private float speed;

    private Vector3 initscale;

    [Header("Enemy Animator")]
 //   [SerializeField] private Animator anim;

    [Header("Idle Behaviour")]
    [SerializeField] private float idleDuration;

    private float idleTime;
    private bool movingleft;
    private void Awake()
    {
        initscale = enemy.localScale;
    }
    private void OnDisable()
    {
       // anim.SetBool("moving", false);
    }


    private void Update()
    {
        if (movingleft)
        {
            if (enemy.position.x >= leftEdge.position.x)
            {
                MoveInDirection(-1);
            }
            else
            {
                DirectionChange();
            }
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
                MoveInDirection(1);
            else
            {
                DirectionChange();
            }
        }

    }
    private void DirectionChange()
    {
      //  anim.SetBool("moving", false);

        idleTime += Time.deltaTime;
        if (idleTime > idleDuration)
            movingleft = !movingleft;
    }
    private void MoveInDirection(int _direction)
    {
        idleTime = 0;
       // anim.SetBool("moving", true);
        enemy.localScale = new Vector3(Mathf.Abs(initscale.x) * _direction, initscale.y, initscale.z);

        enemy.position = new Vector3(enemy.position.x + speed * _direction * Time.deltaTime, enemy.position.y, enemy.position.z);
    }
}