using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        if (collision.tag == "Player")
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
        if (cam != null)
            cam.MoveToNewRoom(roomToActivate);

        roomToActivate.GetComponent<Room>().ActivateRoom(true);
        roomToDeactivate.GetComponent<Room>().ActivateRoom(false);
    }
}
