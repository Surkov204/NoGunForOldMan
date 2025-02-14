using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BulletBar : MonoBehaviour
{

    [SerializeField] private PlayerAttack player;

    [SerializeField] private Image currenBulletBar;

    private void Start()
    {
        currenBulletBar.fillAmount = player.currentBullet / player.maxBullet;
    }
    private void Update()
    {
        currenBulletBar.fillAmount = player.currentBullet / player.maxBullet;
    }
}
