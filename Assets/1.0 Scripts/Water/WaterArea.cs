using UnityEngine;

namespace WaterRipple
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WaterArea : MonoBehaviour
    {
        public WaterShapeController water;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                water.SplashAt(other.transform.position.x, rb.linearVelocity.y);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                float surfaceY = water.GetWaterHeightAt(other.transform.position.x);
                if (other.transform.position.y < surfaceY)
                {
                    float displacement = surfaceY - other.transform.position.y;

                    float buoyancyStrength = 20f * rb.mass;
                    rb.AddForce(Vector2.up * displacement * buoyancyStrength);

                    float damping = 3f;
                    rb.AddForce(-rb.linearVelocity * damping);

                    float surfacePull = 50f; 
                    float diff = surfaceY - rb.position.y;
                    rb.AddForce(Vector2.up * diff * surfacePull);
                }
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearDamping = 0f;
                rb.angularDamping = 0.05f;
            }
        }
    }
}
