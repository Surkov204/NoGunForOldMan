using UnityEngine;
using System.Collections;
public class Grenade : MonoBehaviour
{
    public float fuseSeconds = 2.5f;
    public GameObject explosionPrefab;

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