using UnityEngine;
using System.Collections;

public class BossDashAttack : BossAttackBase
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private AudioClip dashSound;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;

    private Animator anim;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float lastDashTime = -999f;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>();
        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();
    }

    public override void Execute(Transform playerTarget)
    {
        if (GetComponentInParent<BossMovement>() is BossMovement movement && !movement.allowDash) return;
        if (Time.time < lastDashTime + dashCooldown || isDashing) return;

        player = playerTarget;
        anim.SetTrigger("dashattack");
        if (SoundManager.instance != null && dashSound != null) SoundManager.instance.PlaySound(dashSound);
        StartCoroutine(PerformDash());
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashTimer = 0f;
        lastDashTime = Time.time;

        Vector2 direction = player != null ? (player.position - rb.transform.position).normalized : Vector2.right;
        float facing = Mathf.Sign(direction.x);
        Vector3 localScale = rb.transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * facing;
        rb.transform.localScale = localScale;

        while (dashTimer < dashDuration && isDashing)
        {
            rb.velocity = new Vector2(direction.x * dashSpeed, rb.velocity.y);
            dashTimer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = new Vector2(0, rb.velocity.y);
        isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isDashing = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Health>()?.TakeDamage(1);
            ResolveAttack(true);
        }
    }

    public override float GetHeuristicScore(Transform player, Transform boss)
    {
        float horizontalDist = Mathf.Abs(player.position.x - boss.position.x);
        return horizontalDist < 4f ? 9f : 3f;
    }
}
