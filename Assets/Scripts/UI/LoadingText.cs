using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    public TextMeshProUGUI loadingText; 
    public float updateSpeed = 0.5f;

    private string baseText = "Reloading"; 
    private int dotCount = 0; 

    void Start()
    {
        if (loadingText != null)
        {
            StartCoroutine(UpdateLoadingText());
        }
    }

    IEnumerator UpdateLoadingText()
    {
        while (true) 
        {
            dotCount = (dotCount % 3) + 1; 
            loadingText.text = baseText + new string('.', dotCount); 
            yield return new WaitForSeconds(updateSpeed); 
        }
    }
}
