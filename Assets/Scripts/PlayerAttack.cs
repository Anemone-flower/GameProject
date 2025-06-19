using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private bool isAttacking = false;
    private bool canReceiveInput = false;
    private bool inputReceived = false;
    private int comboStep = 0;
    private readonly int maxCombo = 3;

    [Header("공격 설정")]
    public Vector2 boxSize = new Vector2(1.5f, 1f); // 가로, 세로 크기
    public float boxDistance = 1f;
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public int attackDamage = 10;

    [Header("이펙트 설정")]
    public GameObject attackEffectPrefab;
    public Vector2 effectOffset = new Vector2(1f, 0f); // 공격 이펙트 위치 조정

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (!isAttacking)
            {
                isAttacking = true;
                comboStep = 1;
                animator.SetInteger("attackCount", comboStep);
                animator.SetTrigger("meleeAttack");
            }
            else if (canReceiveInput)
            {
                inputReceived = true;
            }
        }
    }

    public void EnableComboInput() => canReceiveInput = true;
    public void DisableComboInput() => canReceiveInput = false;

    public void ContinueCombo()
    {
        if (inputReceived && comboStep < maxCombo)
        {
            inputReceived = false;
            comboStep++;
            animator.SetInteger("attackCount", comboStep);
            animator.SetTrigger("meleeAttack");
        }
        else
        {
            EndCombo();
        }
    }

    public void EndCombo()
    {
        isAttacking = false;
        inputReceived = false;
        canReceiveInput = false;
        comboStep = 0;
        animator.SetTrigger("meleeAttackEnd");
    }

    public void PerformAttack()
    {
        if (attackPoint == null) return;

        float facing = Mathf.Sign(transform.localScale.x); // ← 방향 계산 보정
        Vector2 center = (Vector2)attackPoint.position + new Vector2(facing * boxDistance, 0f);
        float angle = facing > 0 ? 0f : 180f;

        // === 히트박스 판정 ===
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, boxSize, angle, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("적 타격: " + enemy.name);
            enemy.GetComponent<Enemy>()?.TakeDamage(attackDamage);
        }

        // === 3번째 콤보에만 이펙트 생성 ===
        if (comboStep == 3 && attackEffectPrefab != null)
        {
            Vector2 effectPos = (Vector2)attackPoint.position + new Vector2(facing * effectOffset.x, effectOffset.y);
            GameObject effect = Instantiate(attackEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        float facing = Application.isPlaying && transform.localScale.x < 0 ? -1f : 1f;
        Vector2 center = (Vector2)attackPoint.position + new Vector2(facing * boxDistance, 0f);
        float angle = facing > 0 ? 0f : 180f;

        Gizmos.color = Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
