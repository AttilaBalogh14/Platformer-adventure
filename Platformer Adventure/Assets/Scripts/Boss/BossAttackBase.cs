using System;
using UnityEngine;

public abstract class BossAttackBase : MonoBehaviour
{
    [Header("Attack Settings")]
    public float cooldown = 1f;
    public float damage = 1f;

    // 🔹 Esemény: a támadás befejeződött (talált vagy sem)
    public event Action<BossAttackBase, bool> OnAttackResolved;

    /// <summary>
    /// A BossAttackManager ezen keresztül kaphat információt a támadás sikerességéről.
    /// </summary>
    public void ResolveAttack(bool hit)
    {
        OnAttackResolved?.Invoke(this, hit);
    }

    /// <summary>
    /// Az AI döntési logikája ezt hívja, hogy pontozza, mennyire érdemes ezt a támadást használni.
    /// </summary>
    public virtual float GetHeuristicScore(Transform player, Transform boss)
    {
        return 0f; // alapértelmezett
    }

    /// <summary>
    /// A támadás konkrét végrehajtása.
    /// </summary>
    public abstract void Execute(Transform player);
}
