using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    public float dashSpeed = 10f;
    public float dashDuration = 3f;
    public float dashCooldown = 7f;

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isGrounded = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleDash();
        HandleMovement();
        HandleJump();
        UpdateAnimations();

        // 대시 쿨타임 감소
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    void HandleMovement()
    {
        if (isAttacking) return;

        float move = Input.GetAxisRaw("Horizontal");
        float currentSpeed = isDashing ? dashSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(move * currentSpeed, rb.linearVelocity.y);

        if (move != 0)
        {
            sr.flipX = move < 0;
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            anim.SetBool("isJumping", true);
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isJumping", !isGrounded);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                anim.SetBool("isJumping", false);
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
