using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [SerializeField] private BossAttackBase[] attacks;
    [SerializeField] private Transform player;
    [SerializeField] private BossForceAttack forcedAttack;
    [SerializeField] private BossLearningMemory memory;

    private float cooldownTimer = 0f;
    private bool isAwake = false;

    private void Update()
    {
        if (!isAwake || player == null) return;

        cooldownTimer -= Time.deltaTime;
        memory.ObservePlayer();

        if (cooldownTimer <= 0f)
        {
            var bestAttack = ChooseBestAttack();
            if (bestAttack != null)
            {
                bestAttack.Execute(player);
                cooldownTimer = bestAttack.cooldown;
            }
        }
    }

    private BossAttackBase ChooseBestAttack()
    {
        float bestScore = float.MinValue;
        BossAttackBase bestAttack = null;

        foreach (var attack in attacks)
        {
            float heuristicScore = attack.GetHeuristicScore(player, transform);
            float learned = memory.GetEffectiveness(attack) * 3f; // csökkentett súly
            float predicted = PredictPlayerReaction(attack);
            float randomFactor = Random.Range(0f, 3f); // kis véletlenség

            float totalScore = heuristicScore + learned + predicted + randomFactor;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestAttack = attack;
            }
        }

        return bestAttack;
    }

    private float PredictPlayerReaction(BossAttackBase attack)
    {
        float score = 0f;
        float jumpTendency = memory.JumpTendency;
        float aggression = memory.AggressionLevel;
        float horizontalDist = Mathf.Abs(player.position.x - transform.position.x);
        float verticalDiff = player.position.y - transform.position.y;

        // Felfelé támadás – ha player felette van, vagy sokat ugrik
        if (attack is BossAttackUp && (verticalDiff > 0.2f || jumpTendency > 0.4f))
            score += 8f;

        // Lefelé támadás – ha player alatta van vagy gyakran közelharcol
        if (attack is BossAttackDown && (verticalDiff < -0.2f || aggression > 0.6f))
            score += 7f;

        // Dash – ha player agresszív, vagy közel van
        if (attack is BossDashAttack && (horizontalDist < 5f || aggression > 0.5f))
            score += 8f;

        // Fireball – ha player távolságot tart és keveset ugrál
        if (attack is BossFireballAttack)
        {
            if (jumpTendency < 0.4f && horizontalDist > 4f)
                score += 6f;
            else
                score -= 3f; // ne spamelje, ha közel vagy
        }

        return score;
    }


    public void WakeUp()
    {
        isAwake = true;
        cooldownTimer = 0f;
    }

    public void RegisterAttackResult(BossAttackBase attack, bool hit)
    {
        memory.RegisterAttack(attack, hit);
    }

    public void ForceAttack()
    {
        if (forcedAttack != null) forcedAttack.Execute();
    }
}
