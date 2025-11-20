using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform previousRoom;
    [SerializeField] private Transform nextRoom;
    [SerializeField] private CameraController cam;

    private void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.GetComponent<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bool isPlayerLeft = collision.transform.position.x < transform.position.x;

            if (isPlayerLeft)
            {
                SwitchRoom(nextRoom, previousRoom);
            }
            else
            {
                SwitchRoom(previousRoom, nextRoom);
            }
        }
    }

    private void SwitchRoom(Transform roomToActivate, Transform roomToDeactivate)
    {
        Room targetRoom = roomToActivate.GetComponent<Room>();
        Room oldRoom = roomToDeactivate.GetComponent<Room>();

        //Kamera mozgás a room kamera pontjára
        if (cam != null && targetRoom.CameraPoint != null)
            cam.MoveToNewRoom(targetRoom.CameraPoint);
        else if (cam != null)
            cam.MoveToNewRoom(roomToActivate); //fallback

        //Room aktiválás/deaktiválás
        targetRoom.ActivateRoom(true);
        oldRoom.ActivateRoom(false);
    }
}
