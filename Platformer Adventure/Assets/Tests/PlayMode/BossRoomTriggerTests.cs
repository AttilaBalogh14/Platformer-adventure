using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class BossRoomTriggerTests
{
    private GameObject player;
    private Health playerHealth;
    private BossMovement boss;
    private GameObject triggerObject;
    private BossRoomTrigger trigger;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return SceneManager.LoadSceneAsync("Level1");

        //Player megtalálása
        player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "A Player nem található a Level1 scene-ben!");
        playerHealth = player.GetComponent<Health>();
        Assert.IsNotNull(playerHealth, "A Player Health komponense hiányzik!");

        //Boss megtalálása
        BossMovement[] bosses = GameObject.FindObjectsOfType<BossMovement>(true);
        Assert.IsTrue(bosses.Length > 0, "A BossMovement nem található a scene-ben!");
        boss = bosses[0];
        boss.gameObject.SetActive(true); //biztosítsuk, hogy aktív a teszt alatt
        boss.ResetBoss(); //kezdő állapot

        //Trigger megtalálása
        triggerObject = GameObject.Find("BossRoomTrigger");
        if (triggerObject == null)
        {
            triggerObject = new GameObject("BossRoomTrigger");
            triggerObject.AddComponent<BoxCollider2D>().isTrigger = true;
        }
        trigger = triggerObject.GetComponent<BossRoomTrigger>();
        if (trigger == null)
            trigger = triggerObject.AddComponent<BossRoomTrigger>();

        trigger.ResetTriggerState();

        triggerObject.transform.position = Vector3.zero;
        player.transform.position = Vector3.zero;
        boss.transform.position = new Vector3(5f, 0f, 0f); //távolabb
    }

    [UnityTest]
    public IEnumerator Trigger_ActivatesBossAndRestoresPlayerHealth()
    {
        //Player élete legyen alacsony, hogy lássuk a visszatöltést
        playerHealth.currentHealth = 1f;

        //Előzőleg biztosítsuk, hogy boss inaktív felébredés szempontjából
        boss.ResetBoss();

        //Player belép a triggerbe
        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        if (playerCollider == null)
            player.AddComponent<BoxCollider2D>();

        //Fizikailag aktiváljuk a trigger-t
        triggerObject.GetComponent<Collider2D>().isTrigger = true;
        triggerObject.GetComponent<BossRoomTrigger>().ResetTriggerState();
        triggerObject.GetComponent<BossRoomTrigger>().SendMessage("OnTriggerEnter2D", playerCollider, SendMessageOptions.DontRequireReceiver);

        yield return null; //1 frame, hogy a trigger logika lefusson

        //Ellenőrizzük, hogy a boss felébredt
        Assert.IsTrue(boss.BossIsAwake(), "A bossnak fel kellett volna ébrednie a trigger hatására!");

        //Ellenőrizzük, hogy a player élete visszatöltődött
        Assert.AreEqual(playerHealth.startingHealth, playerHealth.currentHealth, "A player élete nem lett visszatöltve!");
    }
}
