using UnityEngine;

public class BossAttackUp : BossAttackBase
{
    [Header("Attack Up Settings")]
    [SerializeField] private Transform[] firePoints;   // Több firePoint
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip shootSound;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInParent<Animator>(); // Boss root animator
    }

    public override void Execute(Transform playerTarget)
    {
        // Hang lejátszás
        if (SoundManager.instance != null && shootSound != null)
            SoundManager.instance.PlaySound(shootSound);

        // Csak triggerrel indítjuk az animációt
        anim.SetTrigger("attack01");
    }

    // Animation Event hívja (amikor ténylegesen kilövi a támadást)
    public void ShootUp()
    {
        foreach (Transform point in firePoints)
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
            projectile.transform.localScale = Vector3.one;
            projectile.transform.position = point.position;

            // Lövedék iránya
            Vector2 direction = point.right.normalized;
            if (point.lossyScale.x < 0)
                direction = -direction;

            // Forgatás a sprite-nak, figyelve a boss flip-re
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Ha a boss flip-elve van, fordítsuk meg a sprite-ot
            if (point.lossyScale.x < 0)
                angle += 180f;

            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (projectile.TryGetComponent<BossProjectile>(out BossProjectile projectileScript))
                projectileScript.SetDirectionAndLaunch(direction);
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
        float verticalDiff = player.position.y - boss.position.y;
        if (verticalDiff > 0.5f) return 10f; // korábban 5, most nagyobb
        return 2f; // alap pont, hogy legyen esélye
    }

}
