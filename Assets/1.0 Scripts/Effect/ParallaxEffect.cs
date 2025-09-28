using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [SerializeField] private Transform cam;
    private Vector3 camStartPos;
    private float distance;

    private GameObject[] backgrounds;
    private Material[] mats;
    private float[] backSpeeds;
    private float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed = 0.02f;

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        backgrounds = new GameObject[backCount];
        mats = new Material[backCount];
        backSpeeds = new float[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mats[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        CalculateBackSpeeds(backCount);
    }

    void CalculateBackSpeeds(int backCount)
    {
        for (int i = 0; i < backCount; i++)
        {
            float dist = backgrounds[i].transform.position.z - cam.position.z;
            if (dist > farthestBack) farthestBack = dist;
        }

        for (int i = 0; i < backCount; i++)
        {
            backSpeeds[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    void LateUpdate()
    {
        Parallax();
    }

    private void Parallax()
    {
        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x, transform.position.y, 0);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeeds[i] * parallaxSpeed;
            mats[i].SetTextureOffset("_MainTex", new Vector2(distance * speed, 0));
        }
    }
}