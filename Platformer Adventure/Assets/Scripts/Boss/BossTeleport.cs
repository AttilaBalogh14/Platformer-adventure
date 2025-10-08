using System.Collections;
using UnityEngine;

public class BossTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private float behindDistance = 3f;
    [SerializeField] private float minTeleportDelay = 3f;
    [SerializeField] private float maxTeleportDelay = 6f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;
    [SerializeField] private Transform highPoint; // magas fix pont
    [SerializeField] private GameObject highPointPlatform; // magas platform megjelenítéshez

    [Header("Boss Health Threshold")]
    [SerializeField] private int teleportHpThreshold = 10;

    [Header("Test Mode")]
    [SerializeField] private bool alwaysTeleportHigh = true; // mindig magas pontba teleportál

    private Health bossHealth;
    private BossLearningMemory memory;
    private bool teleportPhaseActive = false;
    private bool isTeleporting = false;
    private bool isOnHighPoint = false; // true ha a magas pontban van

    private Rigidbody2D rb;
    private BossMovement movement;

    private void Awake()
    {
        bossHealth = GetComponent<Health>();
        memory = GetComponent<BossLearningMemory>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<BossMovement>();

        if (player == null) Debug.LogWarning("BossTeleport: Player reference is missing!");
        if (leftLimit == null || rightLimit == null) Debug.LogWarning("BossTeleport: Left/Right limit references are missing!");
        if (highPoint == null) Debug.LogWarning("BossTeleport: HighPoint reference is missing!");
    }

    private void Update()
    {
        // Ha magas pontban van, ne mozogjon, de támadás maradjon aktív
        if (isOnHighPoint) return;

        if (!teleportPhaseActive && bossHealth != null && bossHealth.currentHealth <= teleportHpThreshold)
        {
            teleportPhaseActive = true;
            StartCoroutine(TeleportRoutine());
        }
    }

    private IEnumerator TeleportRoutine()
    {
        while (teleportPhaseActive)
        {
            yield return StartCoroutine(TeleportDecision());
            float delay = Random.Range(minTeleportDelay, maxTeleportDelay);
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator TeleportDecision()
    {
        if (isTeleporting) yield break;
        isTeleporting = true;

        yield return null; // várjunk 1 frame-et

        Vector3 targetPos;
        bool teleportHigh = alwaysTeleportHigh || ShouldTeleportHigh();

        if (teleportHigh && highPoint != null)
        {
            targetPos = highPoint.position;
            Debug.Log($"[Teleport TEST] HIGH to {targetPos}");
            if (highPointPlatform != null) highPointPlatform.SetActive(true);

            // magas pontban maradás: csak mozgás tiltása
            isOnHighPoint = true;
            if (movement != null) 
            {
                movement.allowMovement = false; // ne mozogjon
                movement.allowDash = false;     // ne dash-eljen
            }
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // player mögé teleportálás
            targetPos = player.position + new Vector3(-Mathf.Sign(player.localScale.x) * behindDistance, 0f, 0f);
            targetPos.x = Mathf.Clamp(targetPos.x, leftLimit.position.x, rightLimit.position.x);
            targetPos.y = transform.position.y;

            Debug.Log($"[Teleport TEST] BEHIND player to {targetPos}");

            if (highPointPlatform != null && highPointPlatform.activeSelf)
                highPointPlatform.SetActive(false);

            isOnHighPoint = false; // nincs magas pontban
        }

        // teleportálás
        if (rb != null)
        {
            rb.position = targetPos;
            rb.velocity = Vector2.zero;
        }
        else
        {
            transform.position = targetPos;
        }

        // fordulás a player felé
        Vector3 dir = player.position - transform.position;
        transform.localScale = new Vector3(dir.x < 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y,
                                           transform.localScale.z);

        // rövid delay a stabil teleporthoz
        yield return new WaitForSeconds(0.1f);

        // csak akkor állítsuk vissza a mozgást és gravity-t, ha nem magas pont
        if (!isOnHighPoint)
        {
            if (rb != null) rb.gravityScale = 3f;
            if (movement != null) 
            {
                movement.allowMovement = true;
                movement.allowDash = true;
            }
        }

        isTeleporting = false;
    }

    private bool ShouldTeleportHigh()
    {
        if (memory == null) return false;

        float hpPercent = bossHealth.currentHealth / bossHealth.startingHealth;
        float aggression = memory.AggressionLevel;
        float jumpTendency = memory.JumpTendency;
        float damageTaken = memory.HealthSinceLastMove;

        float chance = 0.2f;
        if (hpPercent < 0.3f) chance += 0.2f;
        if (aggression > 0.6f) chance += 0.2f;
        if (jumpTendency > 0.5f) chance += 0.2f;
        if (damageTaken >= 3f) chance += 0.2f;

        chance = Mathf.Clamp01(chance);

        Debug.Log($"[TeleportChance] {chance} (HP:{hpPercent}, Agg:{aggression}, Jump:{jumpTendency}, Damage:{damageTaken})");

        return Random.value < chance;
    }

    private void OnDrawGizmosSelected()
    {
        if (highPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(highPoint.position, 0.3f);
        }

        if (player != null)
        {
            Vector3 behind = player.position + new Vector3(-Mathf.Sign(player.localScale.x) * behindDistance, 0f, 0f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(behind, 0.3f);
        }
    }
}
