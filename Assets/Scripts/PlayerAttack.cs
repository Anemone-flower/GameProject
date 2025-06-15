using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private bool isAttacking = false;
    private bool canReceiveInput = false;
    private bool inputReceived = false;
    private int comboStep = 0;
    private readonly int maxCombo = 3;

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

    // === 아래는 애니메이션 이벤트에서 호출 ===

    public void EnableComboInput()
    {
        canReceiveInput = true;
    }

    public void DisableComboInput()
    {
        canReceiveInput = false;
    }

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
}
