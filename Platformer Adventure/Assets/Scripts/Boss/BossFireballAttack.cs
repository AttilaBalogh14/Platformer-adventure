using UnityEngine;

public class BossFireballAttack : BossAttackBase
{
    [Header("Fireball Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip fireballSound;

    [Header("References")]
    [SerializeField] private Transform player; // Játékos referenciája

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>(); // a Boss root Animatorát keressük
    }

    public override void Execute(Transform playerTarget)
    {
        // hang lejátszás
        if (SoundManager.instance != null && fireballSound != null)
            SoundManager.instance.PlaySound(fireballSound);

        // animáció trigger
        anim.SetTrigger("attack03");
    }

    // animáció Event hívja meg, amikor a fireballt "kilövi"
    public void ShootProjectile()
    {
        int idx = GetAvailableProjectile();
        if (idx == -1)
        {
            Debug.LogWarning("Projectile Pool is empty, cannot shoot!");
            return;
        }

        GameObject projectile = projectiles[idx];
        projectile.SetActive(true);
        projectile.transform.SetParent(null);
        projectile.transform.position = firePoint.position;

        // Irány a játékos felé (ez mindig a mozgás iránya)
        Vector2 moveDirection = (player != null) ? (player.position - firePoint.position).normalized : Vector2.right;

        // Sprite forgatása a boss/firePoint flip-jéhez
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        // Ha a firePoint flip-elve van, a sprite forgatását korrigáljuk
        if (firePoint.lossyScale.x < 0)
            angle += 180f;

        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (projectile.TryGetComponent<BossProjectile>(out BossProjectile projectileScript))
        {
            // Sprite tükrözése a boss flip-hez, de a mozgás iránya változatlan
            projectile.transform.localScale = new Vector3(Mathf.Abs(projectileScript.baseScaleX) * Mathf.Sign(firePoint.lossyScale.x),
                                                        projectileScript.baseScaleY,
                                                        1f);

            // Irány továbbra is a player felé
            projectileScript.SetDirectionAndLaunch(moveDirection);
        }
    }




    private int GetAvailableProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i].activeInHierarchy)
                return i;
        }
        return -1;
    }

    public override float GetHeuristicScore(Transform player, Transform boss)
    {
        float horizontalDist = Mathf.Abs(player.position.x - boss.position.x);
        if (horizontalDist > 3f) return 8f; // ha távol van
        return 4f; // alap pont ha közel is
    }
}
