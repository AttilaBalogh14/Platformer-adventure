using System.Collections;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackDamage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    [Header("Attack Sound")]
    [SerializeField] private AudioClip attackSfx;

    //References
    private Animator anim;
    private Health playerHealth;
    private EnemyPatrol enemyPatrol;
    private Health selfHealth; //saját health

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        selfHealth = GetComponent<Health>(); //enemy saját health-je
    }

    private void OnEnable()
    {
        cooldownTimer = Mathf.Infinity;

        if (selfHealth != null)
        {
            //Kis késleltetéssel, hogy minden komponens aktív legyen
            StartCoroutine(ResetHealthNextFrame());
        }
    }

    private IEnumerator ResetHealthNextFrame()
    {
        yield return null; //vár egy frame-et
        selfHealth.ResetHealth();
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            if (cooldownTimer >= attackCooldown && playerHealth != null && playerHealth.currentHealth > 0)
            {
                cooldownTimer = 0f;
                anim.SetTrigger("meleeAttack");
                SoundManager.instance.PlaySound(attackSfx);
            }
        }

        if (enemyPatrol != null)
            enemyPatrol.enabled = !PlayerInSight();
    }

    private bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + transform.right * attackRange * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * attackRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0f,
            Vector2.left,
            0f,
            playerLayer
        );

        if (hit.collider != null)
        {
            playerHealth = hit.collider.GetComponent<Health>();
            return playerHealth != null;
        }

        playerHealth = null;
        return false;
    }

    private void DamagePlayer()
    {
        if (playerHealth != null && PlayerInSight() && playerHealth.CanGetDamage())
            playerHealth.TakeDamage(attackDamage);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * attackRange * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * attackRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z)
        );
    }

    public void ResetEnemy()
    {
        cooldownTimer = Mathf.Infinity;

        //Reseteljük a saját Health-et
        if (selfHealth != null)
            selfHealth.ResetHealth();

        //Engedélyezzük a patrol-t újra
        if (enemyPatrol != null)
            enemyPatrol.enabled = true;

        //Reseteljük az Animator-t
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }

}
