using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpointSound;
    private Transform currentCheckpoint; //Utolsó elért checkpoint
    private Health playerHealth;
    private UIManager uiManager;
    private static Vector3 checkpointPosition = Vector3.zero; //Statikus, hogy megmaradjon scene reload után

    [Header("Score")]
    [SerializeField] private int checkpointScoreValue;

    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject checkpointButton;

    private BossMovement bossMovement;

    void Awake()
    {
        playerHealth = GetComponent<Health>();
        uiManager = FindObjectOfType<UIManager>();
        bossMovement = FindObjectOfType<BossMovement>();
    }

    public void CheckRespawn()
    {
        //Game Over képernyő megjelenítése
        uiManager.GameOver();        

    }

    public void ResetCurrentCheckpoint()
    {
        currentCheckpoint = null;
    }

    public void LoadFromCheckpoint()
    {
        Debug.Log("LoadFromCheckpoint meghívva!");

        if (currentCheckpoint != null || checkpointPosition != Vector3.zero)
        {
            //Pozíció visszaállítása
            Vector3 respawnPos = currentCheckpoint != null ? currentCheckpoint.position : checkpointPosition;
            transform.position = respawnPos;

            //Rigidbody2D frissítése
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.position = respawnPos;
            }

            //Élet visszaállítása
            playerHealth.Respawn();

            //Szoba és kamera beállítása
            Room checkpointRoom = null;

            if (currentCheckpoint != null)
                checkpointRoom = currentCheckpoint.GetComponentInParent<Room>();

            if (checkpointRoom != null)
            {
                //Szoba aktiválása
                checkpointRoom.ActivateRoom(true);

                //Kamera áthelyezése a szoba kamera fókuszpontjára
                CameraController cam = Camera.main.GetComponent<CameraController>();
                if (cam != null)
                {
                    Transform focus = checkpointRoom.CameraPoint != null
                        ? checkpointRoom.CameraPoint
                        : checkpointRoom.transform;

                    cam.MoveToNewRoom(focus);
                    Debug.Log($"Kamera áthelyezve ide: {focus.name}");
                }

                Debug.Log("Checkpoint szoba újraaktiválva!");
            }
            else
            {
                //fallback ha nem található Room komponens
                Debug.LogWarning("Checkpointhoz nem tartozik Room!");
                CameraController cam = Camera.main.GetComponent<CameraController>();
                if (cam != null)
                    cam.MoveToNewRoom(currentCheckpoint != null ? currentCheckpoint : transform);
            }

            //UI visszaállítása
            uiManager.gameOverScreen.SetActive(false);
            Time.timeScale = 1;

            //Pontszám visszaállítása
            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.Instance.SetScore(GameScoreManager.checkpointScore);
                Debug.Log("Pontszám visszaállítva: " + GameScoreManager.checkpointScore);
            }

            Debug.Log("Checkpoint betöltve, játékos pozíció: " + respawnPos);
        }
        else
        {
            //Ha nincs mentett checkpoint
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
            Debug.LogWarning("Nincs mentett checkpoint! Újrakezdés...");
        }
    }

    //Checkpoint aktiválása
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint"))
        {
            currentCheckpoint = collision.transform;
            checkpointPosition = currentCheckpoint.position;

            SoundManager.instance.PlaySound(checkpointSound);
            collision.GetComponent<Collider2D>().enabled = false;
            collision.GetComponent<Animator>().SetTrigger("appear");

            ScoreEvents.AddScore(checkpointScoreValue);

            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.checkpointScore = GameScoreManager.Instance.currentScore;
                Debug.Log("Checkpoint pontszám mentve: " + GameScoreManager.checkpointScore);
            }

            Debug.Log("Checkpoint elérve: " + checkpointPosition);
        }
    }

    public void ResetCurrentheckpoint()
    {
        currentCheckpoint = null;
    }
}
