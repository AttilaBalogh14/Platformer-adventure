using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class PlayerMovementTests
{
    private GameObject player;
    private PlayerMovement movement;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        //Betöltjük a Level1 scene-t
        yield return SceneManager.LoadSceneAsync("Level1");

        player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "A Player nem található a Level1 scene-ben!");

        movement = player.GetComponent<PlayerMovement>();
        Assert.IsNotNull(movement, "A PlayerMovement script hiányzik a Player-ről!");

        //Teszt input nullázása
        movement.SetHorizontalInput(0f);
    }

    [UnityTest]
    public IEnumerator Player_Moves_Right()
    {
        float startX = player.transform.position.x;

        movement.SetHorizontalInput(1f); //jobbra

        yield return new WaitForSeconds(0.2f);

        Assert.Greater(player.transform.position.x, startX, "A játékosnak jobbra kellene mozognia!");
    }

    [UnityTest]
    public IEnumerator Player_Moves_Left()
    {
        float startX = player.transform.position.x;

        movement.SetHorizontalInput(-1f); //balra

        yield return new WaitForSeconds(0.2f);

        Assert.Less(player.transform.position.x, startX, "A játékosnak balra kellene mozognia!");
    }

    [UnityTest]
    public IEnumerator Player_Jumps_Once()
    {
        float startY = player.transform.position.y;

        movement.JumpForTest();

        yield return new WaitForSeconds(0.2f);

        Assert.Greater(player.transform.position.y, startY + 0.1f, "A játékosnak ugornia kellett volna!");
    }

    [UnityTest]
    public IEnumerator Player_Jumps_Twice()
    {
        float startY = player.transform.position.y;

        movement.JumpForTest();
        yield return new WaitForSeconds(0.2f);

        float midY = player.transform.position.y;
        Assert.Greater(midY, startY + 0.1f, "A játékosnak az első ugrásnál ugornia kellett!");

        movement.JumpForTest(1);
        yield return new WaitForSeconds(0.2f);

        float finalY = player.transform.position.y;
        Assert.Greater(finalY, midY + 0.1f, "A játékosnak a dupla ugrásnál is ugornia kellett!");
    }

    [UnityTest]
    public IEnumerator Player_Crouches()
    {
        BoxCollider2D collider = player.GetComponent<BoxCollider2D>();
        float originalHeight = collider.size.y;

        movement.CrouchForTest(true);

        yield return null;

        Assert.Less(collider.size.y, originalHeight, "A játékosnak guggolnia kellett volna!");
    }
}
