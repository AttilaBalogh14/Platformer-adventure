using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float shootDelay;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip fireballSound;
    private Animator anim;
    private PlayerMovement playerMovement;
    private float timerSinceLastShot = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && timerSinceLastShot > shootDelay || Input.GetKeyDown(KeyCode.Space) && timerSinceLastShot > shootDelay/*&& playerMovement.canAttack()*/)
        {
            Attack();
        }

        timerSinceLastShot += Time.deltaTime;
    }

    private void Attack()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlaySound(fireballSound);

        anim.SetTrigger("attack");
        timerSinceLastShot = 0;

        //pool fireballs
        int idx = GetAvailableProjectile();
        projectiles[idx].transform.position = firePoint.position;

        projectiles[idx].GetComponent<Projectile>().setDirection(Mathf.Sign(transform.localScale.x));
    }

    private int GetAvailableProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

}
