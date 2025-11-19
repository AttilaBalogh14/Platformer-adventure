using NUnit.Framework;
using UnityEngine;

public class HealthEditModeTests
{
    private GameObject obj;
    private Health health;

    [SetUp]
    public void Setup()
    {
        obj = new GameObject("Player");
        health = obj.AddComponent<Health>();

        obj.AddComponent<Animator>();
        obj.AddComponent<Rigidbody2D>();
        obj.AddComponent<BoxCollider2D>();

        health.components = new Behaviour[0];

        //Kezdő adatok
        health.startingHealth = 3;
        health.currentHealth = 3;

        //Dead flag reset
        typeof(Health).GetField("isDead",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(health, false);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(obj);
    }

    //AddHealth nem lépi túl a maximumot
    [Test]
    public void AddHealth_DoesNotExceedStartingHealth()
    {
        health.currentHealth = 3;

        health.AddHealth(5);

        Assert.AreEqual(3, health.currentHealth);
    }

    //ResetHealth visszaállítja a HP-t maxra (NEM dob többé NullRef-et)
    [Test]
    public void ResetHealth_RestoresFullHealth()
    {
        health.currentHealth = 1;

        health.ResetHealth();

        Assert.AreEqual(3, health.currentHealth);
        Assert.IsFalse(health.IsDead());
    }

    //TakeDamage nem mehet 0 alá
    [Test]
    public void TakeDamage_DoesNotGoBelowZero()
    {
        health.currentHealth = 1;

        health.TestDamage(5);

        Assert.AreEqual(0, health.currentHealth);
        Assert.IsTrue(health.IsDead());
    }

    //AddHealth normál esetben növeli az életet
    [Test]
    public void AddHealth_IncreasesHealthNormally()
    {
        health.currentHealth = 2;

        health.AddHealth(1);

        Assert.AreEqual(3, health.currentHealth);
    }

    //ResetPlayerState működik-e (privát metódus tesztelése)
    [Test]
    public void ResetPlayerState_SetsCorrectState()
    {
        health.currentHealth = 1;

        var method = typeof(Health).GetMethod(
            "ResetPlayerState",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method?.Invoke(health, null);

        Assert.AreEqual(3, health.currentHealth);
        Assert.IsFalse(health.IsDead());
    }
}
