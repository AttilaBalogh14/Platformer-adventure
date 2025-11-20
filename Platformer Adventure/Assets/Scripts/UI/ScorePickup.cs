using UnityEngine;

public class ScorePickup : MonoBehaviour
{
    [SerializeField] private int scoreAmount; //ennyi pontot ad

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //hozzáadjuk a pontot
            ScoreEvents.AddScore(scoreAmount);

            //eltüntetjük a pickupot
            gameObject.SetActive(false);

        }
    }
}
