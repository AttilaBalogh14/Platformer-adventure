using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float detectionRange;
    [SerializeField] private int attackDamage;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] projectiles; 

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    [Header("Fireball Sound")]
    [SerializeField] private AudioClip fireballSound;

    //References
    private Animator anim;
    private EnemyPatrol enemyPatrol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (IsPlayerInSight())
        {
            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0f;
                anim.SetTrigger("rangedAttack");
            }
        }

        if (enemyPatrol != null)
            enemyPatrol.enabled = !IsPlayerInSight();

    }

    private void RangedAttack()
    {
        SoundManager.instance.PlaySound(fireballSound);

        cooldownTimer = 0f;

        GameObject projectile = projectiles[FindInactiveProjectile()];
        projectile.transform.position = firepoint.position;
        projectile.GetComponent<EnemyProjectile>().ActivateProjectile();
    }

    private int FindInactiveProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    private bool IsPlayerInSight()
    {
        Vector3 castOrigin = boxCollider.bounds.center + transform.right * detectionRange * transform.localScale.x * colliderDistance;
        Vector3 castSize = new Vector3(boxCollider.bounds.size.x * detectionRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z);

        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, castSize, 0f, Vector2.left, 0f, playerLayer);
        return hit.collider != null;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * detectionRange * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * detectionRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }
}
