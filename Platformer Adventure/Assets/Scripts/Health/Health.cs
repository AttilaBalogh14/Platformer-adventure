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
    [Header("Boss Attack Integration")]
    public BossAttackManager attackManager; // referenciát a boss AttackManager-re
    [SerializeField] private int hitsToForceAttack = 2; // hány találat után erőltetett támadás
    private int consecutiveHits = 0; // számláló az egymást követő találatokra

    private void Awake()
    {
        ResetPlayerState();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uIManager = FindObjectOfType<UIManager>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

   public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (isInvulnerable) return;

        currentHealth -= damage;

        // 🔹 Csak boss esetén növeljük a találati számlálót
        if (CompareTag("Boss") && attackManager != null)
        {
            consecutiveHits++;

            if (consecutiveHits >= hitsToForceAttack)
            {
                attackManager.ForceAttack(); // majd létrehozunk egy ilyen függvényt
                consecutiveHits = 0; // reseteljük
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

        // Player Game Over logika mindig
        if (CompareTag("Player"))
        {
            deathCount++;
            // Game Over logika: a UIManager-en futtatjuk
            if (uIManager != null)
                uIManager.StartCoroutine(uIManager.ShowGameOverScreenWithDelay(1f));
        }

        if (CompareTag("Trap"))
        {
            if (scoreValue > 0)
                ScoreEvents.AddScore(scoreValue);
            Destroy(gameObject);
            return;
        }

        // Boss logika
        if (CompareTag("Boss") && !IsGrounded())
        {
            if (deathCoroutine != null) StopCoroutine(deathCoroutine);
            deathCoroutine = StartCoroutine(DieInAirCoroutine());
        }
        else
        {
            DieOnGround();
        }

        // Score hozzáadás
        if (scoreValue > 0)
            ScoreEvents.AddScore(scoreValue);
    }

    // Új coroutine a levegőben történő halál kezelésére
    private IEnumerator DieInAirCoroutine()
    {
        OnDeathEvent?.Invoke();

        if (TryGetComponent<BossAttackManager>(out BossAttackManager bossAttackManager))
            bossAttackManager.enabled = false;

        anim.SetBool("isJumping", false);
        anim.SetBool("dead", true);

        // Lehetővé tesszük az esést
        if (rb != null)
        {
            rb.gravityScale = 3f; // vagy az eredeti gravityScale
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (col != null)
            col.enabled = true;

        // Várakozás a földre érésig
        while (!IsGrounded())
        {
            yield return null;
        }

        // Földre érve végrehajtjuk a halál logikát
        DieOnGround();
    }

    // A régi földön történő halál logika
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

        StartCoroutine(DisableAfterDelay(0.6f));
    }

    private bool IsGrounded()
    {
        if (col is BoxCollider2D box)
        {
            RaycastHit2D hit = Physics2D.BoxCast(box.bounds.center, box.bounds.size, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
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

        foreach (var comp in components)
            comp.enabled = true;

        if (anim != null)
        {
            anim.ResetTrigger("die");
            anim.Play("Idle", 0, 0f);
        }
    }

    private void ResetPlayerState()
    {
        isDead = false;
        currentHealth = startingHealth;

        foreach (var comp in components)
            comp.enabled = true;
    }

    public static int DeathCounter() => deathCount;
}
