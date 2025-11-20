using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static void OpenAllRooms()
    {
        foreach (var activator in FindObjectsOfType<ActivateRoomPoint>())
        {
            if (activator != null && activator.objectToShow != null)
                activator.objectToShow.SetActive(true);
        }

        foreach (var deactivator in FindObjectsOfType<DeactivateRoomPoint>())
        {
            if (deactivator != null && deactivator.objectToHide != null)
                deactivator.objectToHide.SetActive(true);
        }

        Debug.Log("Az összes szoba kinyitva");
    }
}
