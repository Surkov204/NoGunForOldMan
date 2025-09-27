using TMPro;
using UnityEngine;
using System.Collections;

public class TypeWriterEffect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI highDiffText;
    [SerializeField] private TextMeshProUGUI settingText;
    [SerializeField] private TextMeshProUGUI mainScreenText;

    [Header("Screen References")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject highDiffScreen;
    [SerializeField] private GameObject settingScreen;
    [SerializeField] private GameObject mainScreen;

    [Header("Config")]
    [SerializeField] private float updateSpeed = 0.1f;

    [Header("Texts")]
    [TextArea][SerializeField] private string fullTextPause;
    [TextArea][SerializeField] private string fullTextHighDiff;
    [TextArea][SerializeField] private string fullTextSetting;
    [TextArea][SerializeField] private string fullTextMain;

    private void OnEnable()
    {
        if (pauseText && pauseScreen.activeInHierarchy)
            StartCoroutine(TypeText(pauseText, fullTextPause));

        if (highDiffText && highDiffScreen.activeInHierarchy)
            StartCoroutine(TypeText(highDiffText, fullTextHighDiff));

        if (settingText && settingScreen.activeInHierarchy)
            StartCoroutine(TypeText(settingText, fullTextSetting));

        if (mainScreenText && mainScreen.activeInHierarchy)
            StartCoroutine(TypeText(mainScreenText, fullTextMain));
    }

    private IEnumerator TypeText(TextMeshProUGUI targetText, string fullText)
    {
        targetText.text = "";
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSecondsRealtime(updateSpeed);
        }
    }
}
