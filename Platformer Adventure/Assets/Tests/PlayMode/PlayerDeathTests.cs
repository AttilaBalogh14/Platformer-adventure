using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class PlayerDeathTests
{
    private GameObject player;
    private Health health;
    private UIManager uiManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return SceneManager.LoadSceneAsync("Level1");

        player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "A Player nem található a Level1 scene-ben!");

        health = player.GetComponent<Health>();
        Assert.IsNotNull(health, "A Health script hiányzik a Player-ről!");

        uiManager = Object.FindObjectOfType<UIManager>();
        Assert.IsNotNull(uiManager, "UIManager nem található a scene-ben!");

        //Biztosítsuk, hogy GameOver képernyő ki legyen kapcsolva
        if (uiManager.gameOverScreen != null)
            uiManager.gameOverScreen.SetActive(false);

        health.ResetHealth();
    }

    [UnityTest]
    public IEnumerator PlayerDies_WhenHealthReachesZero()
    {
        //Biztosra megyünk, hogy a health nagyon alacsony
        health.TakeDamage(health.currentHealth);

        //Várjunk 1 frame-et, hogy a Die() lefusson
        yield return null;

        Assert.IsTrue(health.IsDead(), "A játékosnak halottnak kell lennie!");
        Assert.LessOrEqual(health.currentHealth, 0, "A játékos életereje nulla vagy kevesebb kell legyen!");
    }

    [UnityTest]
    public IEnumerator PlayerComponentsDisabledOnDeath()
    {
        health.TakeDamage(health.currentHealth);
        yield return null;

        foreach (var comp in health.components)
        {
            Assert.IsFalse(comp.enabled, "A komponenseknek inaktiválva kell lenniük a halál után!");
        }
    }

    [UnityTest]
    public IEnumerator GameOverScreenActivatesOnPlayerDeath()
    {
        health.TakeDamage(health.currentHealth);

        // A Health osztályban 1 másodperces delay van a GameOver megjelenéshez
        yield return new WaitForSecondsRealtime(1.1f);

        Assert.IsTrue(uiManager.gameOverScreen.activeSelf, "A GameOver képernyőnek aktívnak kell lennie a halál után!");
    }
}
