using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] arrows;
    private float cooldownTimer;

    [Header("Audio")]
    [SerializeField] private AudioClip arrowSfx;

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= attackCooldown)
            ShootArrow();
    }

    private void ShootArrow()
    {
        cooldownTimer = 0f;

        SoundManager.instance.PlaySound(arrowSfx);
        GameObject arrow = arrows[FindInactiveArrow()];
        arrow.transform.position = spawnPoint.position;
        arrow.GetComponent<EnemyProjectile>().ActivateProjectile();
    }

    private int FindInactiveArrow()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (!arrows[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

}
