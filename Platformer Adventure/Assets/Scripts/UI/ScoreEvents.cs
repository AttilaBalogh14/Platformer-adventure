using System;

public static class ScoreEvents
{
    public static event Action<int> OnScoreChanged;

    public static void AddScore(int amount)
    {
        OnScoreChanged?.Invoke(amount);
    }
}
