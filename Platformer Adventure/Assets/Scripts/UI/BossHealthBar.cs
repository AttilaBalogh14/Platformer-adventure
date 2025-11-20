using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Hozzáadva a TextMeshPro-hoz

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject boss;      //A boss GameObject
    [SerializeField] private Image healthBarFill;  //A fill Image a sávban
    [SerializeField] private TMP_Text healthPercentageText; //TextMeshPro Text

    private Health bossHealth;

    private void Start()
    {
        if (boss == null)
        {
            Debug.LogError("Boss GameObject nincs beállítva a HealthBar-on!");
            return;
        }

        bossHealth = boss.GetComponent<Health>();

        if (bossHealth == null)
        {
            Debug.LogError("A kiválasztott boss GameObject nem tartalmaz Health komponenst!");
        }
    }

    private void Update()
    {
        if (bossHealth == null || healthBarFill == null)
            return;

        float fillAmount = (float)bossHealth.currentHealth / bossHealth.startingHealth;
        healthBarFill.fillAmount = Mathf.Clamp01(fillAmount);

        if (healthPercentageText != null)
        {
            int percent = Mathf.RoundToInt(fillAmount * 100);
            healthPercentageText.text = percent + "%";
        }
    }
}
