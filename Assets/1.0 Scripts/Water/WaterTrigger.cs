using UnityEngine;

namespace WaterRipple
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WaterTrigger : MonoBehaviour
    {
        private WaterShapeController water;

        void Start()
        {
            water = GetComponent<WaterShapeController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null)
            {
                float velocityY = collision.attachedRigidbody.linearVelocity.y;
            }
        }
    }
}
