using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 0.5f; //mennyi idő múlva essen le
    [SerializeField] private float destroyDelay = 2f; //mennyi idő múlva semmisüljön meg

    private Rigidbody2D rb;
    private bool isFalling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && collision.gameObject.CompareTag("Player"))
        {
            isFalling = true;
            Invoke(nameof(StartFalling), fallDelay);
        }
    }

    private void StartFalling()
    {
        rb.bodyType = RigidbodyType2D.Dynamic; //mostantól a fizika irányítja
        Destroy(gameObject, destroyDelay);
    }
}
