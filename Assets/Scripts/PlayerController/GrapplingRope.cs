using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingRope : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private Transform firePoint; 
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float pullSpeed = 10f;
    [SerializeField] private bool useSwing = true;

    [SerializeField] private int ropeSegments = 20;
    [SerializeField] private float ropeWaveAmplitude = 0.5f;
    [SerializeField] private float ropeWaveFrequency = 2f;

    [SerializeField] private PlayerMoverment playerMoverment;

    private Vector2 grapplePoint;
    private DistanceJoint2D ropeJoint;
    private bool isGrappling;

    void Awake()
    {
        lineRenderer.enabled = false;
        ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
        ropeJoint.enabled = false;
        ropeJoint.autoConfigureDistance = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            DrawRope();
            if (!useSwing)
            {
                Vector2 direction = (grapplePoint - playerRb.position).normalized;
                playerRb.linearVelocity = direction * pullSpeed;
            }
            else
            {
                float horizontalInput = Input.GetAxis("Horizontal"); 
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    Vector2 dirToHook = grapplePoint - playerRb.position;
                    Vector2 swingDir = Vector2.Perpendicular(dirToHook).normalized;

                    swingDir *= -Mathf.Sign(horizontalInput);

                    playerRb.AddForce(swingDir * Mathf.Abs(horizontalInput) * pullSpeed, ForceMode2D.Force);
                }
            }
        }
    }


    void StartGrapple()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            (mousePos - (Vector2)firePoint.position).normalized,
            maxDistance,
            grappleLayer
        );

        if (hit.collider != null)
        {
            isGrappling = true;
            grapplePoint = hit.point;
            lineRenderer.enabled = true;

            if (useSwing)
            {
                ropeJoint.enabled = true;
                ropeJoint.connectedAnchor = grapplePoint;
                ropeJoint.distance = Vector2.Distance(playerRb.position, grapplePoint);
                ropeJoint.autoConfigureDistance = false;
                if (playerRb.linearVelocity.magnitude < 0.1f)
                {
                    Vector2 dir = grapplePoint - playerRb.position;
                    Vector2 perp = Vector2.Perpendicular(dir).normalized;
                    playerRb.AddForce(perp * 10f, ForceMode2D.Impulse);
                }
            }
        }
        playerMoverment.isGrappling = true;
    }

    void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.enabled = false;
        ropeJoint.enabled = false;
        playerMoverment.isGrappling = false;
    }

    void DrawRope()
    {
        if (!isGrappling) return;

        lineRenderer.positionCount = ropeSegments;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            Vector2 point = Vector2.Lerp(firePoint.position, grapplePoint, t);

            float wave = Mathf.Sin(Time.time * ropeWaveFrequency + t * Mathf.PI) * ropeWaveAmplitude * (1 - t);
            point.y -= Mathf.Abs(wave);

            lineRenderer.SetPosition(i, point);
        }
    }
}
