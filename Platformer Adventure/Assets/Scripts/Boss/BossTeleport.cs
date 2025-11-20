using System.Collections;
using UnityEngine;

public class BossTeleport : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float behindDistance = 3f;
    [SerializeField] private float minTeleportDelay = 3f;
    [SerializeField] private float maxTeleportDelay = 6f;
    [SerializeField] private float highPointTimeout = 5f;
    [SerializeField] private int teleportHpThreshold = 10;
    [SerializeField] private bool alwaysTeleportHigh = true;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;
    [SerializeField] private Transform highPoint;
    [SerializeField] private GameObject highPointPlatform;

    private Health bossHealth;
    private BossLearningMemory memory;
    private BossMovement movement;
    private Rigidbody2D rb;

    private bool isTeleporting = false;
    private bool isOnHighPoint = false;
    private bool teleportPhaseActive = false;
    private bool isActive = false;
    private float timeOnHighPoint = 0f;
    private Coroutine teleportRoutine;

    private void Awake()
    {
        bossHealth = GetComponent<Health>();
        memory = GetComponent<BossLearningMemory>();
        movement = GetComponent<BossMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (movement == null || !isActive || !movement.BossIsAwake())
            return;

        if (!teleportPhaseActive && bossHealth != null && bossHealth.currentHealth <= teleportHpThreshold)
        {
            teleportPhaseActive = true;
            teleportRoutine = StartCoroutine(TeleportRoutine());
        }

        if (isOnHighPoint)
        {
            timeOnHighPoint += Time.deltaTime;
            if (timeOnHighPoint >= highPointTimeout || (memory != null && memory.TimeSinceLastHit > 3f))
            {
                StartCoroutine(TeleportDecision(false));
                timeOnHighPoint = 0f;
            }
        }
    }

    private IEnumerator TeleportRoutine()
    {
        while (isActive && teleportPhaseActive)
        {
            yield return StartCoroutine(TeleportDecision());
            yield return new WaitForSeconds(Random.Range(minTeleportDelay, maxTeleportDelay));
        }
    }

    private IEnumerator TeleportDecision(bool allowHigh = true)
    {
        if (isTeleporting || !isActive)
            yield break;

        isTeleporting = true;
        Vector3 targetPos;
        bool teleportHigh = allowHigh && (alwaysTeleportHigh || ShouldTeleportHigh());

        if (teleportHigh && highPoint != null)
        {
            targetPos = highPoint.position;
            if (highPointPlatform != null) highPointPlatform.SetActive(true);
            isOnHighPoint = true;
            timeOnHighPoint = 0f;

            if (movement != null) movement.allowMovement = movement.allowDash = false;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            targetPos = player.position + new Vector3(-Mathf.Sign(player.localScale.x) * behindDistance, 0f, 0f);
            targetPos.x = Mathf.Clamp(targetPos.x, leftLimit.position.x, rightLimit.position.x);
            targetPos.y = transform.position.y;

            if (highPointPlatform != null && highPointPlatform.activeSelf)
                highPointPlatform.SetActive(false);

            isOnHighPoint = false;
        }

        if (rb != null)
        {
            rb.position = targetPos;
            rb.velocity = Vector2.zero;
        }
        else transform.position = targetPos;

        Vector3 dir = player.position - transform.position;
        transform.localScale = new Vector3(
            dir.x < 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);

        yield return new WaitForSeconds(0.1f);

        if (!isOnHighPoint && movement != null)
        {
            movement.allowMovement = movement.allowDash = true;
            if (rb != null) rb.gravityScale = 3f;
        }

        isTeleporting = false;
    }

    //Fuzzy döntés a magas teleporthoz
    private bool ShouldTeleportHigh()
    {
        if (memory == null || bossHealth == null) return false;

        float hp = memory.BossHealthPercent();
        float aggr = memory.AggressionLevel;
        float jump = memory.JumpTendency;
        float dmg = memory.HealthSinceLastMove;

        float hpLow = FuzzyLogic.Low(hp);
        float aggrHigh = FuzzyLogic.High(aggr);
        float jumpHigh = FuzzyLogic.High(jump);
        float dmgHigh = FuzzyLogic.High(dmg / 5f);

        //Soft fuzzy szabály
        float rule = FuzzyLogic.SoftAnd(FuzzyLogic.SoftOr(hpLow, dmgHigh), FuzzyLogic.SoftOr(aggrHigh, jumpHigh));

        //Random faktor hozzáadva
        rule += Random.Range(-0.1f, 0.2f);
        rule = Mathf.Clamp01(rule);

        return Random.value < rule;
    }


    public void ResetTeleportState()
    {
        StopAllCoroutines();
        teleportRoutine = null;
        teleportPhaseActive = false;
        isTeleporting = false;
        isOnHighPoint = false;
        timeOnHighPoint = 0f;
        isActive = false;

        if (movement != null)
        {
            movement.allowMovement = true;
            movement.allowDash = true;
        }

        if (rb != null)
        {
            rb.gravityScale = 3f;
            rb.velocity = Vector2.zero;
        }

        if (highPointPlatform != null)
            highPointPlatform.SetActive(false);
    }

    public void ActivateTeleport()
    {
        isActive = true;
        teleportPhaseActive = false;
        isTeleporting = false;
        isOnHighPoint = false;
        timeOnHighPoint = 0f;
    }
}
