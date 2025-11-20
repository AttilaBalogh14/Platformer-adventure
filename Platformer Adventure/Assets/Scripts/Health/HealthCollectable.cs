using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectable : MonoBehaviour
{
    [SerializeField] private float healthValue = 1f;
    [SerializeField] private AudioClip pickupSound;

    //esemény, amit a RoomController meghallhat
    public System.Action OnPickedUp;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SoundManager.instance.PlaySound(pickupSound);
            collision.GetComponent<Health>()?.AddHealth(healthValue);

            //jelezzük a RoomControllernek, hogy ez inaktiválódott
            OnPickedUp?.Invoke();

            gameObject.SetActive(false);
        }
    }
}
