using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class BossController : MonoBehaviour, IDamageable
{
    [Header("보스 기본 설정")]
    public int maxHealth = 1000;
    private int currentHealth;
    public float moveSpeed = 2f;
    private bool isPhase2 = false;

    [Header("상태")]
    private bool isSwordStriked = false;
    private float swordStrikeTimer = 0f;
    private float shieldAmount = 0f;
    private bool canMove = true;
    private bool isDead = false;

    [Header("공격 관련")]
    public Transform player;
    public float attackInterval = 3f;
    private float lastAttackTime;

    [Header("발검 관련")]
    public float teleportDistance = 10f;
    public float swordStrikeDuration = 3f;
    public GameObject swordStrikeEffect;
    private float swordStrikeCooldown = 20f;
    private float lastSwordStrikeTime = -999f;

    [Header("실드 이펙트")]
    public GameObject shieldEffect;

    [Header("UI 연동")]
    public BossUIManager bossUI;
    public string bossName = "칠흑의 거신";

    [Header("사망 연출")]
    public CanvasGroup fadeCanvasGroup;       // ✅ end 오브젝트 (CanvasGroup)
    public TMP_Text deathDialogueText;
    public string clearSceneName = "Clear";

    [Header("일반 공격 범위 시각화")]
    public float normalAttackRange = 1.5f;
    public Vector2 normalAttackBoxSize = new Vector2(2f, 2f);
    public bool showAttackRange = true;

    [Header("반격 설정")]
    [Range(0f, 1f)] public float counterChance = 0.4f;
    private float lastCounterTime = -999f;
    private float counterCooldown = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private BossAttack bossAttack;
    private bool isAttacking = false;

    void Awake()
    {
        // ✅ 시작 시 사망 UI 숨기기
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        if (deathDialogueText != null)
            deathDialogueText.text = "";
    }

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bossAttack = GetComponent<BossAttack>();

        bossUI?.SetupUI(maxHealth, bossName);
        lastAttackTime = Time.time;
    }

    public void SetIsAttacking(bool value) => isAttacking = value;
    public void SetCanMove(bool value) => canMove = value;
    public void SetIsSwordStriked(bool value) => isSwordStriked = value;
    public void SetSwordStrikeTimer(float time) => swordStrikeTimer = time;
    public float LastSwordStrikeTime { get => lastSwordStrikeTime; set => lastSwordStrikeTime = value; }
    public float LastAttackTime { get => lastAttackTime; set => lastAttackTime = value; }

    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (canMove && !isSwordStriked)
        {
            if (distance > 1.5f)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
                anim.SetBool("isWalking", true);

                if (dir.x != 0)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
                    transform.localScale = scale;
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);

                if (Time.time - lastAttackTime >= attackInterval)
                {
                    lastAttackTime = Time.time;
                    bossAttack.DoComboAttack(3f);
                }
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
        }

        if (!isSwordStriked &&
            Time.time - lastSwordStrikeTime >= swordStrikeCooldown &&
            distance >= teleportDistance)
        {
            lastSwordStrikeTime = Time.time;
            bossAttack.DoSwordStrike(swordStrikeDuration);
        }

        if (!isPhase2 && currentHealth <= maxHealth / 2)
        {
            EnterPhase2();
        }

        if (isSwordStriked)
        {
            swordStrikeTimer -= Time.deltaTime;
            if (swordStrikeTimer <= 0f)
            {
                isSwordStriked = false;
                bossAttack.ExecuteSwordStrikeAttack(swordStrikeEffect);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        bool canCounter = Time.time - lastCounterTime >= counterCooldown;
        bool isLucky = Random.value <= counterChance;

        if (canCounter && isLucky)
        {
            lastCounterTime = Time.time;
            bossAttack.DoCounter();
        }

        float finalDamage = isSwordStriked ? damage * 2f : damage;

        if (shieldAmount > 0)
        {
            float absorbed = Mathf.Min(shieldAmount, finalDamage);
            shieldAmount -= absorbed;
            finalDamage -= absorbed;
        }

        int intFinalDamage = Mathf.RoundToInt(finalDamage);
        currentHealth -= intFinalDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        bossUI?.UpdateHP(currentHealth, shieldAmount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyShieldByRatio(float ratio)
    {
        float amount = maxHealth * ratio;
        shieldAmount += amount;

        if (shieldEffect != null)
            Instantiate(shieldEffect, transform.position, Quaternion.identity);

        bossUI?.UpdateHP(currentHealth, shieldAmount);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        bossUI?.UpdateHP(currentHealth, shieldAmount);
        Debug.Log($"[보스] HP 회복: {amount} → 현재 HP: {currentHealth}");
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        moveSpeed *= 1.5f;
        Debug.Log("보스 2페이즈 진입");
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        bossAttack.DoDie();
        bossUI?.HideUI();
        Debug.Log("보스 사망");

        StartCoroutine(BossDeathSequence());
    }

    private IEnumerator BossDeathSequence()
    {
        float duration = 1.5f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }

        string[] lines = new string[]
        {
            "여명이..도래하기엔..아직..이르군",
            "나에게..밤은..너무나도..길구나.."
        };

        foreach (var line in lines)
        {
            deathDialogueText.text = "";
            yield return StartCoroutine(TypeSentence(line, 0.04f));
            yield return new WaitForSeconds(1.5f);
        }

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(clearSceneName);
    }

    private IEnumerator TypeSentence(string sentence, float speed)
    {
        foreach (char c in sentence)
        {
            deathDialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }
    }

    void OnDrawGizmosSelected()
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;

        Gizmos.color = Color.red;
        Vector3 swordOffset = Vector3.right * 5f * direction;
        Vector2 swordCenter = (Vector2)(transform.position + swordOffset);
        Gizmos.DrawWireCube(swordCenter, new Vector3(10f, 10f, 0f));

        if (showAttackRange)
        {
            Gizmos.color = Color.yellow;
            Vector3 attackOffset = Vector3.right * normalAttackRange * direction;
            Vector3 attackCenter = transform.position + attackOffset;
            Gizmos.DrawWireCube(attackCenter, normalAttackBoxSize);
        }
    }
}
