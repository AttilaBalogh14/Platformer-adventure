using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Projectile Avoidance")]
    [SerializeField] private LayerMask projectileLayer;
    [SerializeField] private float detectionRange = 1f;
    [SerializeField] private float colliderDistance = 0.5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float avoidCooldown = 1f;
    private float cooldownTimer = 0f;

    [SerializeField] private GameObject bossHeatlhBar;

    private bool isAwake = false;
    private bool isDead = false;
    private float originalScaleX;
    private bool isJumping = false;
    private Health health;
    private EnemyDamage enemyDamage;
    [SerializeField] private GameObject BossRoomDoorTerrainLeft;
    [SerializeField] private GameObject BossRoomDoorTerrainRight;

    // 🔹 Új flag-ek a kontrollhoz
    public bool allowMovement = true;
    public bool allowDash = true;

    void Awake()
    {
        health = GetComponent<Health>();
        enemyDamage = FindObjectOfType<EnemyDamage>();
        health.OnDeathEvent += HandleDeath;

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();

        originalScaleX = Mathf.Abs(transform.localScale.x);

        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        if (!isAwake || isDead || player == null) return;

        HandleMovement();
        HandleProjectileAvoidance();
        HandleJumpAnimation();
    }

    private void HandleDeath()
    {
        isDead = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (boxCollider != null) boxCollider.enabled = false;

        if (TryGetComponent<BossAttackManager>(out BossAttackManager bossAttackManager))
            bossAttackManager.enabled = false;

        Debug.Log("BossMovement: Boss meghalt, mozgás leállítva.");
        BossRoomDoorTerrainLeft.SetActive(false);
        BossRoomDoorTerrainRight.SetActive(false);
    }

    private void HandleMovement()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);

        // 🔹 Player felé fordulás mindig
        transform.localScale = new Vector3(dx >= 0 ? originalScaleX : -originalScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        // Ha a mozgás nincs engedélyezve, ne lépjünk tovább
        if (!allowMovement) 
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator?.SetBool("isRunning", false);
            return;
        }

        if (distance > stopDistance)
        {
            rb.velocity = new Vector2(Mathf.Sign(dx) * speed, rb.velocity.y);
            animator?.SetBool("isRunning", true);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator?.SetBool("isRunning", false);
        }
    }

    private void HandleProjectileAvoidance()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= avoidCooldown && IsProjectileInSight() && IsGrounded())
        {
            JumpAway();
            cooldownTimer = 0f;
        }
    }

    private void HandleJumpAnimation()
    {
        if (animator == null || rb == null) return;

        bool grounded = IsGrounded();

        if (!grounded && !isJumping && rb.velocity.y > 0)
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
        }
        else if (grounded && isJumping)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }

    private bool IsProjectileInSight()
    {
        if (boxCollider == null) return false;

        Vector2 castSize = new Vector2(boxCollider.bounds.size.x + detectionRange, boxCollider.bounds.size.y);
        Vector2 direction = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        Vector2 castCenter = (Vector2)boxCollider.bounds.center + direction * (castSize.x / 2);

        Collider2D hit = Physics2D.OverlapBox(castCenter, castSize, 0f, projectileLayer);
        return hit != null && hit.CompareTag("Fireball");
    }

    private void JumpAway()
    {
        if (!allowMovement || rb == null) return;

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    public void WakeUp()
    {
        if (isAwake) return;

        isAwake = true;
        animator?.SetTrigger("WakeUp");
        Debug.Log("Boss felébredt!");

        if (TryGetComponent<BossAttackManager>(out BossAttackManager bossAttackManager))
            bossAttackManager.WakeUp();

        bossHeatlhBar.SetActive(true);
        BossRoomDoorTerrainLeft.SetActive(true);
        BossRoomDoorTerrainRight.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        if (boxCollider != null)
        {
            Vector3 castOrigin = boxCollider.bounds.center + transform.right * detectionRange * transform.localScale.x * colliderDistance;
            Vector3 castSize = new Vector3(boxCollider.bounds.size.x * detectionRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(castOrigin, castSize);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(collision.collider, boxCollider, true);

            if (enemyDamage != null)
                enemyDamage.TryDamage(collision.collider);

            StartCoroutine(ReenableCollision(collision.collider));
        }
    }

    private IEnumerator ReenableCollision(Collider2D playerCollider)
    {
        yield return new WaitForSeconds(1f);
        Physics2D.IgnoreCollision(playerCollider, boxCollider, false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enemyDamage != null)
        {
            enemyDamage.TryDamage(collision);
        }
    }
}
