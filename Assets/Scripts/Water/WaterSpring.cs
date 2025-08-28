using UnityEngine;
using UnityEngine.U2D;

namespace WaterRipple
{
    public class WaterSpring : MonoBehaviour
    {
        private int waveIndex = 0;
        [SerializeField]
        private static SpriteShapeController spriteShapeController = null;
        /////////////////
        [System.NonSerialized]
        public float velocity = 0;
        private float force = 0;
        // current height
        [System.NonSerialized]
        public float height = 0f;
        // normal height
        private float target_height = 0f;
        private float resistance = 40f;
        public void Init(SpriteShapeController ssc, int index)
        {
            waveIndex = index;              
            spriteShapeController = ssc;

            velocity = 0;
            height = transform.localPosition.y;
            target_height = height;
        }

        public void WaveSpringUpdate(float springStiffness, float dampening)
        {

            height = transform.localPosition.y;
            // maximum extension
            var x = height - target_height;
            var loss = -dampening * velocity;

            force = -springStiffness * x + loss;
            velocity += force;
            var y = transform.localPosition.y;
            transform.localPosition = new Vector3(transform.localPosition.x, y + velocity, transform.localPosition.z);

        }
        public void WavePointUpdate()
        {
            if (spriteShapeController != null)
            {
                Spline waterSpline = spriteShapeController.spline;

                if (waveIndex >= 0 && waveIndex < waterSpline.GetPointCount())
                {
                    Vector3 current = transform.localPosition;

                    // Lấy point trái/phải
                    Vector3 left = waveIndex > 0 ? waterSpline.GetPosition(waveIndex - 1) : current;
                    Vector3 right = waveIndex < waterSpline.GetPointCount() - 1 ? waterSpline.GetPosition(waveIndex + 1) : current;

                    // đảm bảo y không đè quá sát hàng xóm
                    float minDistance = 0.01f; // khoảng cách tối thiểu
                    if (Mathf.Abs(current.x - left.x) > minDistance &&
                        Mathf.Abs(current.x - right.x) > minDistance)
                    {
                        waterSpline.SetPosition(waveIndex, new Vector3(current.x, current.y, current.z));
                    }
                }
            }
        }
        // Adding a collider so we can detect the falling object
        // Force send layers set to Nothing
        // so we the circle springs do not interact with the falling object
        // we only want to detect when they collide so we can trigger the impact
        private void OnCollisionEnter2D(Collision2D other)
        {
            Rigidbody2D rb = other.gameObject.GetComponentInParent<Rigidbody2D>();
            if (rb != null)
            {
                // lực ripple phụ thuộc vào tốc độ rơi + khối lượng
                float impact = (rb.linearVelocity.y * rb.mass) / resistance;

                velocity += impact;
            }
        }
    }
}
