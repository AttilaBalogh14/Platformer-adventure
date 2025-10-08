using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] public GameObject ControlsScreen;

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
        Time.timeScale = 1;
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void SoundVolume()
    {
        SoundManager.instance.ChangeSoundVolume(0.2f);
    }

    public void MusicVolume()
    {
        SoundManager.instance.ChangeMusicVolume(0.2f);
    }

    public void Control()
    {
        ControlsScreen.SetActive(true);
    }

    void Update()
    {
        if (ControlsScreen.activeSelf == true && Input.GetKeyDown(KeyCode.Escape))
            ControlsScreen.SetActive(false);
    }
}
