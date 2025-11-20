using UnityEngine;

public class ActivateRoomPoint : MonoBehaviour
{
    [SerializeField] public GameObject objectToShow;

    private void Start()
    {
        if (objectToShow != null)
            objectToShow.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered activateRoomPoint!");

            if (objectToShow != null)
                objectToShow.SetActive(true);
        }
    }
}
