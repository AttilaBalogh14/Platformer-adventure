using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;
    private float travelDir;
    private bool hasHit;
    private float lifeTimer;

    private BoxCollider2D boxCollider;
    private Animator anim;


    void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (hasHit)
            return;

        float step = moveSpeed * Time.deltaTime * travelDir;
        transform.Translate(step, 0, 0);

        lifeTimer += Time.deltaTime;
        if (lifeTimer > 5)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint"))
            return;

        if (collision.CompareTag("Item"))
            return;

        if (collision.CompareTag("bossroom"))
            return;

        hasHit = true;
        boxCollider.enabled = false;

        if(anim != null)
            anim.SetTrigger("explode");

        if (collision.CompareTag("Enemy") || collision.CompareTag("Trap") || collision.CompareTag("Boss"))
        {
            Health hp = collision.GetComponent<Health>();
            if (hp != null)
                hp.TakeDamage(1);
        }
    }

    public void setDirection(float _direction)
    {
        lifeTimer = 0f;
        travelDir = _direction;
        hasHit = false;

        gameObject.SetActive(true);
        boxCollider.enabled = true;

        float scaleX = transform.localScale.x;
        if (Mathf.Sign(scaleX) != _direction)
        {
            scaleX = -scaleX;
        }

        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
