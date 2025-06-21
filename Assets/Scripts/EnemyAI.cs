using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2.5f;
    public float attackRange = 1.3f;
    public float attackCooldown = 3f;
    public float knockbackForce = 3.5f;

    private float spawnTime;
    private float lastAttackTime;
    private float currentMoveSpeed;

    private float berserkStartTime;
    private readonly float berserkSpeedGrowthRate = 0.3f; // 30% per second
    private readonly float berserkSpeedMaxMultiplier = 6f;

    private int comboStep = 0;

    private Enemy enemy;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool isDead = false;
    private bool isHit = false;
    private bool isAttacking = false;
    private bool isBerserk = false;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        spawnTime = Time.time;
        currentMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (isDead || isHit || player == null) return;

        // 광분 상태 진입 (스폰 후 15초 경과)
        if (!isBerserk && Time.time - spawnTime >= 15f)
        {
            EnterBerserkMode();
        }

        // 광분 상태일 경우 이동속도 증가 처리
        if (isBerserk)
        {
            float berserkElapsed = Time.time - berserkStartTime;
            float multiplier = 1f + berserkSpeedGrowthRate * berserkElapsed;
            multiplier = Mathf.Min(multiplier, berserkSpeedMaxMultiplier);
            currentMoveSpeed = moveSpeed * multiplier;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);

            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                comboStep = 1;
                animator.SetInteger("comboStep", comboStep);

                if (isBerserk)
                {
                    animator.SetTrigger("RunAtk");
                    Debug.Log("[EnemyAI] 광분 공격 실행!");
                }
                else
                {
                    animator.SetTrigger("StartAttack");
                    Debug.Log("[EnemyAI] 일반 콤보 공격 시작");
                }

                isAttacking = true;
                lastAttackTime = Time.time;
            }
        }
        else
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(dir.x * currentMoveSpeed, rb.linearVelocity.y);
            animator.SetBool("isMoving", true);

            if (dir.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1f : 1f);
                transform.localScale = scale;
            }
        }

        // 공격 중인데 3초 이상 지속되면 강제 종료
        if (isAttacking && Time.time - lastAttackTime > 3f)
        {
            EndAttack();
        }
    }

    void EnterBerserkMode()
    {
        isBerserk = true;
        berserkStartTime = Time.time;
        animator.SetBool("isBerserk", true);
        Debug.Log("[EnemyAI] 광분 상태 진입!");
    }

    public void OnAttackHit()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                int actualDamage = isBerserk ? Mathf.RoundToInt(enemy.damage * 1.5f) : enemy.damage;
                pc.TakeDamage(actualDamage);
                Debug.Log($"[EnemyAI] {(isBerserk ? "광분 공격!" : "일반 공격")} → 데미지: {actualDamage}");
            }
        }
    }

    public void ContinueCombo()
    {
        comboStep++;
        if (comboStep <= 2)
        {
            animator.SetInteger("comboStep", comboStep);
        }
        else
        {
            EndAttack();
        }
    }

    public void EndAttack()
    {
        comboStep = 0;
        animator.SetInteger("comboStep", 0);
        isAttacking = false;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || isHit) return;

        enemy.currentHealth -= dmg;
        Debug.Log($"{gameObject.name} 피격! 데미지: {dmg}, 남은 체력: {enemy.currentHealth}");

        if (!isBerserk)
        {
            animator.ResetTrigger("StartAttack");
            animator.ResetTrigger("RunAtk");
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Hit");
        }

        Vector2 knockbackDir = (transform.position - player.position).normalized;
        Vector2 knockback = new Vector2(knockbackDir.x * knockbackForce, 0f);

        StartCoroutine(HitStunEffect(knockback));

        if (enemy.currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitStunEffect(Vector2 knockback)
    {
        isHit = true;

        Color originalColor = sr.color;
        rb.linearVelocity = knockback;

        float blinkInterval = 0.1f;
        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);
            sr.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }

        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;
        isHit = false;
    }

  public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("EDie");

        // 움직임, 추격, 공격 전부 차단
        rb.linearVelocity = Vector2.zero;

        // 1.5초 뒤 파괴
        Destroy(gameObject, 1.5f);
    }

}
