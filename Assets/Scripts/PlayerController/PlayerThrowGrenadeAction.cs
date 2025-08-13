using UnityEngine;
using System.Collections;

public class PlayerThrowGrenadeAction : MonoBehaviour
{
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Rigidbody2D grenadePrefab;        
    [SerializeField] private GameObject grenadePreview;       
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private int dotCount = 20;
    [SerializeField] private float stepTime = 0.1f;
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float fuseSeconds = 2.5f;

    private Camera cam;
    private GameObject[] dots;
    private bool isAiming = false;

    void Start()
    {
        cam = Camera.main;
        dots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            dots[i] = Instantiate(dotPrefab, transform);
            dots[i].SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            isAiming = true;
            if (grenadePreview) grenadePreview.SetActive(true);
        }

        if (isAiming && Input.GetKey(KeyCode.G))
        {
            ShowTrajectory();
        }

        if (isAiming && Input.GetKeyUp(KeyCode.G))
        {
            Throw();
            HideDots();
            isAiming = false;
        }
    }

    void ShowTrajectory()
    {
        Vector2 startPos = throwPoint.position;
        Vector2 velocity = AimDir().normalized * throwForce;
        Vector2 gravity = Physics2D.gravity;

        for (int i = 0; i < dotCount; i++)
        {
            float t = i * stepTime;
            Vector2 pos = startPos + velocity * t + 0.5f * gravity * (t * t);
            dots[i].transform.position = pos;
            dots[i].SetActive(true);
        }
    }

    void HideDots()
    {
        foreach (var dot in dots)
            dot.SetActive(false);
    }

    void Throw()
    {
        var rb = Instantiate(grenadePrefab, throwPoint.position, Quaternion.identity);
        rb.linearVelocity = AimDir().normalized * throwForce;

        Debug.Log("boom begin");
        if (grenadePreview)
            Destroy(grenadePreview);

        Destroy(gameObject); 
    }



    Vector2 AimDir()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - throwPoint.position);
    }
}
