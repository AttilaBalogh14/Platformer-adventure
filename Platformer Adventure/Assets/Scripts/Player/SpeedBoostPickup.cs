using UnityEngine;

public class SpeedBoostPickup : MonoBehaviour
{
    public float duration = 3f;
    public float speedMultiplier = 1.5f;
    public float jumpMultiplier = 1.3f;
    public int extraJumps = 1;

    [SerializeField] private float respawnTime; //ennyi idő után újraspawnol

    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickupCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplySpeedBoost(duration, speedMultiplier, jumpMultiplier, extraJumps);
            }

            //pickup kikapcsolása és respawn elindítása
            StartCoroutine(RespawnCoroutine());
        }
    }

    private System.Collections.IEnumerator RespawnCoroutine()
    {
        //kikapcsoljuk a grafikát és a collidert
        spriteRenderer.enabled = false;
        pickupCollider.enabled = false;

        //várakozás
        yield return new WaitForSeconds(respawnTime);

        //újra bekapcsoljuk
        spriteRenderer.enabled = true;
        pickupCollider.enabled = true;
    }
}
