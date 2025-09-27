using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System.Collections;

namespace WaterRipple
{
    [ExecuteAlways]
    public class WaterShapeController : MonoBehaviour
    {
        [SerializeField] private GameObject wavePointPref;
        [SerializeField] private SpriteShapeController spriteShapeController;
        [SerializeField] private GameObject wavePoints;

        private int CornersCount = 2;

        [Range(1, 100)]
        [SerializeField] private int WavesCount = 6;

        [Header("Spring Settings")]
        public float spread = 0.002f;
        [SerializeField] private float dampening = 0.08f;
        [SerializeField] private float springStiffness = 0.04f;

        [SerializeField] private List<WaterSpring> springs = new();

        void FixedUpdate()
        {
            foreach (WaterSpring s in springs)
            {
                s.WaveSpringUpdate(springStiffness, dampening);
                s.WavePointUpdate();
            }
            UpdateSprings();
        }

        private void SetWaves()
        {
            Spline waterSpline = spriteShapeController.spline;
            int waterPointsCount = waterSpline.GetPointCount();

            // Remove middle points, keep only corners
            for (int i = CornersCount; i < waterPointsCount - CornersCount; i++)
            {
                waterSpline.RemovePointAt(CornersCount);
            }

            Vector3 left = waterSpline.GetPosition(1);
            Vector3 right = waterSpline.GetPosition(2);
            float width = right.x - left.x;
            float spacing = width / (WavesCount + 1);

            for (int i = WavesCount; i > 0; i--)
            {
                int index = CornersCount;
                float x = left.x + (spacing * i);
                Vector3 pos = new Vector3(x, left.y, left.z);

                waterSpline.InsertPointAt(index, pos);
                waterSpline.SetHeight(index, 0.1f);
                waterSpline.SetCorner(index, false);
                waterSpline.SetTangentMode(index, ShapeTangentMode.Continuous);
            }

            CreateSprings(waterSpline);
        }

        private void CreateSprings(Spline waterSpline)
        {
            springs = new();

            for (int i = 0; i <= WavesCount + 1; i++)
            {
                int index = i + 1;
                Smoothen(waterSpline, index);

                GameObject wp = Instantiate(wavePointPref, wavePoints.transform, false);
                wp.transform.localPosition = waterSpline.GetPosition(index);

                WaterSpring spring = wp.GetComponent<WaterSpring>();
                spring.Init(spriteShapeController, index);
                springs.Add(spring);
            }
        }

        private void Smoothen(Spline waterSpline, int index)
        {
            Vector3 position = waterSpline.GetPosition(index);
            Vector3 prev = index > 1 ? waterSpline.GetPosition(index - 1) : position;
            Vector3 next = index - 1 <= WavesCount ? waterSpline.GetPosition(index + 1) : position;

            Vector3 forward = transform.forward;
            float scale = Mathf.Min((next - position).magnitude, (prev - position).magnitude) * 0.33f;

            Vector3 leftTangent, rightTangent;
            SplineUtility.CalculateTangents(position, prev, next, forward, scale, out rightTangent, out leftTangent);

            waterSpline.SetLeftTangent(index, leftTangent);
            waterSpline.SetRightTangent(index, rightTangent);
        }

        private void UpdateSprings()
        {
            int count = springs.Count;
            float[] leftDeltas = new float[count];
            float[] rightDeltas = new float[count];

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    leftDeltas[i] = spread * (springs[i].height - springs[i - 1].height);
                    springs[i - 1].velocity += leftDeltas[i];
                }
                if (i < count - 1)
                {
                    rightDeltas[i] = spread * (springs[i].height - springs[i + 1].height);
                    springs[i + 1].velocity += rightDeltas[i];
                }
            }
        }

        private void Splash(int index, float speed)
        {
            if (index >= 0 && index < springs.Count)
            {
                springs[index].velocity += speed;
            }
        }

        void OnEnable()
        {
            StartCoroutine(CreateWaves());
            foreach (WaterSpring s in springs)
            {
                s.Init(spriteShapeController, 0);
            }
        }

        void OnValidate()
        {
            if (!gameObject.active) return;
            StartCoroutine(CreateWaves());
        }

        IEnumerator CreateWaves()
        {
            foreach (Transform child in wavePoints.transform)
            {
                StartCoroutine(Destroy(child.gameObject));
            }
            yield return null;
            SetWaves();
            yield return null;

            //if (springs.Count > 0)
            //{
            //    int mid = springs.Count / 2;
            //    Splash(mid, 0.05f); 
            //}
        }

        IEnumerator Destroy(GameObject go)
        {
            yield return null;
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(go);
            else Destroy(go);
#else
            Destroy(go);
#endif
        }

        public float GetWaterHeightAt(float x)
        {
            Spline spline = spriteShapeController.spline;
            for (int i = 1; i < spline.GetPointCount() - 1; i++)
            {
                Vector3 p1 = spline.GetPosition(i);
                Vector3 p2 = spline.GetPosition(i + 1);
                if (x >= p1.x && x <= p2.x)
                {
                    float t = Mathf.InverseLerp(p1.x, p2.x, x);
                    return Mathf.Lerp(p1.y, p2.y, t);
                }
            }
            return transform.position.y;
        }

        public void SplashAt(float worldX, float speed)
        {
            float localX = wavePoints.transform.InverseTransformPoint(new Vector3(worldX, 0, 0)).x;

            float nearest = float.MaxValue;
            int index = -1;
            for (int i = 0; i < springs.Count; i++)
            {
                float dist = Mathf.Abs(springs[i].transform.localPosition.x - localX);
                if (dist < nearest)
                {
                    nearest = dist;
                    index = i;
                }
            }
            if (index >= 0)
            {
                springs[index].velocity += speed * 0.005f;
            }
        }
    }
}
