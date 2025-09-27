using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawm : MonoBehaviour
{
    public GameObject objectToSpawn;


    public float spawnInterval = 30f;


    public Transform spawnPoint;

    void Start()
    {
        
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        while (true)
        {
            // Spawn object tại vị trí
            SpawnObject();

            // Chờ một khoảng thời gian
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObject()
    {
        if (objectToSpawn != null)
        {
            // Kiểm tra vị trí spawn có tồn tại
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

            // Tạo object tại vị trí spawn
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}
