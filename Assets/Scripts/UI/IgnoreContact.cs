using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreContact : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    [SerializeField] private AudioClip soundDrop;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground" || collision.tag == "Surface")
        {
            AudioManager.Instance.PlaySound(soundDrop);
            boxCollider.isTrigger = false;  
 
        }
    }
}
