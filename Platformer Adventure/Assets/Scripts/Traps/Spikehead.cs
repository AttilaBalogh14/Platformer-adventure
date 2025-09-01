using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikehead : EnemyDamage
{
    private Vector3 destination;
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private float checkDelay;
    [SerializeField] private LayerMask playerLayer;
    private float checkTimer;

    private bool attacking;

    private Vector3[] directions = new Vector3[4];

    [Header("SFX")]
    [SerializeField] private AudioClip impactSound;


    void OnEnable()
    {
        Stop();
    }

    private void Update()
    {
        //Move spikehead to destination only if attacking 
        if(attacking)    
            transform.Translate(destination * Time.deltaTime * speed);
        else
        {
            checkTimer += Time.deltaTime;
            if(checkTimer > checkDelay)
                CheckForPlayer();
        }
    }

    private void CheckForPlayer()
    {
        CalculateDirections();

        //check if spikehead sees player in all 4 directions
        for (int i = 0; i < directions.Length; i++)
        {
            Debug.DrawRay(transform.position, directions[i], Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], range, playerLayer);

            if(hit.collider != null && !attacking)
            {
                attacking = true;
                destination = directions[i];
                checkTimer = 0;
            }
        }
    } 
    private void CalculateDirections()
    {
        directions[0] = transform.right * range; //right direction
        directions[1] = -transform.right * range; //left direction
        directions[2] = transform.up * range; //up direction
        directions[3] = -transform.up * range; //right direction
    }

    private void Stop()
    {
        destination = transform.position; //set destination as current position so it doesnt move 
        attacking = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        SoundManager.instance.PlaySound(impactSound);
        base.OnTriggerEnter2D(collision);
        Stop(); //stop spikehead once he hits something
    }
}
