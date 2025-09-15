using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float startingHealth = 100;
    public float currentHealth;

    private bool isDead;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    [Header("Invulnerability")]
    public float iFramesDuration = 0.5f;
    public int flashCount = 3;
    private bool isInvulnerable;

    private Coroutine hurtCoroutine;
    private Coroutine powerupCoroutine;

    [Header("Components")]
    public Behaviour[] components;

    [Header("Audio")]
    public AudioClip hurtClip;
    public AudioClip deathClip;

    [Header("Score")]
    public int scoreValue = 0;

    private UIManager uIManager;

    private void Awake()
    {
        ResetPlayerState();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uIManager = FindObjectOfType<UIManager>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            Debug.Log("Nem sebződik, mert már halott.");
            return;
        }

        if (isInvulnerable)
        {
            Debug.Log("Nem sebződik, mert invulnerable.");
            return;
        }

        currentHealth -= damage;
        Debug.Log("Sebzés: -" + damage + " | Jelenlegi élet: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Élet <= 0, meghívjuk a Die()-t!");
            Die();
        }
        else
        {
            anim.SetTrigger("hurt");
            StartHurtInvulnerability();
            if (hurtClip != null)
                SoundManager.instance.PlaySound(hurtClip);
        }
    }

    public bool CanGetDamage()
    {
        return !isInvulnerable;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        foreach (var comp in components)
            comp.enabled = false;

        anim.ResetTrigger("die");
        anim.SetTrigger("die");
        Debug.Log("Die trigger elküldve az Animatornak!");
        if (deathClip != null)
            SoundManager.instance.PlaySound(deathClip);

        if (scoreValue > 0)
        {
            ScoreEvents.AddScore(scoreValue);
        }
        else if (CompareTag("Player"))
        {
            StartCoroutine(ShowGameOverScreenWithDelay(1f));
        }
    }

    private IEnumerator ShowGameOverScreenWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (uIManager != null)
        {
            uIManager.gameOverScreen.SetActive(true);
        }
        Time.timeScale = 0;
    }

    // Sebződés utáni sebezhetetlenség
    public void StartHurtInvulnerability()
    {
        // Csak akkor induljon el, ha nincs pajzs aktívan
        if (!isInvulnerable && powerupCoroutine == null)
        {
            if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
            hurtCoroutine = StartCoroutine(HurtInvulnerabilityCoroutine());
        }
    }

    // Pajzs pickup indítása
    public void StartPowerupInvulnerability(float duration)
    {
        // Leállítjuk a villogást teljesen
        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }

        // Ha már fut egy pajzs, újraindítjuk
        if (powerupCoroutine != null) StopCoroutine(powerupCoroutine);
        powerupCoroutine = StartCoroutine(InvulnerabilityPowerupCoroutine(duration));
    }

    private IEnumerator HurtInvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (flashCount * 2));
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (flashCount * 2));
        }

        isInvulnerable = false;
        hurtCoroutine = null;
    }

    private IEnumerator InvulnerabilityPowerupCoroutine(float duration)
    {
        isInvulnerable = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // Pajzs effekt

        yield return new WaitForSeconds(duration);

        spriteRenderer.color = Color.white; // Visszaáll normálra
        isInvulnerable = false;
        powerupCoroutine = null;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void AddHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > startingHealth)
            currentHealth = startingHealth;
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = startingHealth;
        anim.ResetTrigger("die");
        anim.Play("Idle");

        foreach (var comp in components)
            comp.enabled = true;

        StartHurtInvulnerability();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = startingHealth;
        foreach (Behaviour comp in components)
        {
            comp.enabled = true;
        }
    }

    private void ResetPlayerState()
    {
        isDead = false;
        currentHealth = startingHealth;

        foreach (Behaviour comp in components)
        {
            comp.enabled = true;
        }
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
