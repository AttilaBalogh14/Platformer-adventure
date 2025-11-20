using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] protected float damage = 1f;

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryDamage(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryDamage(collision);
        }
    }

    public void TryDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();
        if (health != null && health.CanGetDamage()) //Csak akkor sebez, ha nincs i-frame
        {
            health.TakeDamage(damage);
        }
    }
}
