using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private AudioClip DeathSound_1;
    [SerializeField] private AudioClip DeathSound_2;
    [SerializeField] private AudioClip DeathSound_3;
    public float fieldOfImpact;
    
    public float force;

    public LayerMask layerMask;

    private void Start()
    {
        AudioManager.instance.PlaySound(DeathSound_2);
    }

    private void Update()
    {

        Explode();
    }


    private void Explode()
    {
       Collider2D[] Objects = Physics2D.OverlapCircleAll(transform.position, fieldOfImpact, layerMask);
        foreach (Collider2D obj in Objects)
        {
            Vector2 direction = obj.transform.position - transform.position;

            obj.GetComponent<Rigidbody2D>().AddForce(direction * force);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fieldOfImpact);
    }
}
