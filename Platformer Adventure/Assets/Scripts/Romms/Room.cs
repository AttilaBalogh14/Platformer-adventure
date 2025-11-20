using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Camera Focus Point")]
    [SerializeField] private Transform cameraPoint;

    [Header("Enemies")]
    [SerializeField] private GameObject[] roomEnemies;
    private Vector3[] initialPosition;

    public Transform CameraPoint => cameraPoint;

    void Awake()
    {
        initialPosition = new Vector3[roomEnemies.Length];
        for (int i = 0; i < roomEnemies.Length; i++)
        {
            if (roomEnemies[i] != null)
                initialPosition[i] = roomEnemies[i].transform.position;
        }

        if (transform.GetSiblingIndex() != 0)
            ActivateRoom(false);
    }

    public void ActivateRoom(bool isActive)
    {
        for (int i = 0; i < roomEnemies.Length; i++)
        {
            GameObject enemy = roomEnemies[i];
            if (enemy != null)
            {
                enemy.SetActive(isActive);

                enemy.transform.position = initialPosition[i];

                if (isActive)
                {
                    Health health = enemy.GetComponent<Health>();
                    if (health != null)
                        health.Respawn();

                    EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
                    if (patrol != null)
                        patrol.enabled = true;

                    Animator anim = enemy.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.Rebind();
                        anim.Update(0f);
                    }

                    MeleeEnemy melee = enemy.GetComponent<MeleeEnemy>();
                    if (melee != null)
                        melee.ResetEnemy();
                }
            }
        }
    }
}
