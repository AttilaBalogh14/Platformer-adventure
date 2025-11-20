using UnityEngine;

public class EnemyProjectile : EnemyDamage
{
    [SerializeField] private float speed;
    [SerializeField] private float resetTime = 5f;
    private float lifetime;

    private Animator anim;
    private BoxCollider2D coll;
    private Rigidbody2D rb;

    private bool hasHit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void ActivateProjectile()
    {
        hasHit = false;
        lifetime = 0f;

        gameObject.SetActive(true);

        coll.enabled = true;

        //Ha Rigidbody van, reseteljük a sebességet
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (hasHit) return;

        float movement = speed * Time.deltaTime;
        transform.Translate(movement, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > resetTime)
            Deactivate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint"))
            return;

        if (collision.CompareTag("Item"))
            return;

        hasHit = true;
        coll.enabled = false;

        base.OnTriggerEnter2D(collision);

        if (anim != null)
        {
            anim.SetTrigger("explode");
        }
        else
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        hasHit = false;
        gameObject.SetActive(false);
    }
}
