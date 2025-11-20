using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
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

    //eredeti értékek mentéséhez
    private float originalMoveSpeed;
    private float originalJumpStrength;
    private int originalBonusJumps;
    private Coroutine speedBoostCoroutine;
    private bool isCrouching;

    //BoxCollider méretek
    private Vector2 standingOffset = new Vector2(0.0465f, -0.520f);
    private Vector2 standingSize = new Vector2(0.8738f, 1.3773f);
    private Vector2 crouchingOffset = new Vector2(0.0465f, -0.723f);
    private Vector2 crouchingSize = new Vector2(0.8738f, 0.9721f);

    //Teszt input
    [SerializeField] private bool useTestingInput = false;
    private float testingHorizontalInput = 0f;

    [SerializeField] private bool useTestingCrouch = false;
    private bool testingCrouch = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        originalMoveSpeed = moveSpeed;
        originalJumpStrength = jumpStrength;
        originalBonusJumps = bonusJumps;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        if (playerHealth != null && playerHealth.IsDead())
        {
            body.velocity = Vector2.zero;
            return;
        }

        horizontalInput = useTestingInput ? testingHorizontalInput : Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", CheckGround());

        // Guggolás felülbírálása tesztnél
        if (useTestingCrouch)
            isCrouching = testingCrouch;
        else
            isCrouching = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);

        UpdateCollider();
        anim.SetBool("crouch", isCrouching);

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            DoJump();

        if ((Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) && body.velocity.y > 0)
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * 0.5f);

        if (CheckWall() && !CheckGround())
        {
            body.gravityScale = 0;
            body.velocity = Vector2.zero;
        }
        else
        {
            body.gravityScale = 7;

            if (!isCrouching)
            {
                Vector2 moveDir = new Vector2(horizontalInput * moveSpeed, body.velocity.y);

                if (horizontalInput != 0)
                {
                    bool hitDoor = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f,
                                      new Vector2(Mathf.Sign(horizontalInput), 0), 0.1f)
                                   .collider?.CompareTag("closedDoor") ?? false;

                    body.velocity = hitDoor ? new Vector2(0, body.velocity.y) : moveDir;
                }
                else
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
            else
            {
                body.velocity = new Vector2(0, body.velocity.y);
            }

            if (CheckGround())
            {
                coyoteTimer = coyoteDuration;
                jumpsleft = bonusJumps;
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
            }
        }
    }

    private void UpdateCollider()
    {
        if (isCrouching)
        {
            boxCollider.offset = crouchingOffset;
            boxCollider.size = crouchingSize;
        }
        else
        {
            boxCollider.offset = standingOffset;
            boxCollider.size = standingSize;
        }
    }

    private void DoJump()
    {
        if (coyoteTimer < 0 && !CheckWall() && jumpsleft <= 0) return;

        if (SoundManager.instance != null)
            SoundManager.instance.PlaySound(jumpSound);

        if (CheckWall() && !CheckGround())
            ExecuteWallJump();
        else
        {
            if (CheckGround() || coyoteTimer > 0)
                body.velocity = new Vector2(body.velocity.x, jumpStrength);
            else if (jumpsleft > 0)
            {
                body.velocity = new Vector2(body.velocity.x, jumpStrength);
                jumpsleft--;
            }

            coyoteTimer = 0;
        }
    }

    private void ExecuteWallJump()
    {
        body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX, wallJumpY));
        wallJumpColdown = 0;
    }

    public bool CheckGround()
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

    public void ApplySpeedBoost(float duration, float speedMultiplier, float jumpMultiplier, int extraJumps)
    {
        if (speedBoostCoroutine != null)
            StopCoroutine(speedBoostCoroutine);

        speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine(duration, speedMultiplier, jumpMultiplier, extraJumps));
    }

    private IEnumerator SpeedBoostCoroutine(float duration, float speedMultiplier, float jumpMultiplier, int extraJumps)
    {
        moveSpeed = originalMoveSpeed * speedMultiplier;
        jumpStrength = originalJumpStrength * jumpMultiplier;
        bonusJumps = originalBonusJumps + extraJumps;

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed;
        jumpStrength = originalJumpStrength;
        bonusJumps = originalBonusJumps;

        speedBoostCoroutine = null;
    }

    public void SetHorizontalInput(float value)
    {
        useTestingInput = true;
        testingHorizontalInput = value;
    }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public void JumpForTest(int extraJumps = 0)
    {
        jumpsleft += extraJumps;
        DoJump();
    }

    public void CrouchForTest(bool crouch)
    {
        useTestingCrouch = true;
        testingCrouch = crouch;
        UpdateCollider();
    }
}
