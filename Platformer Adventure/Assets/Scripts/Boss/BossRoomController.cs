using System.Collections;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [Header("Boss Room References")]
    [SerializeField] private Transform player;
    [SerializeField] private Health bossHealth;
    [SerializeField] private Health playerHealth;
    [SerializeField] private BossLearningMemory bossMemory;
    [SerializeField] private GameObject[] healthPickups;
    [SerializeField] private GameObject invulnerabilityPickupPrefab;

    [Header("Health Pickup AI Settings")]
    [SerializeField] private float minRespawnDelay = 5f;
    [SerializeField] private float maxRespawnDelay = 15f;

    [Header("Invulnerability Pickup Settings")]
    [SerializeField] private float minDistanceFromPlayer = 2f;
    [SerializeField] private float maxDistanceFromPlayer = 6f;
    [SerializeField] private Transform minSpawnPoint;
    [SerializeField] private Transform maxSpawnPoint;
    [SerializeField] private float fixedY = -2.6f;
    [SerializeField] private float minInvulSpawnCooldown = 8f;
    [SerializeField] private float maxInvulSpawnCooldown = 18f;

    [Header("Fuzzy Player Low HP Settings")]
    [SerializeField] private float lowHPThreshold = 0.3f;
    [SerializeField] private float maxLowHPTimeEffect = 10f;

    private bool bossFightActive = false;
    private int initialGlobalDeathCount = 0;
    private int deathsDuringBossFight = 0;
    private float playerLowHPTime = 0f;
    private float nextInvulSpawnTime = 0f;

    public void OnBossWakeUp()
    {
        if (bossFightActive) return;

        bossFightActive = true;
        initialGlobalDeathCount = Health.DeathCounter();
        deathsDuringBossFight = 0;

        foreach (var pickup in healthPickups)
        {
            if (pickup != null)
                pickup.SetActive(true);
        }

        foreach (var pickup in healthPickups)
        {
            StartCoroutine(HealthPickupRespawnRoutine(pickup));
        }
    }

    private void Update()
    {
        if (!bossFightActive) return;

        deathsDuringBossFight = Health.DeathCounter() - initialGlobalDeathCount;

        UpdatePlayerLowHPTime();

        TrySpawnInvulnerabilityFuzzy();
    }

    private void UpdatePlayerLowHPTime()
    {
        float lowHPRate = 1f;
        float decayRate = 0.3f;

        if (playerHealth.currentHealth / playerHealth.startingHealth < lowHPThreshold)
        {
            playerLowHPTime += Time.deltaTime * lowHPRate;
            if (playerLowHPTime > maxLowHPTimeEffect)
                playerLowHPTime = maxLowHPTimeEffect;
        }
        else
        {
            playerLowHPTime -= Time.deltaTime * decayRate;
            if (playerLowHPTime < 0f)
                playerLowHPTime = 0f;
        }
    }

    private IEnumerator HealthPickupRespawnRoutine(GameObject pickup)
    {
        while (bossFightActive)
        {
            if (pickup != null && !pickup.activeSelf)
            {
                float respawnDelay = GetAdaptiveRespawnDelay();
                yield return new WaitForSeconds(respawnDelay);
                pickup.SetActive(true);

                Debug.Log($"[HealthPickup] Spawnolt újra: Delay={respawnDelay:F2}s, deathsDuringBossFight={deathsDuringBossFight}");
            }
            else
            {
                yield return null;
            }
        }
    }

    private float GetAdaptiveRespawnDelay()
    {
        if (bossMemory == null || bossHealth == null || playerHealth == null)
            return Random.Range(minRespawnDelay, maxRespawnDelay);

        float healthPercent = bossMemory.BossHealthPercent();
        float aggression = bossMemory.AggressionLevel;
        float jumpTendency = bossMemory.JumpTendency;
        float deathsFactor = Mathf.Clamp01((float)deathsDuringBossFight / 5f);

        //Fuzzy szabályok
        float healthLow = FuzzyLogic.Low(healthPercent);
        float aggrHigh = FuzzyLogic.High(aggression);
        float jumpHigh = FuzzyLogic.High(jumpTendency);
        float deathsHigh = FuzzyLogic.High(deathsFactor);

        float fuzzyScore = FuzzyLogic.Or(FuzzyLogic.Or(healthLow, aggrHigh), FuzzyLogic.Or(jumpHigh, deathsHigh));
        fuzzyScore = Mathf.Clamp01(fuzzyScore);

        float respawnDelay = Mathf.Lerp(maxRespawnDelay, minRespawnDelay, fuzzyScore);
        respawnDelay *= Random.Range(0.9f, 1.1f);

        return respawnDelay;
    }

    private void TrySpawnInvulnerabilityFuzzy()
    {
        if (invulnerabilityPickupPrefab == null || player == null || bossMemory == null || playerHealth == null || minSpawnPoint == null || maxSpawnPoint == null)
            return;

        if (Time.time < nextInvulSpawnTime)
            return;

        //Fuzzy halmazok
        float playerHPPercent = playerHealth.currentHealth / playerHealth.startingHealth;
        float bossHPPercent = bossMemory.BossHealthPercent();
        float aggression = bossMemory.AggressionLevel;
        float jumpTendency = bossMemory.JumpTendency;
        float deathsFactor = Mathf.Clamp01((float)deathsDuringBossFight / 5f);
        float lowHPTimeFactor = Mathf.Clamp01(playerLowHPTime / maxLowHPTimeEffect);

        float playerHPLow = FuzzyLogic.Low(playerHPPercent);
        float playerHPMed = FuzzyLogic.Medium(playerHPPercent);
        float playerHPHigh = FuzzyLogic.High(playerHPPercent);

        float bossHPLow = FuzzyLogic.Low(bossHPPercent);
        float bossHPHigh = FuzzyLogic.High(bossHPPercent);

        float aggrHigh = FuzzyLogic.High(aggression);
        float jumpHigh = FuzzyLogic.High(jumpTendency);
        float deathsHigh = FuzzyLogic.High(deathsFactor);
        float lowHPTimeHigh = FuzzyLogic.High(lowHPTimeFactor);

        //Fuzzy szabályok a spawn esélyre
        float rule1 = FuzzyLogic.Or(playerHPLow, lowHPTimeHigh);
        float rule2 = bossHPLow;
        float rule3 = FuzzyLogic.Or(aggrHigh, jumpHigh);
        float rule4 = deathsHigh;

        float criticalScore = FuzzyLogic.Or(FuzzyLogic.Or(rule1, rule2), FuzzyLogic.Or(rule3, rule4));
        criticalScore = Mathf.Clamp01(criticalScore);

        float spawnChancePerSecond = Mathf.Lerp(0.01f, 0.4f, criticalScore); 
        if (Random.value < spawnChancePerSecond * Time.deltaTime)
        {
            float spawnX = player.position.x;
            int tries = 0;
            bool validPos = false;
            do
            {
                spawnX = Random.Range(minSpawnPoint.position.x, maxSpawnPoint.position.x);
                float distanceToPlayer = Mathf.Abs(spawnX - player.position.x);
                if (distanceToPlayer >= minDistanceFromPlayer && distanceToPlayer <= maxDistanceFromPlayer)
                {
                    validPos = true;
                    break;
                }
                tries++;
            } while (tries < 20);

            if (!validPos)
                spawnX = player.position.x + (Random.value > 0.5f ? minDistanceFromPlayer : -minDistanceFromPlayer);

            Vector2 spawnPos = new Vector2(spawnX, fixedY);
            Instantiate(invulnerabilityPickupPrefab, spawnPos, Quaternion.identity);

            float cooldown = Mathf.Lerp(maxInvulSpawnCooldown, minInvulSpawnCooldown, criticalScore);
            nextInvulSpawnTime = Time.time + cooldown;

            Debug.Log($"[InvulPickup Fuzzy] Spawnolt: pos={spawnPos}, criticalScore={criticalScore:F2}, deathsDuringBossFight={deathsDuringBossFight}, lowHPTime={playerLowHPTime:F2}s, nextSpawnCooldown={cooldown:F2}s");
        }
    }

    public int GetDeathsDuringBossFight()
    {
        return deathsDuringBossFight;
    }
}
