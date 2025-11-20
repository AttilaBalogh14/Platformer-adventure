using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float startingHealth;
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
    private Coroutine deathCoroutine;

    [Header("Components")]
    public Behaviour[] components;

    [Header("Audio")]
    public AudioClip hurtClip;
    public AudioClip deathClip;

    [Header("Score")]
    public int scoreValue = 0;

    private UIManager uIManager;
    private PlayerMovement playerMovement;

    private static int deathCount = 0;

    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeathEvent;

    private Rigidbody2D rb;
    private Collider2D col;

    //Eredeti Rigidbody értékek mentése
    private float originalGravityScale;
    private RigidbodyConstraints2D originalConstraints;

    [Header("Boss Attack Integration")]
    public BossAttackManager attackManager;
    [SerializeField] private int hitsToForceAttack = 2;
    private int consecutiveHits = 0;
    private bool isGameOver = false;

    private void Awake()
    {
        ResetPlayerState();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uIManager = FindObjectOfType<UIManager>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        isGameOver = false;

        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
            originalConstraints = rb.constraints;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        currentHealth -= damage;

        if (CompareTag("Boss") && attackManager != null)
        {
            consecutiveHits++;
            if (consecutiveHits >= hitsToForceAttack)
            {
                attackManager.ForceAttack();
                consecutiveHits = 0;
            }
        }

        if (currentHealth <= 0)
            Die();
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

        if (CompareTag("Player"))
        {
            //Nyissuk ki az összes szobát halálkor (inaktívakat is megtaláljuk)
            foreach (var roomActivator in FindObjectsOfType<ActivateRoomPoint>(true))
            {
                if (roomActivator != null && roomActivator.objectToShow != null)
                {
                    roomActivator.objectToShow.SetActive(false);
                }
            }

            deathCount++;
            isGameOver = true;

            if (uIManager != null)
            {
                //Tiltsuk le a pause-t halál után
                uIManager.DisablePause();
                uIManager.StartCoroutine(uIManager.ShowGameOverScreenWithDelay(1f));
            }

            //Boss resetelése, ha aktív
            BossMovement boss = FindObjectOfType<BossMovement>();
            if (boss != null && boss.BossIsAwake())
                boss.ResetBoss();

            Debug.Log("All rooms opened after player death!");
        }

        if (CompareTag("Trap"))
        {
            if (scoreValue > 0)
                ScoreEvents.AddScore(scoreValue);
            Destroy(gameObject);
            return;
        }

        if (CompareTag("Boss") && !IsGrounded())
        {
            if (deathCoroutine != null) StopCoroutine(deathCoroutine);
            deathCoroutine = StartCoroutine(DieInAirCoroutine());
        }
        else
        {
            DieOnGround();
        }

        if (scoreValue > 0)
            ScoreEvents.AddScore(scoreValue);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    private IEnumerator DieInAirCoroutine()
    {
        OnDeathEvent?.Invoke();

        if (TryGetComponent<BossAttackManager>(out BossAttackManager bossAttackManager))
            bossAttackManager.enabled = false;

        anim.SetBool("isJumping", false);
        anim.SetBool("dead", true);

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (col != null)
            col.enabled = true;

        while (!IsGrounded())
            yield return null;

        DieOnGround();
    }

    private void DieOnGround()
    {
        OnDeathEvent?.Invoke();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (col != null)
            col.enabled = false;

        if (TryGetComponent<BossAttackManager>(out BossAttackManager bossAttackManager))
            bossAttackManager.enabled = false;

        anim.SetBool("isJumping", false);
        anim.SetBool("dead", true);
        anim.SetBool("grounded", true);
        anim.SetTrigger("die");

        if (deathClip != null)
            SoundManager.instance.PlaySound(deathClip);

        foreach (var comp in components)
            comp.enabled = false;

        if (!CompareTag("Player"))
            StartCoroutine(DisableAfterDelay(0.6f));
    }

    private bool IsGrounded()
    {
        if (col == null) return true;

        if (col is BoxCollider2D box)
        {
            RaycastHit2D hit = Physics2D.BoxCast(
                box.bounds.center, box.bounds.size, 0f,
                Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
            return hit.collider != null;
        }
        return true;
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private IEnumerator ShowGameOverScreenWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (uIManager != null)
            uIManager.gameOverScreen.SetActive(true);

        Time.timeScale = 0;
    }

    public void StartHurtInvulnerability()
    {
        if (!isInvulnerable && powerupCoroutine == null)
        {
            if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
            hurtCoroutine = StartCoroutine(HurtInvulnerabilityCoroutine());
        }
    }

    public void StartPowerupInvulnerability(float duration)
    {
        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }

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
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = Color.white;
        isInvulnerable = false;
        powerupCoroutine = null;
    }

    public bool IsDead() => isDead;

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

        if (anim != null)
        {
            anim.ResetTrigger("die");
            anim.SetBool("dead", false);
            anim.Play("Idle", 0, 0f);
        }

        if (rb != null)
        {
            rb.constraints = originalConstraints;
            rb.gravityScale = originalGravityScale;
            rb.velocity = Vector2.zero;
        }

        if (col != null)
            col.enabled = true;

        foreach (var comp in components)
            comp.enabled = true;

        StartHurtInvulnerability();

        //Pause újra engedélyezése
        if (uIManager != null)
            uIManager.EnablePause();

        foreach (var trigger in FindObjectsOfType<BossRoomTrigger>())
        {
            trigger.ResetTriggerState();
        }

    }


    public void ResetHealth()
    {
        isDead = false;
        currentHealth = startingHealth;

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }

        foreach (var comp in components)
            comp.enabled = true;

        if (anim != null)
        {
            anim.ResetTrigger("die");
            anim.SetBool("dead", false);
            anim.SetBool("isJumping", false);
            anim.SetBool("grounded", true);
            anim.Play("Idle", 0, 0f);
        }

        if (rb != null)
        {
            rb.constraints = originalConstraints;
            rb.gravityScale = originalGravityScale;
            rb.velocity = Vector2.zero;
        }

        if (col != null)
            col.enabled = true;

        consecutiveHits = 0;
    }

    private void ResetPlayerState()
    {
        isDead = false;
        currentHealth = startingHealth;

        foreach (var comp in components)
            comp.enabled = true;
    }

    public static int DeathCounter() => deathCount;

    #if UNITY_EDITOR
    public void TestDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        if (currentHealth == 0)
        {
            typeof(Health).GetField("isDead",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, true);
        }
    }
    #endif
}
