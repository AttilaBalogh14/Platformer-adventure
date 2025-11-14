using UnityEngine;

public class BossFireballAttack : BossAttackBase
{
    [Header("Fireball Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip fireballSound;

    [Header("References")]
    [SerializeField] private Transform player;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>();
    }

    public override void Execute(Transform playerTarget)
    {
        if (SoundManager.instance != null && fireballSound != null)
            SoundManager.instance.PlaySound(fireballSound);

        anim.SetTrigger("attack03");
    }

    public void ShootProjectile()
    {
        int idx = GetAvailableProjectile();
        if (idx == -1) return;

        GameObject projectile = projectiles[idx];
        projectile.SetActive(true);
        projectile.transform.SetParent(null);
        projectile.transform.position = firePoint.position;

        Vector2 moveDirection = (player != null) ? (player.position - firePoint.position).normalized : Vector2.right;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        if (firePoint.lossyScale.x < 0) angle += 180f;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (projectile.TryGetComponent<BossProjectile>(out BossProjectile proj))
        {
            proj.OnHitPlayer += () => ResolveAttack(true);
            proj.SetDirectionAndLaunch(moveDirection);
            projectile.transform.localScale = new Vector3(Mathf.Abs(proj.baseScaleX) * Mathf.Sign(firePoint.lossyScale.x),
                                                         proj.baseScaleY, 1f);
        }
    }

    private int GetAvailableProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
            if (!projectiles[i].activeInHierarchy)
                return i;
        return -1;
    }

    public override float GetHeuristicScore(Transform player, Transform boss)
    {
        float horizontalDist = Mathf.Abs(player.position.x - boss.position.x);
        return horizontalDist > 3f ? 8f : 4f;
    }
}
