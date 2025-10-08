using UnityEngine;

public class BossAttackDown : BossAttackBase
{
    [Header("Attack Down Settings")]
    [SerializeField] private Transform[] firePoints;   // Több firePoint (pl. 3 db)
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

        // Animáció trigger
        anim.SetTrigger("attack02");
    }

    // Animation Event hívja (amikor ténylegesen kilövi a támadást)
    public void ShootDown()
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

            // 🔹 Lövedék iránya — a firePoint aktuális irányát követi.
            Vector2 direction = point.right.normalized;
            // Ha a firePoint vagy a boss flip-elve van X tengelyen, tükrözzük a vektort
            if (point.lossyScale.x < 0)
                direction = -direction;

            // 🔹 Sprite forgatás az irány alapján
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Ha a firePoint flipelve van, korrigáljuk a szöget (ugyanúgy, mint az Up verzióban)
            if (point.lossyScale.x < 0)
                angle += 180f;

            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 🔹 Lövedék indítása
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
        if (verticalDiff < -1.5f) return 10f; // ha a player alatta van
        return 0f;
    }
}
