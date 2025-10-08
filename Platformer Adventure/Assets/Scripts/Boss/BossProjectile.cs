using UnityEngine;
using System.Collections;

public class BossProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Scale Settings")]
    [SerializeField] public float baseScaleX = 1f;
    [SerializeField] public float baseScaleY = 1f;

    [Header("References")]
    [SerializeField] private Transform player;           // Player referencia
    [SerializeField] private Animator bossAnimator;      // Boss Animator

    private Vector2 direction;
    private bool hasHit;
    private float lifeTimer;

    private BoxCollider2D boxCollider;
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (baseScaleX <= 0) baseScaleX = 1f;
        if (baseScaleY <= 0) baseScaleY = 1f;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (bossAnimator == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
                bossAnimator = boss.GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (hasHit) return;

        // Ha attacking03 aktív, frissítjük az irányt a Player felé
        if (bossAnimator != null && bossAnimator.GetBool("attacking03") && player != null)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            direction = toPlayer;
        }

        // Mozgás az aktuális direction alapján
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        lifeTimer += Time.deltaTime;
        if (lifeTimer > 5f)
            Deactivate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kizáró lista
        if (collision.CompareTag("Checkpoint") ||
            collision.CompareTag("Item") ||
            collision.CompareTag("bossroom") ||
            collision.CompareTag("Boss") ||
            collision.CompareTag("BossProjectile"))
        {
            return;
        }

        hasHit = true;
        boxCollider.enabled = false;

        if (anim != null)
            anim.SetTrigger("explode");
        else
            Deactivate();

        if (collision.CompareTag("Player"))
        {
            Health hp = collision.GetComponent<Health>();
            if (hp != null)
                hp.TakeDamage(1);
        }
    }

    public void SetDirectionAndLaunch(Vector2 _direction)
    {
        lifeTimer = 0f;
        direction = _direction.normalized;
        hasHit = false;

        // Skála tükrözés X irány szerint
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * baseScaleX, baseScaleY, 1f);
        }

        // Collider kikapcsolása spawnkor
        boxCollider.enabled = false;

        StopAllCoroutines();
        StartCoroutine(EnableColliderAfterDelay(0.1f));

        if (anim != null)
            anim.ResetTrigger("explode");
    }

    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!hasHit)
            boxCollider.enabled = true;
    }

    public void ResetProjectile()
    {
        direction = Vector2.zero;
        hasHit = false;
        lifeTimer = 0f;

        // Collider kikapcsolása
        if (boxCollider != null)
            boxCollider.enabled = false;

        // Alap scale visszaállítása
        transform.localScale = new Vector3(baseScaleX, baseScaleY, 1f);

        // Alap rotation visszaállítása
        transform.rotation = Quaternion.identity;

        // Anim trigger reset
        if (anim != null)
            anim.ResetTrigger("explode");
    }

    public void Deactivate()
    {
        StopAllCoroutines();
        ResetProjectile();
        gameObject.SetActive(false);
    }
}
