using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Room Transition Settings")]
    [SerializeField] private float smoothTime = 0.3f; //minél nagyobb, annál lassabb az átúszás
    private Vector3 targetPos;
    private Vector3 velocity = Vector3.zero;

    [Header("Optional: Player Follow")]
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float followSmooth;
    private float lookAhead;

    void Start()
    {
        float targetAspect = 16f / 9f;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = Camera.main;

        if (scaleHeight < 1.0f)
        {
            camera.orthographicSize = camera.orthographicSize / scaleHeight;
        }

        //alap cél a jelenlegi pozíció
        targetPos = transform.position;
    }

    private void Update()
    {
        //Finom átúszás a cél pozícióra
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        //Alternatív: player követés
        /*
        transform.position = new Vector3(player.position.x + lookAhead, transform.position.y, transform.position.z);
        lookAhead = Mathf.Lerp(lookAhead, aheadDistance * player.localScale.x, Time.deltaTime * followSmooth);
        */
    }

    public void MoveToNewRoom(Transform newRoomPoint)
    {
        if (newRoomPoint != null)
            targetPos = new Vector3(newRoomPoint.position.x, newRoomPoint.position.y, transform.position.z);
    }
}
