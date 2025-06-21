using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("대시")]
    public float dashSpeed = 10f;
    public float dashDuration = 3f;
    public float dashCooldown = 7f;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;

    [Header("체력")]
    public int maxHP = 100;
    private int currentHP;

    public Slider hpSlider;

    [Header("지면 체크")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isGrounded = false;
    private bool isAttacking = false;

    private PlayerSkills playerSkills;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        playerSkills = GetComponent<PlayerSkills>();

        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleDash();
        HandleMovement();
        HandleJump();
        UpdateAnimations();

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleMovement()
    {
        if (isAttacking || (playerSkills != null && playerSkills.IsFocusing)) return;

        float move = Input.GetAxisRaw("Horizontal");
        float currentSpeed = isDashing ? dashSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(move * currentSpeed, rb.linearVelocity.y);

        if (move != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (move < 0 ? -1f : 1f);
            transform.localScale = scale;
        }
    }

    void HandleJump()
    {
        if ((playerSkills != null && playerSkills.IsFocusing)) return;

        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
        // 이 부분은 레이캐스트 기반으로 대체되므로 생략 가능
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 이 부분도 생략 가능
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망");
        // 사망 애니메이션 또는 처리 추가 가능
    }
}
