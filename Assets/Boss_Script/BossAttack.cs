using UnityEngine;
using System.Collections;

public class BossAttack : MonoBehaviour
{
    private Animator anim;
    private BossController controller;
    private Transform player;

    private int comboStep = 0;
    private readonly int maxCombo = 2;
    private Rigidbody2D rb;

    [Header("공격 설정")]
    public int comboDamage = 20;

    // ✅ 여명 쿨타임 변수
    private float lastDawnTime = -999f;
    private float dawnInterval = 10f;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<BossController>();
        player = controller.player;
        rb = GetComponent<Rigidbody2D>();
    }

    public void DoComboAttack(float stopDuration)
    {
        StartCoroutine(ComboRoutine(stopDuration));
    }

    IEnumerator ComboRoutine(float stopDuration)
    {
        controller.SetCanMove(false);
        controller.SetIsAttacking(true);
        StartCombo();
        yield return new WaitForSeconds(stopDuration);
        controller.SetIsAttacking(false);
        controller.SetCanMove(true);
    }

    public void StartCombo()
    {
        if (comboStep == 0)
        {
            comboStep = 1;
            anim.SetInteger("Combo", comboStep);
            anim.SetTrigger("Attack");
        }
    }

    public void ContinueCombo()
    {
        if (comboStep < maxCombo)
        {
            comboStep++;
            anim.SetInteger("Combo", comboStep);
            anim.SetTrigger("Attack");
        }
        else
        {
            EndCombo();
        }
    }

    public void EndCombo()
    {
        comboStep = 0;
        anim.SetTrigger("AttackEnd");
        anim.SetInteger("Combo", 0);
    }

    public void ComboDamageEvent()
    {
        if (player == null) return;

        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        Vector3 attackOffset = Vector3.right * controller.normalAttackRange * direction;
        Vector2 attackCenter = (Vector2)(transform.position + attackOffset);
        Vector2 attackSize = controller.normalAttackBoxSize;

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>()?.TakeDamage(comboDamage);

                // 10초마다 1회 여명 부여
                if (Time.time - lastDawnTime >= dawnInterval)
                {
                    hit.GetComponent<PlayerStatusManager>()?.ApplyDawn(15, 3f);
                    lastDawnTime = Time.time;
                }
            }
        }
    }

    public void DoSwordStrike(float duration)
    {
        StartCoroutine(SwordStrikeRoutine(duration));
    }

    IEnumerator SwordStrikeRoutine(float duration)
    {
        controller.SetIsSwordStriked(true);
        controller.SetSwordStrikeTimer(duration);

        anim.SetTrigger("Ready");
        controller.SetIsAttacking(true);
        transform.position = player.position;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        yield return null;
    }

    public void ExecuteSwordStrikeAttack(GameObject effectPrefab)
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        Vector3 offset = Vector3.right * 5f * direction;
        Vector2 center = (Vector2)(transform.position + offset);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(15f, 15f), 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>()?.TakeDamage(120);
                hit.GetComponent<PlayerStatusManager>()?.ApplyDawn(15, 3f);
            }
        }

        anim.SetTrigger("Fire");
        controller.SetIsAttacking(false);

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position + offset, Quaternion.identity);
            Destroy(effect, 2f);
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void DoCounter(float knockbackForce = 10f)
    {
        anim.SetTrigger("DoCounter");
        Debug.Log("[반격] SetTrigger 호출됨");

        controller.SetCanMove(false);
        controller.SetIsAttacking(true);
        controller.LastAttackTime = Time.time;

        // ✅ 3% 회복
        int healAmount = Mathf.RoundToInt(controller.maxHealth * 0.03f);
        controller.Heal(healAmount);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            PlayerStatusManager status = playerObj.GetComponent<PlayerStatusManager>();
            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();

            if (pc != null)
            {
                Vector2 dir = ((Vector2)(playerObj.transform.position - transform.position)).normalized + Vector2.up * 0.5f;
                pc.ApplyKnockback(dir * knockbackForce);
            }

            if (status != null)
                status.TriggerCameraShakeAndZoom();
        }

        // ✅ 즉시 납도
        controller.LastSwordStrikeTime = -999f;
        DoSwordStrike(controller.swordStrikeDuration);

        StartCoroutine(RecoverMovementAfterDelay());
    }

    private IEnumerator RecoverMovementAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        controller.SetCanMove(true);
        controller.SetIsAttacking(false);
    }

    public void DoDie()
    {
        anim.SetTrigger("Die");
    }
}
