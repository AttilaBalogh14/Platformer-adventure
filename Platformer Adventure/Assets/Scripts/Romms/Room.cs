using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Camera Focus Point")]
    [SerializeField] private Transform cameraPoint;   // ➜ ezt add hozzá az Inspectorban (Empty object a szoba közepén)

    [Header("Enemies")]
    [SerializeField] private GameObject[] roomEnemies;
    private Vector3[] initialPosition;

    public Transform CameraPoint => cameraPoint;

    void Awake()
    {
        // Save the initial positions for the enemies
        initialPosition = new Vector3[roomEnemies.Length];
        for (int i = 0; i < roomEnemies.Length; i++)
        {
            if (roomEnemies[i] != null)
                initialPosition[i] = roomEnemies[i].transform.position;
        }

        // Deactivate rooms except the first one
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
                // Set enemy active or inactive
                enemy.SetActive(isActive);

                // Reset position
                enemy.transform.position = initialPosition[i];

                if (isActive)
                {
                    // 🔹 Respawn Health instead of ResetHealth
                    Health health = enemy.GetComponent<Health>();
                    if (health != null)
                        health.Respawn();

                    // Reset EnemyPatrol if exists
                    EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
                    if (patrol != null)
                        patrol.enabled = true;

                    // Reset Animator
                    Animator anim = enemy.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.Rebind();       // reset animator state
                        anim.Update(0f);     // refresh animation
                    }

                    // Reset MeleeEnemy cooldown or state
                    MeleeEnemy melee = enemy.GetComponent<MeleeEnemy>();
                    if (melee != null)
                        melee.ResetEnemy();
                }
            }
        }
    }
}
