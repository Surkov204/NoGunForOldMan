using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Transform playerTransform;
   

    private void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));

        Vector3 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
      
        transform.rotation = Quaternion.Euler(new Vector3(0f,0f,angle));

        if (mousePosition.x > playerTransform.position.x)
        {
            playerTransform.localScale = new Vector3(1f, 1f, 1f);
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            playerTransform.localScale = new Vector3(-1f, 1f, 1f);
            transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
        }
      
    }
}
