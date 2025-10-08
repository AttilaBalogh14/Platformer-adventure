using UnityEngine;

public abstract class BossAttackBase : MonoBehaviour
{
    public float cooldown;
    public float damage;

    // Az AI integrációs pont: minden támadás maga értékeli magát
    public virtual float GetHeuristicScore(Transform player, Transform boss)
    {
        return 0f; // alapértelmezett: semmi extra
    }

    // Alap végrehajtás, örökölhető
    public abstract void Execute(Transform player);
}
