using System;
using System.Collections.Generic;
using UnityEngine;

public class BossLearningMemory : MonoBehaviour
{
    [Serializable]
    public class AttackStats
    {
        public BossAttackBase attack;
        public int used = 0;
        public int hits = 0;
        public float lastSuccess = 0.5f;
        public float effectiveness => used > 0 ? (float)hits / used : 0.5f;
    }

    [Serializable]
    public class MoveStats
    {
        public string moveType;
        public int used = 0;
        public int survived = 0;
        public float lastSuccess = 0.5f;
        public float effectiveness => used > 0 ? (float)survived / used : 0.5f;
    }

    [SerializeField] private List<AttackStats> attackStats = new List<AttackStats>();
    [SerializeField] private List<MoveStats> moveStats = new List<MoveStats>();
    [SerializeField] private Transform player;
    [SerializeField] private Health bossHealth;

    private Vector2 lastPlayerPos;
    private float jumpTendency = 0.5f;
    private float aggressionLevel = 0.5f;

    private float lastHealth;
    private float healthSinceLastMove;
    private string lastMoveType = null;
    private float lastHitTime = 0f;

    private const float learningRate = 0.15f;
    private const float decayRate = 0.98f;
    private float adaptiveAggressionMemory = 0.5f;

    void Start()
    {
        if (player == null) Debug.LogError("BossLearningMemory: Player nincs beállítva!");
        if (bossHealth == null) bossHealth = GetComponent<Health>();

        lastPlayerPos = player != null ? player.position : Vector2.zero;
        lastHealth = bossHealth?.currentHealth ?? 100;
        lastHitTime = Time.time;
    }

    void Update()
    {
        ObservePlayer();
        TrackDamageDuringMove();
        DecayMemory();
    }

    private void TrackDamageDuringMove()
    {
        if (bossHealth == null) return;

        float currentHealth = bossHealth.currentHealth;
        if (currentHealth < lastHealth)
        {
            healthSinceLastMove += (lastHealth - currentHealth);
            lastHitTime = Time.time;
        }
        lastHealth = currentHealth;
    }

    public void RegisterAttack(BossAttackBase attack, bool hit)
    {
        var stat = attackStats.Find(a => a.attack == attack);
        if (stat == null)
        {
            stat = new AttackStats() { attack = attack };
            attackStats.Add(stat);
        }

        stat.used++;
        if (hit)
        {
            stat.hits++;
            stat.lastSuccess = Mathf.Lerp(stat.lastSuccess, 1f, learningRate);
            lastHitTime = Time.time;
        }
        else
        {
            stat.lastSuccess = Mathf.Lerp(stat.lastSuccess, 0f, learningRate);
        }

        float adjust = hit ? 0.05f : -0.05f;
        aggressionLevel = Mathf.Clamp01(aggressionLevel + adjust * learningRate);
    }

    public void RegisterMove(string moveType)
    {
        var stat = moveStats.Find(m => m.moveType == moveType);
        if (stat == null)
        {
            stat = new MoveStats() { moveType = moveType };
            moveStats.Add(stat);
        }

        stat.used++;
        lastMoveType = moveType;
        healthSinceLastMove = 0f;
        lastHealth = bossHealth.currentHealth;
    }

    public void EndMoveEvaluation()
    {
        if (lastMoveType == null) return;

        var stat = moveStats.Find(m => m.moveType == lastMoveType);
        if (stat != null)
        {
            float survived = healthSinceLastMove < 3f ? 1f : 0f;
            stat.survived += (int)survived;
            stat.lastSuccess = Mathf.Lerp(stat.lastSuccess, survived, learningRate);
        }

        lastMoveType = null;
        healthSinceLastMove = 0f;
    }

    public float GetEffectiveness(BossAttackBase attack)
    {
        var stat = attackStats.Find(a => a.attack == attack);
        if (stat == null) return 0.5f;
        return Mathf.Clamp01(Mathf.Lerp(stat.effectiveness, stat.lastSuccess, 0.6f));
    }

    public float GetMoveEffectiveness(string moveType)
    {
        var stat = moveStats.Find(m => m.moveType == moveType);
        if (stat == null) return 0.5f;
        return Mathf.Clamp01(Mathf.Lerp(stat.effectiveness, stat.lastSuccess, 0.6f));
    }

    public void ObservePlayer()
    {
        if (player == null) return;

        Vector2 delta = (Vector2)player.position - lastPlayerPos;

        //Jump tendency figyelés
        if (delta.y > 0.1f)
            jumpTendency = Mathf.Clamp01(jumpTendency + 0.05f);
        else
            jumpTendency = Mathf.Clamp01(jumpTendency - 0.02f);

        //Aggresszió a player távolsága alapján
        float dist = Vector2.Distance(player.position, transform.position);
        float proximity = Mathf.Clamp01(1f - dist / 5f);
        adaptiveAggressionMemory = Mathf.Lerp(adaptiveAggressionMemory, proximity, 0.1f);

        aggressionLevel = Mathf.Lerp(aggressionLevel, adaptiveAggressionMemory, 0.05f);

        lastPlayerPos = player.position;
    }

    private void DecayMemory()
    {
        for (int i = 0; i < attackStats.Count; i++)
            attackStats[i].lastSuccess = Mathf.Lerp(attackStats[i].lastSuccess, 0.5f, 1f - decayRate);

        for (int i = 0; i < moveStats.Count; i++)
            moveStats[i].lastSuccess = Mathf.Lerp(moveStats[i].lastSuccess, 0.5f, 1f - decayRate);

        aggressionLevel = Mathf.Lerp(aggressionLevel, 0.5f, 1f - decayRate);
        jumpTendency = Mathf.Lerp(jumpTendency, 0.5f, 1f - decayRate);
    }

    public float JumpTendency => jumpTendency;
    public float AggressionLevel => aggressionLevel;
    public float HealthSinceLastMove => healthSinceLastMove;
    public float TimeSinceLastHit => Time.time - lastHitTime;

    public float BossHealthPercent() =>
        bossHealth != null ? Mathf.Clamp01(bossHealth.currentHealth / bossHealth.startingHealth) : 1f;

    //Player magasság és távolság információ
    public float PlayerDistanceFraction(Transform boss, Transform player, float maxDistance = 10f)
    {
        if (player == null || boss == null) return 0f;
        return Mathf.Clamp01(Vector2.Distance(player.position, boss.position) / maxDistance);
    }

    public float PlayerHeightFraction(Transform boss, Transform player, float maxHeight = 5f)
    {
        if (player == null || boss == null) return 0f;
        return Mathf.Clamp01((player.position.y - boss.position.y + maxHeight / 2f) / maxHeight);
    }
}
