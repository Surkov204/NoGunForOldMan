using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private health playerHealth;
    [SerializeField] private Image totalhealthBar;
    [SerializeField] private Image currenthealthBar;

    private void Start()
    {
        totalhealthBar.fillAmount = playerHealth.startingHealth / 10;
    }
    private void Update()
    {
        currenthealthBar.fillAmount = playerHealth.currentHealth / 100;
      //  Debug.Log(playerHealth.currentHealth);
    }
}
