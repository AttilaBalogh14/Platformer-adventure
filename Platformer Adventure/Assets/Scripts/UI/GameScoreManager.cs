using UnityEngine;
using TMPro;

public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance;
    public static int checkpointScore;

    public int currentScore = 0;
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetScore();
    }

    private void OnEnable()
    {
        ScoreEvents.OnScoreChanged += AddScore;
    }

    private void OnDisable()
    {
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
