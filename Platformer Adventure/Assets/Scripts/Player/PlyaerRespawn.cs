using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlyaerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpointSound; //Sounf that we'll play when picking up a new checkpoint
    private Transform currentCheckpoint; //we'll store our last checkpoint here
    private Health playerHealth;
    private UIManager uiManager;

    void Awake()
    {
        playerHealth = GetComponent<Health>();
        uiManager = FindObjectOfType<UIManager>();
    }

    public void CheckRespawn()
    {
        //Ceck if checkpoint available
        if (currentCheckpoint == null)
        {
            //Show game over screen
            uiManager.GameOver();

            return; //don't execute the rest of this function
        }

        transform.position = currentCheckpoint.position; //Move player to checkpoint psoition
        playerHealth.Respawn(); //restore player health and reset animation

        //Move camera back to checkpoint room (for this to work the checkpoint objects has to placed as a child of the room object)
        Camera.main.GetComponent<CameraContoller>().MoveToNewRoom(currentCheckpoint.parent);
    }

    //Activate checkpoints
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Checkpoint")
        {
            currentCheckpoint = collision.transform; //Store the checkpoint that we activated as the current one 
            SoundManager.instance.PlaySound(checkpointSound);
            collision.GetComponent<Collider2D>().enabled = false; //Deactivate checkpoint collider 
            collision.GetComponent<Animator>().SetTrigger("appear"); //trigger checkpoint animation

        }
    }

}
