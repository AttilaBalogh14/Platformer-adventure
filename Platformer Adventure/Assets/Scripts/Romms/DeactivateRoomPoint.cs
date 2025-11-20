using UnityEngine;

public class DeactivateRoomPoint : MonoBehaviour
{
    [SerializeField] public GameObject objectToHide;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered deactivateRoomPoint!");

            if (objectToHide != null)
                objectToHide.SetActive(false);
        }
    }
}
