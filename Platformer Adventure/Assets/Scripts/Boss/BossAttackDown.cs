using UnityEngine;

public class BossAttackDown : BossAttackBase
{
    [Header("Attack Down Settings")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip shootSound;

    private Animator anim;
    private BossAttackManager manager;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>();
        manager = GetComponentInParent<BossAttackManager>();
    }

    public override void Execute(Transform playerTarget)
    {
        if (SoundManager.instance != null && shootSound != null)
            SoundManager.instance.PlaySound(shootSound);

        anim.SetTrigger("attack02");
    }

    //Animation Event hívja
    public void ShootDown()
    {
        foreach (Transform point in firePoints)
        {
            int idx = GetAvailableProjectile();
            if (idx == -1) return;

            GameObject projectile = projectiles[idx];
            projectile.SetActive(true);
            projectile.transform.SetParent(null);
            projectile.transform.localScale = Vector3.one;
            projectile.transform.position = point.position;

            Vector2 direction = point.right.normalized;
            if (point.lossyScale.x < 0)
                direction = -direction;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (point.lossyScale.x < 0)
                angle += 180f;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (projectile.TryGetComponent<BossProjectile>(out BossProjectile proj))
            {
                proj.SetDirectionAndLaunch(direction);
                proj.OnHitPlayer += () => ResolveAttack(true); //Csak találat esetén
            }
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
        float verticalDiff = player.position.y - boss.position.y;
        return verticalDiff < -1.5f ? 10f : 0f;
    }
}
