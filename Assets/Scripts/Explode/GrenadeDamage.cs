using Unity.Burst.CompilerServices;
using UnityEngine;

public class GrenadeDamage : MonoBehaviour
{
    [SerializeField] private float damagedtanken;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enermy" || collision.tag =="Player")
        {
            collision.GetComponent<health>().TakeDamage(damagedtanken);
        }
        if (collision.tag == "Wall") Destroy(gameObject);
 
    }

}
