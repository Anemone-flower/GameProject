using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    [Header("집중 모드")]
    public float focusDuration = 5f;
    public int maxFocusShots = 4;
    public int focusDamage = 30;
    public LayerMask enemyLayer;
    public float attackRange = 10f;

    [Header("카메라 연출")]
    public Camera mainCam;
    public float focusZoomSize = 2f;
    public float normalZoomSize = 5f;
    public float cameraShakeDuration = 0.15f;
    public float cameraShakeMagnitude = 1.1f;

    [Header("이펙트 설정")]
    public GameObject focusAttackEffectPrefab;
    public Vector2 effectOffset = new Vector2(1f, 0f);
    public float effectDuration = 0.5f;

    [Header("집념 스택")]
    public int maxStack = 4;
    private int _currentStack = 0;
    public int currentStack
    {
        get => _currentStack;
        set
        {
            _currentStack = Mathf.Clamp(value, 0, maxStack);
            ApplyStackEffects();
            UpdateStackUI();
            Debug.Log($"[집념] 스택 변경됨 → {_currentStack}");
        }
    }

    [Header("스택 동그라미 UI")]
    public Image[] stackCircles;
    public Color filledColor = Color.white;
    public Color emptyColor = Color.gray;

    private bool isFocusing = false;
    private float focusTimer = 0f;
    private int focusShotsRemaining = 0;
    private float shotCooldownTimer = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 originalCamPos;
    private bool isShaking = false;

    private PlayerController player;

    public bool IsFocusing => isFocusing;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        if (player == null)
            player = FindObjectOfType<PlayerController>(); // 백업 검색

        if (player == null)
            Debug.LogError("[PlayerSkills] PlayerController 연결 실패!");
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentStack = _currentStack;
        UpdateStackUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && !isFocusing && currentStack >= 2)
        {
            EnterFocusMode();
        }

        if (isFocusing)
        {
            focusTimer += Time.deltaTime;
            if (shotCooldownTimer > 0f)
                shotCooldownTimer -= Time.deltaTime;

            if (focusTimer >= focusDuration || currentStack <= 0)
            {
                ExitFocusMode();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExitFocusMode(true);
            }

            if (focusShotsRemaining > 0 && shotCooldownTimer <= 0f &&
                (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
            {
                int direction = (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) ? -1 : 1;
                ExecuteFocusShot(direction);
            }

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void EnterFocusMode()
    {
        isFocusing = true;
        focusTimer = 0f;
        focusShotsRemaining = Mathf.Min(maxFocusShots, currentStack);
        shotCooldownTimer = 0f;
        Debug.Log("집중 상태 진입");

        if (mainCam != null)
            mainCam.orthographicSize = focusZoomSize;

        animator?.SetTrigger("EnterFocus");
    }

    void ExitFocusMode(bool interrupted = false)
    {
        isFocusing = false;
        Debug.Log("집중 상태 종료");

        if (mainCam != null)
            mainCam.orthographicSize = normalZoomSize;

        animator?.SetTrigger("ExitFocus");
    }

void ExecuteFocusShot(int direction)
{
    focusShotsRemaining--;
    focusTimer = 0f;
    shotCooldownTimer = 1.5f;

    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

    Vector3 scale = transform.localScale;
    scale.x = Mathf.Abs(scale.x) * direction;
    transform.localScale = scale;

    FocusAttack(direction); // ✅ 먼저 공격

    currentStack--; // ✅ 그 다음 스택 차감 → ApplyStackEffects 자동 반영

    animator?.SetTrigger("FocusShoot");

    if (focusAttackEffectPrefab != null)
    {
        Vector3 spawnPos = transform.position + new Vector3(effectOffset.x * direction, effectOffset.y, 0f);
        GameObject effect = Instantiate(focusAttackEffectPrefab, spawnPos, Quaternion.identity);
        effect.transform.localScale = new Vector3(Mathf.Abs(effect.transform.localScale.x) * direction, effect.transform.localScale.y, 1f);
        Destroy(effect, effectDuration);
    }

    StartCoroutine(ReturnToHoldAfterShoot());

    if (mainCam != null)
        StartCoroutine(ShakeCamera());

    if (focusShotsRemaining <= 0 || currentStack <= 0)
        ExitFocusMode();
}

    IEnumerator ReturnToHoldAfterShoot()
    {
        yield return new WaitForSeconds(0.3f);
        if (isFocusing)
            animator?.SetTrigger("BackToHold");
    }

    void FocusAttack(int direction)
    {
        if (player == null)
        {
            return;
        }

        float multiplier = player.attackPowerMultiplier;
        int realDamage = Mathf.RoundToInt(focusDamage * multiplier);
        Debug.Log($"[집중] 공격 전 스택: {currentStack}, 배수: {multiplier}, 피해: {realDamage}");

        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(direction, 0f);
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, attackRange, enemyLayer);

        if (hit.collider != null && hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(realDamage);
            Debug.Log($"[집중] 명중! 대상: {hit.collider.name}");
        }
    }

    IEnumerator ShakeCamera()
    {
        if (isShaking || mainCam == null) yield break;

        isShaking = true;
        originalCamPos = mainCam.transform.position;

        float elapsed = 0f;
        while (elapsed < cameraShakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * cameraShakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * cameraShakeMagnitude;
            mainCam.transform.position = originalCamPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.position = originalCamPos;
        isShaking = false;
    }

    public void GainStack()
    {
        Debug.Log($"[집념] GainStack 호출됨 / 현재 player null? {player == null}");
        currentStack++;
        if (player != null)
            player.Heal(Mathf.RoundToInt(player.maxHP * 0.03f));
    }

    void ApplyStackEffects()
    {
        if (player == null)
        {
            Debug.LogError("[집념] ApplyStackEffects에서 player가 null입니다!");
            return;
        }

        player.attackSpeedMultiplier = 1f;
        player.damageReduction = 0f;
        player.attackPowerMultiplier = 1f;
        CancelInvoke(nameof(RegenerateHP));

        if (currentStack >= 1)
        {
            player.attackSpeedMultiplier = 1.2f;
            Debug.Log("[집념] 1스택 적용됨: 공격속도 +20%");
        }

        if (currentStack >= 2)
        {
            player.damageReduction = 0.10f;
            Debug.Log("[집념] 2스택 적용됨: 받는 피해 -10%");
        }

        if (currentStack >= 3)
        {
            InvokeRepeating(nameof(RegenerateHP), 1f, 1f);
            Debug.Log("[집념] 3스택 적용됨: 초당 체력 회복");
        }

        if (currentStack >= 4)
        {
            player.attackPowerMultiplier = 2f;
            Debug.Log("[집념] 4스택 적용됨: 공격력 +100%");
        }
    }

    void RegenerateHP()
    {
        if (player != null)
            player.Heal(Mathf.RoundToInt(player.maxHP * 0.01f));
    }

    private void UpdateStackUI()
    {
        for (int i = 0; i < stackCircles.Length; i++)
        {
            stackCircles[i].color = i < currentStack ? filledColor : emptyColor;
        }
    }
}
