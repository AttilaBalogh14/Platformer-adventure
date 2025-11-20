using UnityEngine;

public class TeleportToBossRoom : MonoBehaviour
{
    //boss teszteléséhez használt teleportáció
    [Header("Teleport Settings")]
    [SerializeField] private Transform bossRoomSpawnPoint;
    [SerializeField] private Transform bossRoomCameraPoint;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            //Teleportáljuk a playert
            if (bossRoomSpawnPoint != null)
            {
                other.transform.position = bossRoomSpawnPoint.position;
                Debug.Log("Player teleportálva a boss szobába!");
            }
            else
            {
                Debug.LogWarning("TeleportToBossRoom: Nincs beállítva bossRoomSpawnPoint!");
            }

            //Kamera mozgatása a boss szobába
            CameraController cam = Camera.main.GetComponent<CameraController>();
            if (cam != null && bossRoomCameraPoint != null)
            {
                cam.MoveToNewRoom(bossRoomCameraPoint);
                Debug.Log("Kamera áthelyezve a boss szobára!");
            }
            else
            {
                Debug.LogWarning("TeleportToBossRoom: Nincs CameraController vagy bossRoomCameraPoint!");
            }
        }
    }
}
