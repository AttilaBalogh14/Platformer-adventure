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
        public float effectiveness => used > 0 ? (float)hits / used : 0.5f;
    }

    [Serializable]
    public class MoveStats
    {
        public string moveType;
        public int used = 0;
        public int survived = 0;
        public float effectiveness => used > 0 ? (float)survived / used : 0.5f;
    }

    [SerializeField] private List<AttackStats> attackStats = new List<AttackStats>();
    [SerializeField] private List<MoveStats> moveStats = new List<MoveStats>();
    [SerializeField] private Transform player;
    [SerializeField] private Health bossHealth;

    private Vector2 lastPlayerPos;
    private float jumpTendency = 0f;
    private float aggressionLevel = 0f;

    private float lastHealth;
    private float healthSinceLastMove;
    private string lastMoveType = null;

    void Start()
    {
        if (player == null) Debug.LogError("BossLearningMemory: Player nincs beállítva!");
        if (bossHealth == null) bossHealth = GetComponent<Health>();

        lastPlayerPos = player.position;
        lastHealth = bossHealth?.currentHealth ?? 100;
    }

    void Update()
    {
        ObservePlayer();
        TrackDamageDuringMove();
    }

    private void TrackDamageDuringMove()
    {
        if (bossHealth == null) return;

        float currentHealth = bossHealth.currentHealth;
        if (currentHealth < lastHealth)
        {
            // Mindig növeljük a damage-t
            healthSinceLastMove += (lastHealth - currentHealth);
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
        if (hit) stat.hits++;
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
            if (healthSinceLastMove < 3f)
                stat.survived++;
        }

        lastMoveType = null;
        healthSinceLastMove = 0f;
    }

    public float GetEffectiveness(BossAttackBase attack)
    {
        var stat = attackStats.Find(a => a.attack == attack);
        return stat != null ? stat.effectiveness : 0.5f;
    }

    public float GetMoveEffectiveness(string moveType)
    {
        var stat = moveStats.Find(m => m.moveType == moveType);
        return stat != null ? stat.effectiveness : 0.5f;
    }

    public void ObservePlayer()
    {
        if (player == null) return;

        Vector2 delta = (Vector2)player.position - lastPlayerPos;

        if (delta.y > 0.1f) jumpTendency = Mathf.Clamp01(jumpTendency + 0.05f);
        else jumpTendency = Mathf.Clamp01(jumpTendency - 0.02f);

        float dist = Mathf.Abs(player.position.x - transform.position.x);
        aggressionLevel = Mathf.Lerp(aggressionLevel, dist < 2f ? 1f : 0f, 0.1f);

        lastPlayerPos = player.position;
    }

    public float JumpTendency => jumpTendency;
    public float AggressionLevel => aggressionLevel;

    // 🔹 AI teleport logikához szükséges getter
    public float HealthSinceLastMove => healthSinceLastMove;
}
