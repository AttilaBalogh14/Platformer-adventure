using UnityEngine;

public static class FuzzyLogic
{
    //Membership függvények (0–1 tartományban)
    public static float Low(float x, float midpoint = 0.3f, float width = 0.3f)
        => Mathf.Clamp01(1f - Mathf.InverseLerp(midpoint - width, midpoint + width, x));

    public static float High(float x, float midpoint = 0.7f, float width = 0.3f)
        => Mathf.Clamp01(Mathf.InverseLerp(midpoint - width, midpoint + width, x));

    public static float Medium(float x, float lowMid = 0.3f, float highMid = 0.7f)
    {
        if (x < lowMid) return Mathf.InverseLerp(0f, lowMid, x);
        if (x > highMid) return 1f - Mathf.InverseLerp(highMid, 1f, x);
        return 1f;
    }

    //Alap fuzzy operátorok
    public static float And(float a, float b) => Mathf.Min(a, b);
    public static float Or(float a, float b) => Mathf.Max(a, b);
    public static float Not(float a) => 1f - a;

    //Soft fuzzy operátorok (finomabb döntéshez)
    public static float SoftAnd(float a, float b) => a * b + 0.2f * ((a + b) / 2f);
    public static float SoftOr(float a, float b) => Mathf.Max(a, b) * 0.8f + 0.2f * ((a + b) / 2f);

    //Defuzzifikáció (weighted average)
    public static float Defuzzify(params (float value, float weight)[] inputs)
    {
        float sumW = 0f, sumV = 0f;
        foreach (var i in inputs)
        {
            sumW += i.weight;
            sumV += i.value * i.weight;
        }
        return sumW > 0f ? sumV / sumW : 0.5f;
    }
}
