using System.Collections;
using UnityEngine;

public class LoadingCallback : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(2f);
        SceneLoader.LoaderCallback();
    }
}
