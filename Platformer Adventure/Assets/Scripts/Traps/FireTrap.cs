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

    private bool isTriggered; //When the trap gets triggered
    private bool isActive; //when the trap is active and can hurt the player

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
            /*if (active)
                collision.GetComponent<Health>().TakeDamage(damage);*/
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
        //turn the sprite red to notify the player and trigger the trap
        isTriggered = true;
        spriteRenderer.color = Color.red;

        //wait for delay, activate trap, turn on animataion, return color back to normal
        yield return new WaitForSeconds(delayBeforeActivation);
        SoundManager.instance.PlaySound(firetrapSfx);
        spriteRenderer.color = Color.white; //turn the sprite red
        isActive = true;
        anim.SetBool("activated", true);

        //wait until X seconds, deactivate trap and reset all variables and animator
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
        // ha szoba kikapcsol → reset
        StopAllCoroutines();
        ResetTrap();
    }

    private void OnEnable()
    {
        // ha szoba visszakapcsol → tiszta állapot
        ResetTrap();
    }
}
