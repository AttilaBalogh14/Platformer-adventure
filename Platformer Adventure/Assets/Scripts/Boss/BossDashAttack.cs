using UnityEngine;

public class BossDashAttack : BossAttackBase
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;       // milyen gyorsan dash-el a boss
    [SerializeField] private float dashDuration = 0.4f;   // meddig tart a dash
    [SerializeField] private float dashCooldown = 2f;     // két dash között mennyi idő teljen el
    [SerializeField] private AudioClip dashSound;         // opcionális hang

    [Header("References")]
    [SerializeField] private Transform player;            // a player referenciája
    [SerializeField] private Rigidbody2D rb;              // a boss rigidbody-ja

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
        // 🔹 Ha a dash tiltva, ne fusson
        BossMovement movement = GetComponentInParent<BossMovement>();
        if (movement != null && !movement.allowDash)
            return;

        if (Time.time < lastDashTime + dashCooldown || isDashing)
            return; // még cooldown-on van

        player = playerTarget;

        // Animáció trigger
        anim.SetTrigger("dashattack");

        // Hang lejátszás (ha van)
        if (SoundManager.instance != null && dashSound != null)
            SoundManager.instance.PlaySound(dashSound);

        StartCoroutine(PerformDash());
    }


    private System.Collections.IEnumerator PerformDash()
    {
        isDashing = true;
        dashTimer = 0f;
        lastDashTime = Time.time;

        // Meghatározzuk a mozgás irányát a játékos felé
        Vector2 direction = Vector2.right; // alapértelmezett jobbra
        if (player != null)
            direction = (player.position - rb.transform.position).normalized;

        // sprite flip-hez igazítjuk az irányt
        float facing = Mathf.Sign(direction.x);
        Vector3 localScale = rb.transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * facing;
        rb.transform.localScale = localScale;

        // Addig dash-el, amíg tart az idő és nincs ütközés
        while (dashTimer < dashDuration && isDashing)
        {
            rb.velocity = new Vector2(direction.x * dashSpeed, rb.velocity.y);
            dashTimer += Time.deltaTime;
            yield return null;
        }

        // Dash vége — leállítjuk a mozgást
        rb.velocity = new Vector2(0, rb.velocity.y);
        isDashing = false;
    }

    // Ütközés kezelése
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing)
            return;

        // Ha fal vagy akadály (pl. Layer alapján), megáll a dash
        // Példa: ha a fal Layer neve "Ground"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isDashing = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public override float GetHeuristicScore(Transform player, Transform boss)
    {
        float horizontalDist = Mathf.Abs(player.position.x - boss.position.x);
        if (horizontalDist < 4f) return 9f; // közelebb, nagyobb pont
        return 3f; // távol kevésbé hatékony
    }


}
