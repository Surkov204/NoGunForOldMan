using Unity.Burst.CompilerServices;
using GameHealthSystem;
using UnityEngine;

public class GrenadeDamage : MonoBehaviour
{
    [SerializeField] private float damagedtaken;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enermy" || collision.tag =="Player")
        {
            collision.GetComponent<health>().TakeDamage(damagedtaken);
        }
        if (collision.tag == "Wall") Destroy(gameObject);
 
    }

}
