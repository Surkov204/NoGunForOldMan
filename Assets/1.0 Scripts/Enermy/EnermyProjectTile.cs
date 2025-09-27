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
    public void ActivateProjectile(float _direction, Transform gunScaletransform)
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
        transform.rotation = gunScaletransform.rotation;
    }

    private void Update()
    {
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

    }
}