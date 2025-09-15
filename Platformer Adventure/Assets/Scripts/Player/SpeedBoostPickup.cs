using UnityEngine;

public class SpeedBoostPickup : MonoBehaviour
{
    public float duration = 3f;
    public float speedMultiplier = 1.5f;
    public float jumpMultiplier = 1.3f;
    public int extraJumps = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplySpeedBoost(duration, speedMultiplier, jumpMultiplier, extraJumps);
            }

            Destroy(gameObject);
        }
    }
}
