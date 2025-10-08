using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    private BossMovement boss; // Húzd be az Inspectorban a boss Movement scriptét

    void Awake()
    {
        boss = FindObjectOfType<BossMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            boss.WakeUp();
        }
    }
}
