using TMPro;
using UnityEngine;
using System.Collections;
using System.Linq;
public class TypeTextEffect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI mainTargetText;

    [Header("Texts")]
    [TextArea][SerializeField] private string fullText;
    private void OnEnable()
    {
        StartCoroutine(StartTypeTheText(mainTargetText, fullText));
    }

    private IEnumerator StartTypeTheText(TextMeshProUGUI mainTargetText, string fulltext) {
        mainTargetText.text = "";
        foreach (char c in fullText) {
            mainTargetText.text += c;
            yield return new WaitForSeconds(0.1f);
        }
      
    }
}
