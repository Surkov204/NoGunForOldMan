using UnityEngine;

public class GunThrowDamaged : MonoBehaviour
{
    [SerializeField] private int damage = 30;          
    [SerializeField] private string enemyTag = "Enermy"; 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(enemyTag))
        {
            Debug.Log("Gun hit enemy: " + collision.collider.name);

            health enemyHealth = collision.collider.GetComponent<health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

        }
    }
}
