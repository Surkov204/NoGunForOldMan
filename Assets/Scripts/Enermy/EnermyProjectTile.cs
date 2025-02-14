using UnityEngine;

public class EnemyProjectile : Damaged
{
    [SerializeField] private float speed;
    [SerializeField] private float resetTime;
    private float direction;
    private float lifetime;
   
    private BoxCollider2D coll;
    private bool hit;

    private void Awake()
    {
     
        coll = GetComponent<BoxCollider2D>();
    }
    public void ActivateProjectile(float _direction)
    {
        hit = false;
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        coll.enabled = true;

        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _direction)
            localScaleX = -localScaleX;

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime* direction;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;

        base.OnTriggerEnter2D(collision);

        coll.enabled = false;

        gameObject.SetActive(false);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}