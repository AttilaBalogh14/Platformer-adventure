using UnityEngine;

public enum BossTactic
{
    Aggressive,
    Defensive,
    Ranged,
    Evade
}

public class BossAttackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossAttackBase[] attacks;
    [SerializeField] private Transform player;
    [SerializeField] private BossForceAttack forcedAttack;
    [SerializeField] private BossLearningMemory memory;

    [Header("Attack Settings")]
    [SerializeField] private float decisionInterval = 0.5f;

    private float cooldownTimer = 0f;
    private float decisionTimer = 0f;
    private bool isAwake = false;

    private BossTactic currentTactic = BossTactic.Aggressive;

    private void Start()
    {
        if (memory == null)
            memory = GetComponent<BossLearningMemory>();

        foreach (var attack in attacks)
            if (attack != null)
                attack.OnAttackResolved += OnAttackResolved;
    }

    private void Update()
    {
        if (!isAwake || player == null) return;

        decisionTimer -= Time.deltaTime;
        cooldownTimer -= Time.deltaTime;

        if (decisionTimer <= 0f)
        {
            var bestAttack = ChooseBestAttack();
            if (bestAttack != null && cooldownTimer <= 0f)
            {
                bestAttack.Execute(player);
                cooldownTimer = bestAttack.cooldown;
            }
            decisionTimer = decisionInterval;
        }
    }

    private BossAttackBase ChooseBestAttack()
    {
        currentTactic = DecideTactic();

        BossAttackBase bestAttack = null;
        float bestScore = float.MinValue;

        foreach (var attack in attacks)
        {
            float score = CalculateFuzzyScore(attack, currentTactic);
            Debug.Log($"[AttackDebug] Attack '{attack.name}', Score={score:F2}, Tactic={currentTactic}");
            if (score > bestScore)
            {
                bestScore = score;
                bestAttack = attack;
            }
        }

        Debug.Log($"[AttackDebug] Chosen Attack: '{bestAttack?.name}' with score {bestScore:F2}");
        return bestAttack;
    }

    private BossTactic DecideTactic()
    {
        float hp = memory.BossHealthPercent();
        float dist = memory.PlayerDistanceFraction(transform, player);
        float aggr = memory.AggressionLevel;
        float jump = memory.JumpTendency;

        float hpLow = FuzzyLogic.Low(hp);
        float hpHigh = FuzzyLogic.High(hp);
        float distNear = FuzzyLogic.Low(dist);
        float distFar = FuzzyLogic.High(dist);
        float aggrHigh = FuzzyLogic.High(aggr);
        float aggrLow = FuzzyLogic.Low(aggr);
        float jumpHigh = FuzzyLogic.High(jump);

        float evadeRule = FuzzyLogic.And(hpLow, distNear);
        float defensiveRule = FuzzyLogic.And(distNear, aggrLow);
        float rangedRule = FuzzyLogic.Or(distFar, jumpHigh);
        float aggressiveRule = FuzzyLogic.And(aggrHigh, FuzzyLogic.Not(hpLow));

        float max = Mathf.Max(aggressiveRule, defensiveRule, rangedRule, evadeRule);
        BossTactic chosen = BossTactic.Evade;
        if (max == aggressiveRule) chosen = BossTactic.Aggressive;
        else if (max == defensiveRule) chosen = BossTactic.Defensive;
        else if (max == rangedRule) chosen = BossTactic.Ranged;

        return chosen;
    }

    private float CalculateFuzzyScore(BossAttackBase attack, BossTactic tactic)
    {
        float dist = memory.PlayerDistanceFraction(transform, player);
        float aggr = memory.AggressionLevel;
        float jump = memory.JumpTendency;
        float hp = memory.BossHealthPercent();
        float playerHeight = memory.PlayerHeightFraction(transform, player);

        float distNear = FuzzyLogic.Low(dist);
        float distFar = FuzzyLogic.High(dist);
        float aggrHigh = FuzzyLogic.High(aggr);
        float jumpHigh = FuzzyLogic.High(jump);
        float hpLow = FuzzyLogic.Low(hp);

        float baseScore = 0f;

        if (attack is BossFireballAttack)
            baseScore = FuzzyLogic.SoftOr(distFar, jumpHigh) * 10f;
        else if (attack is BossDashAttack)
            baseScore = FuzzyLogic.SoftAnd(distNear, aggrHigh) * 12f;
        else if (attack is BossAttackUp)
            baseScore = FuzzyLogic.SoftAnd(jumpHigh, aggrHigh) * 8f + FuzzyLogic.High(playerHeight) * 5f;
        else if (attack is BossAttackDown)
            baseScore = FuzzyLogic.SoftAnd(distNear, hpLow) * 7f + FuzzyLogic.Low(playerHeight) * 5f;

        //taktika súlyozás
        switch (tactic)
        {
            case BossTactic.Aggressive: baseScore *= 1.2f; break;
            case BossTactic.Defensive: baseScore *= 0.9f; break;
            case BossTactic.Ranged: baseScore *= 1.1f; break;
            case BossTactic.Evade: baseScore *= 0.6f; break;
        }

        float learned = memory.GetEffectiveness(attack);
        baseScore = Mathf.Lerp(baseScore, baseScore * 1.5f, learned);

        //kis random faktor, hogy ritkábban használt támadások is előjöjjenek
        baseScore += UnityEngine.Random.Range(-1f, 1.5f);
        return Mathf.Clamp(baseScore, 0f, 15f);
    }

    private void OnAttackResolved(BossAttackBase attack, bool hit)
    {
        if (memory != null)
            memory.RegisterAttack(attack, hit);
    }

    public void WakeUp()
    {
        isAwake = true;
        cooldownTimer = 0f;
        decisionTimer = 0f;
    }

    public void ForceAttack()
    {
        if (forcedAttack != null)
            forcedAttack.Execute();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (player == null) return;
        Vector3 gizmoPos = transform.position + Vector3.up * 2f;
        Color color = Color.white;

        switch (currentTactic)
        {
            case BossTactic.Aggressive: color = Color.red; break;
            case BossTactic.Defensive: color = Color.blue; break;
            case BossTactic.Ranged: color = Color.yellow; break;
            case BossTactic.Evade: color = Color.green; break;
        }

        Gizmos.color = color;
        Gizmos.DrawSphere(gizmoPos, 0.3f);
        UnityEditor.Handles.Label(gizmoPos + Vector3.up * 0.5f, $"Tactic: {currentTactic}");
    }
#endif
}
