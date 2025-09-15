
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private GameObject[] roomEnemies;
    private Vector3[] initialPosition;

    void Awake()
    {
        //Save the initial positions for the enemies 
        initialPosition = new Vector3[roomEnemies.Length];
        for (int i = 0; i < roomEnemies.Length; i++)
        {
            if (roomEnemies[i] != null)
                initialPosition[i] = roomEnemies[i].transform.position;
        }
        
        //Deactivate rooms
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
                // Aktíválás / deaktiválás
                enemy.SetActive(isActive);

                // Visszaállítjuk az eredeti pozícióra
                enemy.transform.position = initialPosition[i];

                if (isActive)
                {
                    // Reseteljük a Health-et
                    Health health = enemy.GetComponent<Health>();
                    if (health != null)
                        health.ResetHealth();

                    // Reseteljük az EnemyPatrol-t (ha van)
                    EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
                    if (patrol != null)
                        patrol.enabled = true;

                    // Reseteljük az Animator-t
                    Animator anim = enemy.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.Rebind();       // újrainicializálja az animator állapotot
                        anim.Update(0f);     // frissítjük az animációt
                    }

                    // Ha van MeleeEnemy script, reseteljük a cooldown-t
                    MeleeEnemy melee = enemy.GetComponent<MeleeEnemy>();
                    if (melee != null)
                        melee.ResetEnemy();
                }
            }
    }
}

}
