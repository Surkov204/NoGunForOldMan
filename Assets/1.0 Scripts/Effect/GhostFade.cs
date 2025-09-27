using UnityEngine;

public class GhostFade : MonoBehaviour
{
    [SerializeField] private float fadeTime = 0.5f;
    private SpriteRenderer sr;
    private Color startColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    private void Update()
    {
        Color c = sr.color;
        c.a -= Time.deltaTime / fadeTime;
        sr.color = c;

        if (sr.color.a <= 0)
            Destroy(gameObject);
    }
}
