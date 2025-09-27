using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public float speed = 5f;
    void Update()
    {
        // Tính toán vị trí mới theo trục x
        float newX = transform.position.x + speed * Time.deltaTime;

        // Gán lại vị trí mới cho vật thể
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
