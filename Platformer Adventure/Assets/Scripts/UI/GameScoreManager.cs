using UnityEngine;
using TMPro;

public class GameScoreManager : MonoBehaviour
{
    // A Singleton minta statikus példánya
    public static GameScoreManager Instance;
    // A checkpoint pontszáma
    public static int checkpointScore;

    public int currentScore = 0;
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        // Ha az Instance még nem létezik, ez lesz az egyetlen példány
        if (Instance == null)
        {
            Instance = this;
            // Ezzel az objektummal a jelenetek között is megmarad
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Ha már létezik egy példány, megsemmisítjük ezt a másodikat
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // A játék indításakor nullázzuk a pontszámot.
        // Ezt a metódust a Unity minden jelenet betöltésekor meghívja a DontDestroyOnLoad miatt.
        ResetScore();
    }

    private void OnEnable()
    {
        // Feliratkozás az eseményre
        ScoreEvents.OnScoreChanged += AddScore;
    }

    private void OnDisable()
    {
        // Fontos: leiratkozás az eseményről, amikor az objektum elpusztul
        ScoreEvents.OnScoreChanged -= AddScore;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    public void SetScore(int newScore)
    {
        currentScore = newScore;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }
}
