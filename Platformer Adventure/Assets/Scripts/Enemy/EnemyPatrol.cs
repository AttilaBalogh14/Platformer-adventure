using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol points")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header("Enemy")]
    [SerializeField] private Transform enemy;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private Vector3 initialScale;
    private bool isMovingLeft;

    [Header("Idle Behaviour")]
    [SerializeField] private float idleTime;
    private float idleCounter;

    [Header("Enemy Animator")]
    [SerializeField] private Animator anim;

    private void Awake()
    {
        initialScale = enemy.localScale;
    }

    private void Update()
    {
        if (isMovingLeft)
        {
            if (enemy.position.x >= leftEdge.position.x)
                MoveInDirection(-1);
            else
                ChangeDirection();
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
                MoveInDirection(1);
            else
                ChangeDirection();
        }
    }

    private void ChangeDirection()
    {
        anim.SetBool("moving", false);

        idleCounter += Time.deltaTime;

        if (idleCounter >= idleTime)
            isMovingLeft = !isMovingLeft;
    }

    private void MoveInDirection(int direction)
    {
        idleCounter = 0f;
        anim.SetBool("moving", true);

        enemy.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
        
        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * direction * moveSpeed, enemy.position.y, enemy.position.z);
    }

    private void OnDisable()
    {
        anim.SetBool("moving", false);
    }
}
