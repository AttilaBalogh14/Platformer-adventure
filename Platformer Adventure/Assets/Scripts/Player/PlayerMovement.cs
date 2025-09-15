using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpStrength;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteDuration;
    private float coyoteTimer;

    [Header("Multiple jumps")]
    [SerializeField] private int bonusJumps;

    [Header("Wall jumping")]
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;

    private int jumpsleft;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpColdown;
    private float horizontalInput;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSound;

    private Health playerHealth;

    // 🔹 eredeti értékek mentéséhez
    private float originalMoveSpeed;
    private float originalJumpStrength;
    private int originalBonusJumps;
    private Coroutine speedBoostCoroutine;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        // 🔹 elmentjük az eredeti értékeket
        originalMoveSpeed = moveSpeed;
        originalJumpStrength = jumpStrength;
        originalBonusJumps = bonusJumps;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", CheckGround());

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            DoJump();

        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * 0.5f);

        if (CheckWall() && !CheckGround())
        {
            body.gravityScale = 0;
            body.velocity = Vector2.zero;
        }
        else
        {
            body.gravityScale = 7;
            body.velocity = new Vector2(horizontalInput * moveSpeed, body.velocity.y);

            if (CheckGround())
            {
                coyoteTimer = coyoteDuration;
                jumpsleft = bonusJumps;
            }
            else
                coyoteTimer -= Time.deltaTime;
        }
    }

    private void DoJump()
    {
        if (coyoteTimer < 0 && !CheckWall() && jumpsleft <= 0)
            return;

        if (SoundManager.instance != null)
            SoundManager.instance.PlaySound(jumpSound);

        if (CheckWall() && !CheckGround())
            ExecuteWallJump();
        else
        {
            if (CheckGround() || coyoteTimer > 0)
                body.velocity = new Vector2(body.velocity.x, jumpStrength);
            else
            {
                if (coyoteTimer > 0)
                    body.velocity = new Vector2(body.velocity.x, jumpStrength);
                else
                {
                    if (jumpsleft > 0)
                    {
                        body.velocity = new Vector2(body.velocity.x, jumpStrength);
                        jumpsleft--;
                    }
                }
            }
            coyoteTimer = 0;
        }
    }

    private void ExecuteWallJump()
    {
        body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX, wallJumpY));
        wallJumpColdown = 0;
    }

    private bool CheckGround()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool CheckWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack()
    {
        return horizontalInput == 0 && CheckGround() && !CheckWall();
    }

    // 🔹 Új metódus: Speed Boost alkalmazása
    public void ApplySpeedBoost(float duration, float speedMultiplier, float jumpMultiplier, int extraJumps)
    {
        // ha már fut egy boost, állítsuk le
        if (speedBoostCoroutine != null)
            StopCoroutine(speedBoostCoroutine);

        speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine(duration, speedMultiplier, jumpMultiplier, extraJumps));
    }

    private IEnumerator SpeedBoostCoroutine(float duration, float speedMultiplier, float jumpMultiplier, int extraJumps)
    {
        // ideiglenes értékek
        moveSpeed = originalMoveSpeed * speedMultiplier;
        jumpStrength = originalJumpStrength * jumpMultiplier;
        bonusJumps = originalBonusJumps + extraJumps;

        yield return new WaitForSeconds(duration);

        // visszaállítás
        moveSpeed = originalMoveSpeed;
        jumpStrength = originalJumpStrength;
        bonusJumps = originalBonusJumps;

        speedBoostCoroutine = null;
    }
}
