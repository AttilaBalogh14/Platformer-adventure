using UnityEngine;

public class CameraController : MonoBehaviour
{
    // --- Room camera ---
    [SerializeField] private float smoothTime;  // minél nagyobb, annál lassabb az átúszás
    private float targetPosX;
    private Vector3 velocity = Vector3.zero;

    // --- Follow player (opcionális) ---
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float followSmooth;
    private float lookAhead;

    private void Update()
    {
        // Finom átúszás az aktuális cél X-pozícióhoz
        Vector3 targetPosition = new Vector3(targetPosX, transform.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // --- Alternatív: követés ---
        /*
        transform.position = new Vector3(player.position.x + lookAhead, transform.position.y, transform.position.z);
        lookAhead = Mathf.Lerp(lookAhead, aheadDistance * player.localScale.x, Time.deltaTime * followSmooth);
        */
    }

    public void MoveToNewRoom(Transform newRoom)
    {
        // Beállítjuk az új room X-pozícióját, SmoothDamp szépen átúsztatja
        targetPosX = newRoom.position.x;
    }
}
