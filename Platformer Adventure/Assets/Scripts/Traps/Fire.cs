using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private float damageAmount;

    private Health playerHealth;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    [Header("SFX")]
    [SerializeField] private AudioClip firetrapSfx;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Aktiváljuk az animációt rögtön
        if (anim != null)
            anim.SetBool("activated", true);

        // Szín visszaállítása normálra
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    void Update()
    {
        // Aktiváljuk az animációt rögtön
        if (anim != null)
            anim.SetBool("activated", true);

        // Szín visszaállítása normálra
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount); 
            // folyamatos sebzés frame-enként (adjustálhatod)
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth = null;
        }
    }
}
