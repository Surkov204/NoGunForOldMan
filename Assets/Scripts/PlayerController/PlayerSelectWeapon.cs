using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectWeapon : MonoBehaviour
{
    public GameObject[] weapons;
    public GameObject[] imageWeapon;
    private int selectedWeapon = 0; 

    void Start()
    {
        SelectCurrentWeapon();
    }

    void Update()
    {
     
        int previousWeapon = selectedWeapon;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Length >= 2)
        {
            selectedWeapon = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Length >= 3)
        {
            selectedWeapon = 2;
        }

        // Chuyển đổi vũ khí bằng con lăn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedWeapon = (selectedWeapon + 1) % weapons.Length;
        }
        else if (scroll < 0f)
        {
            selectedWeapon = (selectedWeapon - 1 + weapons.Length) % weapons.Length;
        }

        
        if (previousWeapon != selectedWeapon)
        {
            SelectCurrentWeapon();
        }
    }

    void SelectCurrentWeapon()
    {

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == selectedWeapon);
            imageWeapon[i].SetActive(i == selectedWeapon);
        }
        Debug.Log("Selected Weapon: " + (selectedWeapon + 1));
    }
}
