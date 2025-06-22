using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable
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
    public Image hpFillImage;

    private Color originalHPColor; // ✅ 원래 색상 저장

    [Header("지면 체크")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isGrounded = false;
    private bool isAttacking = false;
    private bool isDead = false;

    private PlayerSkills playerSkills;
    public bool IsDead => isDead;
    public string gameOverSceneName = "GameOverScene";

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

        if (hpFillImage != null)
        {
            hpFillImage.enabled = true;
            originalHPColor = hpFillImage.color; // ✅ 원래 색상 저장
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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (hpSlider != null)
            hpSlider.value = currentHP;

        if (hpFillImage != null)
            hpFillImage.enabled = (currentHP > 0);

        anim.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            Die();
        }

        BossController boss = FindObjectOfType<BossController>();
        if (boss != null)
        {
            boss.ApplyShieldByRatio(0.03f); // 최대 체력의 3%
        }
    }

    // 상태이상용 조용한 데미지 처리
  public void ApplyDotDamage(int damage)
{
    if (isDead) return;

    Debug.Log($"[DOT] 잠식 데미지 적용: {damage} (남은 체력: {currentHP - damage})");

    currentHP -= damage;
    currentHP = Mathf.Clamp(currentHP, 0, maxHP);

    if (hpSlider != null)
        hpSlider.value = currentHP;

    if (hpFillImage != null)
        hpFillImage.enabled = (currentHP > 0);

    if (currentHP <= 0)
    {
        Die();
    }
}



    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        StartCoroutine(DelayedSceneChange());
    }

    IEnumerator DelayedSceneChange()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(gameOverSceneName);
    }

    // ✅ 상태이상 색상 제어 함수
    public void SetHPBarColor(Color color)
    {
        if (hpFillImage != null)
            hpFillImage.color = color;
    }

    public void ResetHPBarColor()
    {
        if (hpFillImage != null)
            hpFillImage.color = originalHPColor;
    }
}
