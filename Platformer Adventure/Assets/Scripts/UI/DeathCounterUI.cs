using UnityEngine;
using TMPro;

public class DeathCounterUI : MonoBehaviour
{
    [Header("UI Component")]
    public TextMeshProUGUI deathCountText;

    private void Awake()
    {
        if (deathCountText == null)
            Debug.Log("DeathCounterUI: Nincs beállítva a TextMeshProUGUI komponens!");
    }

    private void Update()
    {
        //Frissítjük a szöveget a static deathCount változóval
        deathCountText.text = Health.DeathCounter().ToString();
    }
}
