using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine;

public class DashBar : MonoBehaviour
{
    [SerializeField] private PlayerMoverment PlayerMoverment;
    [SerializeField] private Image currentDashBar;

    private void Awake()
    {
        currentDashBar.fillAmount = PlayerMoverment._coolDownBoosting / PlayerMoverment.coolDownBoosting;
    }
    private void Update()
    {
        currentDashBar.fillAmount = PlayerMoverment._coolDownBoosting / PlayerMoverment.coolDownBoosting;

    }
}
