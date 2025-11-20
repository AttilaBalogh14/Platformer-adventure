using UnityEngine;

public class BossForceAttack : MonoBehaviour
{
    [Header("Forced Attack Settings")]
    [SerializeField] private Transform[] firePoints;      // pl. 8 firePoint
    [SerializeField] private GameObject[] projectiles;    // lövedék pool
    [SerializeField] private AudioClip shootSound;

    private void Awake()
    {
        if (firePoints.Length == 0)
            Debug.LogWarning("Nincsenek firePoint-ok beállítva!");
        if (projectiles.Length == 0)
            Debug.LogWarning("Nincsenek projectiles beállítva!");
    }

    public void Execute()
    {
        if (SoundManager.instance != null && shootSound != null)
            SoundManager.instance.PlaySound(shootSound);

        //Lövés minden firePoint-ról
        foreach (Transform point in firePoints)
        {
            int idx = GetAvailableProjectile();
            if (idx == -1)
            {
                Debug.LogWarning("Projectile pool is empty!");
                continue;
            }

            GameObject projectile = projectiles[idx];
            projectile.SetActive(true);
            projectile.transform.SetParent(null);
            projectile.transform.position = point.position;
            projectile.transform.localScale = Vector3.one;

            //Lövedék iránya a firePoint aktuális iránya szerint
            Vector2 direction = point.right.normalized;
            if (point.lossyScale.x < 0)
                direction = -direction;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (point.lossyScale.x < 0)
                angle += 180f;

            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (projectile.TryGetComponent<BossProjectile>(out BossProjectile projScript))
                projScript.SetDirectionAndLaunch(direction);
        }
    }

    private int GetAvailableProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
            if (!projectiles[i].activeInHierarchy)
                return i;
        return -1;
    }
}
