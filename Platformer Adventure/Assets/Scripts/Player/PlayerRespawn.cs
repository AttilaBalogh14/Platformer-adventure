using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpointSound; //Sound that we'll play when picking up a new checkpoint
    private Transform currentCheckpoint; //we'll store our last checkpoint here
    private Health playerHealth;
    private UIManager uiManager;
    private static Vector3 checkpointPosition; // statikus, hogy ne törlődjön Scene reload után

    [Header("Score")]
    [SerializeField] private int checkpointScoreValue;

    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject checkpointButton;

    void Awake()
    {
        playerHealth = GetComponent<Health>();
        uiManager = FindObjectOfType<UIManager>();
    }

    public void CheckRespawn()
    {
        //Show game over screen
        uiManager.GameOver();

        /*if (currentCheckpoint == null)
        {
            restartButton.SetActive(true);
            checkpointButton.SetActive(false);

        }
        else
        {
            restartButton.SetActive(false);
            checkpointButton.SetActive(true);
        }*/
    }

    public void LoadFromCheckpoint()
    {
        if (currentCheckpoint != null)
        {
            // Player állapot visszaállítása
            transform.position = currentCheckpoint.position;
            playerHealth.Respawn();
            Camera.main.GetComponent<CameraController>().MoveToNewRoom(currentCheckpoint.parent);
            uiManager.gameOverScreen.SetActive(false);
            Time.timeScale = 1;

            // Reseteljük a checkpoint szobájának enemy-it
            Room checkpointRoom = currentCheckpoint.GetComponentInParent<Room>();
            if (checkpointRoom != null)
            {
                checkpointRoom.ActivateRoom(true); // újrapozícionálja és aktiválja az enemy-ket
            }

            // Visszaállítjuk a pontszámot a checkpointnál tárolt értékre
            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.Instance.SetScore(GameScoreManager.checkpointScore);
            }

            Debug.Log("Checkpoint betöltve, enemy-k resetelve!");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
            Debug.Log("Nincs mentett checkpoint!");
        }
    }

    public void TestButton()
    {
        Debug.Log("A test gomb működik!");
    }

    //Activate checkpoints
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Checkpoint")
        {
            currentCheckpoint = collision.transform; //Store the checkpoint that we activated as the current one 
            checkpointPosition = currentCheckpoint.position; // mentés statikus változóba
            SoundManager.instance.PlaySound(checkpointSound);
            collision.GetComponent<Collider2D>().enabled = false; //Deactivate checkpoint collider 
            collision.GetComponent<Animator>().SetTrigger("appear"); //trigger checkpoint animation
            ScoreEvents.AddScore(checkpointScoreValue);

            // Elmentjük a pontszámot, amikor a játékos eléri a checkpointot
            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.checkpointScore = GameScoreManager.Instance.currentScore;
                Debug.Log("Checkpoint pontszám mentve: " + GameScoreManager.checkpointScore);
            }
        }
    }
}
