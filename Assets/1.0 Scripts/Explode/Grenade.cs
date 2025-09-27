using UnityEngine;
using System.Collections;
using System;
public class Grenade : MonoBehaviour
{
    [SerializeField] private float fuseSeconds = 2.5f;
    [SerializeField] private GameObject explosionPrefab;

    void Start()
    {
        StartCoroutine(ExplodeAfterTime());
    }

    IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(fuseSeconds);
        Debug.Log("boom after");

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}