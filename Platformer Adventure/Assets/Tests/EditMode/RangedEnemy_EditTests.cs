using NUnit.Framework;
using UnityEngine;

public class RangedEnemyEditModeTests
{
    private GameObject enemyObj;
    private RangedEnemy rangedEnemy;

    private GameObject[] projectiles;

    [SetUp]
    public void Setup()
    {
        enemyObj = new GameObject("RangedEnemy");
        rangedEnemy = enemyObj.AddComponent<RangedEnemy>();

        rangedEnemy.GetType()
            .GetField("projectiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(rangedEnemy, CreateProjectileArray());
    }

    private GameObject[] CreateProjectileArray()
    {
        projectiles = new GameObject[3];

        for (int i = 0; i < projectiles.Length; i++)
        {
            projectiles[i] = new GameObject("Projectile" + i);
            projectiles[i].SetActive(false);
        }

        return projectiles;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(enemyObj);

        foreach (var proj in projectiles)
            Object.DestroyImmediate(proj);
    }

    //1. Ha van inaktív projectile, akkor azt adja vissza
    [Test]
    public void FindInactiveProjectile_ReturnsCorrectIndex()
    {
        projectiles[0].SetActive(true);
        projectiles[1].SetActive(false);
        projectiles[2].SetActive(true);

        int index = (int)rangedEnemy.GetType()
            .GetMethod("FindInactiveProjectile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(rangedEnemy, null);

        Assert.AreEqual(1, index);
    }

    //2. Ha minden projectile aktív, akkor fallback = 0
    [Test]
    public void FindInactiveProjectile_WhenAllActive_ReturnsZero()
    {
        projectiles[0].SetActive(true);
        projectiles[1].SetActive(true);
        projectiles[2].SetActive(true);

        int index = (int)rangedEnemy.GetType()
            .GetMethod("FindInactiveProjectile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(rangedEnemy, null);

        Assert.AreEqual(0, index);
    }
}
