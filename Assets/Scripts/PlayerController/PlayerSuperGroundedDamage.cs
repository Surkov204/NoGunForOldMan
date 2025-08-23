using UnityEngine;

public class PlayerSuperGroundedDamage : MonoBehaviour
{
   [SerializeField] private float damagedTaken = 10f; 

    public void SetDamage(float dmg)
    {
        damagedTaken = dmg;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enermy")
        {
            collision.GetComponent<health>().TakeDamage(damagedTaken);
        }
    }
}
