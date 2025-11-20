using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [SerializeField] private float damageAmount;

    [Header("Firetrap Timers")]
    [SerializeField] private float delayBeforeActivation;
    [SerializeField] private float activeDuration;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isTriggered;
    private bool isActive;

    private Health playerHealth;

    [Header("SFX")]
    [SerializeField] private AudioClip firetrapSfx;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerHealth != null && isActive)
        {
            playerHealth.TakeDamage(damageAmount);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth = collision.GetComponent<Health>();

            if (!isTriggered)
            {
                StartCoroutine(ActivateTrap());
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth = null;
        }
    }

    private IEnumerator ActivateTrap()
    {
        isTriggered = true;
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(delayBeforeActivation);
        SoundManager.instance.PlaySound(firetrapSfx);
        spriteRenderer.color = Color.white;
        isActive = true;
        anim.SetBool("activated", true);

        yield return new WaitForSeconds(activeDuration);

        ResetTrap();
    }
    
    private void ResetTrap()
    {
        isActive = false;
        isTriggered = false;
        anim.SetBool("activated", false);
        spriteRenderer.color = Color.white;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ResetTrap();
    }

    private void OnEnable()
    {
        ResetTrap();
    }
}
