using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikehead : EnemyDamage
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float detectionRange;
    [SerializeField] private float checkInterval;
    [SerializeField] private LayerMask playerLayer;
    private float checkTimer;
    private Vector3 moveDirection;
    private bool isAttacking;

    private Vector3[] possibleDirections = new Vector3[4];

    [Header("SFX")]
    [SerializeField] private AudioClip impactSfx;

    private Health selfHealth;

    void Awake()
    {
        selfHealth = GetComponent<Health>();
    }

    void OnEnable()
    {
        ResetSpikehead();

        if (selfHealth != null)
            selfHealth.ResetHealth();
    }

    private void Update()
    {
        if (isAttacking)
        {
            Move();
        }
        else
        {
            checkTimer += Time.deltaTime;
            if(checkTimer > checkInterval)
                ScanForPlayer();
        }
    }
    
    private void Move()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void ScanForPlayer()
    {
        UpdateDirections();

        for (int i = 0; i < possibleDirections.Length; i++)
        {
            Debug.DrawRay(transform.position, possibleDirections[i], Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, possibleDirections[i], detectionRange, playerLayer);

            if (hit.collider != null && !isAttacking)
            {
                isAttacking = true;
                moveDirection = possibleDirections[i];
                checkTimer = 0f;
            }
        }
    } 
    private void UpdateDirections()
    {
        possibleDirections[0] = transform.right * detectionRange;
        possibleDirections[1] = -transform.right * detectionRange;
        possibleDirections[2] = transform.up * detectionRange;
        possibleDirections[3] = -transform.up * detectionRange;
    }

    private void ResetSpikehead()
    {
        moveDirection = transform.position;  
        isAttacking = false;
        checkTimer = 0f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        SoundManager.instance.PlaySound(impactSfx);
        base.OnTriggerEnter2D(collision);
        ResetSpikehead();
    }
}
