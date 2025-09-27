using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockEnermyHealthBar : MonoBehaviour
{
    public Transform enemy;          
    public Vector3 offset;             

    private Vector3 initialHealthBarScale;

    void Start()
    {
        initialHealthBarScale = transform.localScale;
    }

    void Update()
    {

        transform.position = new Vector3(enemy.position.x, enemy.position.y + offset.y, enemy.position.z);
        transform.localScale = new Vector3(Mathf.Abs(initialHealthBarScale.x), initialHealthBarScale.y, initialHealthBarScale.z);
    }
}
