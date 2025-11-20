using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    private BossMovement boss;
    private bool hasActivated = false; //csak egyszer aktiválódjon

    void Awake()
    {
        boss = FindObjectOfType<BossMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasActivated) return; //ha már aktiválódott, kilépünk

        if (collision.CompareTag("Player"))
        {
            hasActivated = true; //most már aktiválva van

            //Player életerejének visszaállítása
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            //Boss ébresztése
            if (boss != null)
            {
                boss.WakeUp();
            }

            Debug.Log("BossRoomTrigger aktiválva!");
        }
    }

    public void ResetTriggerState()
    {
        hasActivated = false;
    }

}
