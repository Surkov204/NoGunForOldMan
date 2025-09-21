using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    [SerializeField] private AudioClip DeathSound;
    [SerializeField] private float fieldOfImpact;
    [SerializeField] private float force;
    [SerializeField] private bool isSuperGrounded = false;
    private bool hasExploded = false;
    public LayerMask layerMask;

    private void Start()
    {
        AudioManager.Instance.PlaySound(DeathSound);
       
    }
    private void FixedUpdate()
    {
        if (!hasExploded && isSuperGrounded == false)
        {
            CameraManager.Instance.GrenadeCamera();
            StartCoroutine(ResetCameraShake(0.5f));
            hasExploded = true;
        }
        Explode();
    }

    public void Explode()
    {
        Collider2D[] Objects = Physics2D.OverlapCircleAll(transform.position, fieldOfImpact, layerMask);
        foreach (Collider2D obj in Objects)
        {
            Vector2 direction = obj.transform.position - transform.position;

            obj.GetComponent<Rigidbody2D>().AddForce(direction * force);
        }
       
    }
    private IEnumerator ResetCameraShake(float timeToReset) {
        yield return new WaitForSeconds(timeToReset);
        CameraManager.Instance.ResetGrenadeCamera();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fieldOfImpact);
    }
}
