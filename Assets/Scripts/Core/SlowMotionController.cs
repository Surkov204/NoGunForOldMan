using UnityEngine;
using UnityEngine.UI;
public class SlowMotionController : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowFactor = 0.2f;      
    [SerializeField] private float maxSlowMotionTime = 10f; 
    [SerializeField] private float rechargeRate = 2f;

    [Header("UI")]
    [SerializeField] private Image slowMotionBar;
    [SerializeField] private Image slowMotionScreen;

    private bool isSlowMotion = false;
    private float currentSlowTime;

    private void Start()
    {
        currentSlowTime = maxSlowMotionTime;
        if (slowMotionBar != null)
            slowMotionBar.fillAmount = 1f; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (!isSlowMotion && currentSlowTime >= maxSlowMotionTime)
            {
                ActivateSlowMotion();
            }
            else if (isSlowMotion)
            {
                DeactivateSlowMotion(); 
            }
        }

        if (isSlowMotion)
        {
            currentSlowTime -= Time.unscaledDeltaTime;
            if (currentSlowTime <= 0f)
            {
                currentSlowTime = 0f;
                DeactivateSlowMotion();
            }
        }
        else
        {
            if (currentSlowTime < maxSlowMotionTime)
            {
                currentSlowTime += rechargeRate * Time.unscaledDeltaTime;
                if (currentSlowTime > maxSlowMotionTime)
                    currentSlowTime = maxSlowMotionTime;
            }
        }
        if (slowMotionBar != null)
            slowMotionBar.fillAmount = currentSlowTime / maxSlowMotionTime;
    }

    private void ActivateSlowMotion()
    {
        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        isSlowMotion = true;
        slowMotionScreen.gameObject.SetActive(true);
    }

    private void DeactivateSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isSlowMotion = false;
        slowMotionScreen.gameObject.SetActive(false);
    }
}
