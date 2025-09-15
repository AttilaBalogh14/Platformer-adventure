using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Sideways : MonoBehaviour
{
    [SerializeField] private float contactDamage;
    [SerializeField] private float movementDistance;
    [SerializeField] private float moveSpeed;
    private bool isMovingLeft;
    private float leftEdge;
    private float rightEdge;

    void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
    }

    private void Update()
    {
        Moving();
    }

     private void Moving()
    {
        if (isMovingLeft)
        {
            if (transform.position.x > leftEdge)
            {
                MoveHorizontally(-1);
            }
            else
            {
                isMovingLeft = false;
            }
        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                MoveHorizontally(1);
            }
            else
            {
                isMovingLeft = true;
            }
        }
    }

    private void MoveHorizontally(int direction)
    {
        transform.position = new Vector3(
            transform.position.x + direction * moveSpeed * Time.deltaTime,
            transform.position.y,
            transform.position.z
        );
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.TakeDamage(contactDamage);
        }
    }
}
