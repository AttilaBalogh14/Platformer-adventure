using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] public GameObject gameOverScreen;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;
    private Health health;

    private PlayerRespawn playerRespawn;

    private bool canPause = true; // 🔹 új: tiltás engedélyezése

    private void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
        health = FindObjectOfType<Health>();

        playerRespawn = FindObjectOfType<PlayerRespawn>();
    }

    void Update()
    {
        if (!canPause) return; // 🔹 ha tiltva van, ne engedje a pause-t

        if (Input.GetKeyDown(KeyCode.Escape) && !health.IsGameOver())
        {
            if (pauseScreen.activeInHierarchy)
                PauseGame(false);
            else
                PauseGame(true);
        }
    }

    #region Game Over

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        SoundManager.instance.PlaySound(gameOverSound);
    }

    public void Restart()
    {
        EnablePause(); // újra engedélyezzük a pause-t

        // --- 🧩 Checkpoint törlése ---
        if (playerRespawn != null)
        {
            playerRespawn.ResetCurrentCheckpoint();
            // Statikus checkpoint pozíciót is töröljük
            typeof(PlayerRespawn)
                .GetField("checkpointPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, Vector3.zero);
        }

        // --- 🧩 Pontszám nullázása (opcionális) ---
        if (GameScoreManager.Instance != null)
            GameScoreManager.checkpointScore = 0;

        // --- 🧩 Scene újratöltése ---
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;

        Debug.Log("Restart gomb megnyomva: checkpoint és pontszám törölve!");
    }

    public void MainMenu()
    {
        EnablePause();
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Pause
    public void PauseGame(bool status)
    {
        pauseScreen.SetActive(status);

        if (status)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void SoundVolume()
    {
        SoundManager.instance.ChangeSoundVolume(0.2f);
    }

    public void MusicVolume()
    {
        SoundManager.instance.ChangeMusicVolume(0.2f);
    }
    #endregion

    // 🔹 új: halál utáni pause-tiltás
    public void DisablePause()
    {
        canPause = false;
        pauseScreen.SetActive(false);
    }

    public void EnablePause()
    {
        canPause = true;
    }

    public IEnumerator ShowGameOverScreenWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
