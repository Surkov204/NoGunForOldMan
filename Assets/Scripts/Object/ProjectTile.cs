using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectTile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float damaged;
    private Vector3 initialDirection ;

    private float direction;
    private bool hit;
    private BoxCollider2D boxCollider;
    private float lifetime ;
    
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
       
    }

    private void Update()
    {

        if (hit) return;
        float movementSpeed = speed * Time.deltaTime * direction ;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > 3)
        {
            gameObject.SetActive(false);
            Debug.Log("inactive");
        }


}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        boxCollider.enabled = false;
        Deactivate();

        if (collision.tag == "Enermy")
        {
            collision.GetComponent<health>().TakeDamage(damaged);
            Deactivate();
        }

        if (collision.tag == "Ground")
        {
            Deactivate(); // Vô hiệu hóa viên đạn
            Debug.Log("on wall");
        }

        if (collision.tag == "Player")
        {
            collision.GetComponent <health>().TakeDamage(damaged); ;
            Deactivate();
        }
    }
   
    public void SetDirection(float _directionX,Transform gunScaletransform)
    {
        lifetime = 0;
        direction = _directionX;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _directionX)
            localScaleX = -localScaleX;

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
        transform.rotation = gunScaletransform.rotation; 
  
    }
    public void Deactivate()
    {
        Debug.Log("Deactivate");
        gameObject.SetActive(false);
    }
}
