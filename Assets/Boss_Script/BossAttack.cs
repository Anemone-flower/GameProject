using UnityEngine;
using System.Collections;

public class BossAttack : MonoBehaviour
{
    private Animator anim;
    private BossController controller;
    private Transform player;

    private int comboStep = 0;
    private readonly int maxCombo = 2; // ATK1 → ATK2 콤보까지
    private Rigidbody2D rb;

    [Header("공격 설정")]
    public int comboDamage = 20;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<BossController>();
        player = controller.player;
        rb = GetComponent<Rigidbody2D>();
    }

    // 콤보 시작
    public void DoComboAttack(float stopDuration)
    {
        StartCoroutine(ComboRoutine(stopDuration));
    }

    IEnumerator ComboRoutine(float stopDuration)
    {
        controller.SetCanMove(false);
        controller.SetIsAttacking(true);
        StartCombo(); // 첫 타
        yield return new WaitForSeconds(stopDuration);
        controller.SetIsAttacking(false);
        controller.SetCanMove(true);
    }

    // 콤보 첫 공격 시작
    public void StartCombo()
    {
        if (comboStep == 0)
        {
            comboStep = 1;
            anim.SetInteger("Combo", comboStep);
            anim.SetTrigger("Attack");
        }
    }

    // 애니메이션 이벤트에서 호출됨: 다음 콤보 가능 시점
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

    // 콤보 종료
    public void EndCombo()
    {
        comboStep = 0;
        anim.SetTrigger("AttackEnd");
        anim.SetInteger("Combo", 0);
    }

    // 애니메이션 이벤트에서 타격 처리
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
                hit.GetComponent<PlayerStatusManager>()?.ApplyCorruption(10, 3f);
            }
        }
    }

    // 발검 준비 및 실행
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
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        yield return null;
    }

    // 발검 타격 실행
    public void ExecuteSwordStrikeAttack(GameObject effectPrefab)
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        Vector3 offset = Vector3.right * 5f * direction;
        Vector2 center = (Vector2)(transform.position + offset);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(10f, 10f), 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>()?.TakeDamage(50);
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

    // 반격
public void DoCounter(float knockbackForce = 10f, int damage = 10)
{
    anim.SetTrigger("DoCounter");
    Debug.Log("[반격] SetTrigger 호출됨");

    AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
    Debug.Log($"[Animator State] 현재 상태: {state.fullPathHash}, IsName(Counter)? {state.IsName("Counter")}");

    controller.SetCanMove(false);
    controller.LastAttackTime = Time.time;
    

    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj != null)
    {
        PlayerController pc = playerObj.GetComponent<PlayerController>();
        PlayerStatusManager status = playerObj.GetComponent<PlayerStatusManager>();
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();

        if (pc != null)
        {
            pc.TakeDamage(damage); // ✅ 데미지만 적용
        }

        if (playerRb != null)
        {
            Vector2 dir = (playerObj.transform.position - transform.position).normalized;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse); // ✅ 넉백
        }

        if (status != null)
        {
            status.TriggerCameraShakeAndZoom(); // ✅ 연출만
        }
    }

    StartCoroutine(RecoverMovementAfterDelay());
}


private IEnumerator RecoverMovementAfterDelay()
{
    yield return new WaitForSeconds(0.5f); // 스턴 시간
    controller.SetCanMove(true);
}

    // 사망
    public void DoDie()
    {
        anim.SetTrigger("Die");
    }
}
