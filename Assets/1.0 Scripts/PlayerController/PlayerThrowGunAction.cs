using UnityEngine;

public class PlayerThrowGunAction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Rigidbody2D gunPrefab;
    [SerializeField] private PlayerGunInventory playerGunInventory;

    [Header("Trajectory Dots")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int dotCount = 10;
    [SerializeField] private float dotSpacing = 0.4f;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 18f;
    [SerializeField] private float spinForce = 500f;

    [SerializeField] private Animator shakingCamera;
    private static readonly int Shaking = Animator.StringToHash("Saking");

    private Camera cam;
    private GameObject[] dots;
    private bool isAiming;

    private void Start()
    {
        cam = Camera.main;

        dots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            dots[i] = Instantiate(dotPrefab, transform);
            dots[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isAiming = true;
        }

        if (isAiming && Input.GetKey(KeyCode.T))
        {
            ShowTrajectory();
        }

        if (isAiming && Input.GetKeyUp(KeyCode.T))
        {
            ThrowGun();
            HideDots();
            isAiming = false;
        }
    }

    private void ShowTrajectory()
    {
        Vector2 dir = AimDir().normalized;

        for (int i = 0; i < dotCount; i++)
        {
            Vector2 pos = (Vector2)throwPoint.position + dir * (i * dotSpacing);
            dots[i].transform.position = pos;
            dots[i].SetActive(true);
        }
    }

    private void HideDots()
    {
        foreach (var dot in dots)
            dot.SetActive(false);
    }

    private void ThrowGun()
    {
        shakingCamera.ResetTrigger(Shaking);
        shakingCamera.SetTrigger(Shaking);

        GameObject currentGun = playerGunInventory.CurrentGO();
        if (currentGun == null)
        {
            Debug.Log("No gun to throw!");
            return;
        }

        Vector2 dir = AimDir().normalized;

        Rigidbody2D rb = Instantiate(gunPrefab, throwPoint.position, Quaternion.identity);
        rb.gameObject.SetActive(true);
        rb.linearVelocity = dir * throwForce;
        rb.angularVelocity = Random.Range(-spinForce, spinForce);

        Debug.Log("Throw gun straight!");

        playerGunInventory.RemoveCurrentGun();
        playerGunInventory.SwitchToEmptyHand();
    }

    private Vector2 AimDir()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        return mousePos - throwPoint.position;
    }
}
