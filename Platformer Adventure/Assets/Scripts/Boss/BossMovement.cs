using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private BossTeleport bossTeleport;
    [SerializeField] private BossRoomController bossRoomController;
    private PlayerRespawn playerRespawn;

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
    private bool isJumping = false;
    private float originalScaleX;
    private Vector3 initialPosition;
    private Health health;
    private EnemyDamage enemyDamage;
    [SerializeField] private GameObject BossRoomDoorTerrainLeft;
    [SerializeField] private GameObject BossRoomDoorTerrainRight;

    public bool allowMovement = true;
    public bool allowDash = true;

    private BossAttackManager bossAttackManager;
    [SerializeField] private GameObject winScene;
    [SerializeField] private AudioClip winSound;

    //flag a duplikált hívások elkerülésére
    private bool hasHandledDeath = false;

    void Awake()
    {
        health = GetComponent<Health>();
        enemyDamage = FindObjectOfType<EnemyDamage>();

        playerRespawn = FindObjectOfType<PlayerRespawn>();

        if (health != null)
            health.OnDeathEvent += HandleDeath;

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        bossAttackManager = GetComponent<BossAttackManager>();

        if (bossTeleport == null)
            bossTeleport = GetComponent<BossTeleport>();

        if (bossRoomController == null)
            bossRoomController = FindObjectOfType<BossRoomController>();

        originalScaleX = Mathf.Abs(transform.localScale.x);

        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Start()
    {
        initialPosition = transform.position;
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
        if (hasHandledDeath) return;
        hasHandledDeath = true;

        isDead = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (boxCollider != null)
            boxCollider.enabled = false;

        if (bossAttackManager != null)
            bossAttackManager.enabled = false;

        if (BossRoomDoorTerrainLeft != null)
            BossRoomDoorTerrainLeft.SetActive(false);

        if (BossRoomDoorTerrainRight != null)
            BossRoomDoorTerrainRight.SetActive(false);

        if (winScene != null)
            winScene.SetActive(true);

        if (winSound != null)
            SoundManager.instance.PlaySound(winSound);
    }

    private void HandleMovement()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);
        transform.localScale = new Vector3(dx >= 0 ? originalScaleX : -originalScaleX, transform.localScale.y, transform.localScale.z);

        if (!allowMovement)
        {
            if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
            animator?.SetBool("isRunning", false);
            return;
        }

        if (distance > stopDistance)
        {
            if (rb != null) rb.velocity = new Vector2(Mathf.Sign(dx) * speed, rb.velocity.y);
            animator?.SetBool("isRunning", true);
        }
        else
        {
            if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
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
        if (boxCollider == null) return false;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }

    private bool IsProjectileInSight()
    {
        if (boxCollider == null) return false;
        Vector2 castSize = new Vector2(boxCollider.bounds.size.x + detectionRange, boxCollider.bounds.size.y);
        Vector2 direction = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        Vector2 castCenter = (Vector2)boxCollider.bounds.center + direction * (detectionRange / 2f);
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

        if (bossAttackManager != null)
        {
            bossAttackManager.enabled = true;
            bossAttackManager.WakeUp();
        }

        if (bossTeleport != null)
        {
            bossTeleport.ResetTeleportState();
            bossTeleport.ActivateTeleport();
        }

        if (bossRoomController != null)
        {
            bossRoomController.OnBossWakeUp();
        }

        if (bossHeatlhBar != null) bossHeatlhBar.SetActive(true);
        if (BossRoomDoorTerrainLeft != null) BossRoomDoorTerrainLeft.SetActive(true);
        if (BossRoomDoorTerrainRight != null) BossRoomDoorTerrainRight.SetActive(true);
    }

    public void ResetBoss()
    {
        isAwake = false;
        isDead = false;
        hasHandledDeath = false; //visszaállítjuk, hogy újra működjön

        if (health != null) health.ResetHealth();
        if (bossTeleport != null) bossTeleport.ResetTeleportState();

        transform.position = initialPosition;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3f;
        }

        if (boxCollider != null) boxCollider.enabled = true;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.ResetTrigger("WakeUp");
        }

        if (bossAttackManager != null)
            bossAttackManager.enabled = false;

        if (bossHeatlhBar != null) bossHeatlhBar.SetActive(false);
        if (BossRoomDoorTerrainLeft != null) BossRoomDoorTerrainLeft.SetActive(false);
        if (BossRoomDoorTerrainRight != null) BossRoomDoorTerrainRight.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (boxCollider != null)
                Physics2D.IgnoreCollision(collision.collider, boxCollider, true);
            if (enemyDamage != null)
                enemyDamage.TryDamage(collision.collider);
            StartCoroutine(ReenableCollision(collision.collider));
        }
    }

    private IEnumerator ReenableCollision(Collider2D playerCollider)
    {
        yield return new WaitForSeconds(1f);
        if (boxCollider != null && boxCollider.enabled)
            Physics2D.IgnoreCollision(playerCollider, boxCollider, false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enemyDamage != null)
            enemyDamage.TryDamage(collision);
    }

    public bool BossIsAwake() => isAwake;
}
