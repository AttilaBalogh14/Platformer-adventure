using System;
using UnityEngine;

public abstract class BossAttackBase : MonoBehaviour
{
    [Header("Attack Settings")]
    public float cooldown = 1f;
    public float damage = 1f;

    //A támadás befejeződött (talált vagy sem)
    public event Action<BossAttackBase, bool> OnAttackResolved;

    ///A BossAttackManager ezen keresztül kaphat információt a támadás sikerességéről
    public void ResolveAttack(bool hit)
    {
        OnAttackResolved?.Invoke(this, hit);
    }

    ///Az AI döntési logikája ezt hívja, hogy pontozza, mennyire érdemes ezt a támadást használni
    public virtual float GetHeuristicScore(Transform player, Transform boss)
    {
        return 0f; // alapértelmezett
    }

    ///A támadás konkrét végrehajtása
    public abstract void Execute(Transform player);
}
