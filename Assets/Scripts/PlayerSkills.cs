using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    [Header("집중 모드")]
    public float focusDuration = 5f;
    public int maxFocusShots = 3;
    public int focusDamage = 30;
    public LayerMask enemyLayer;
    public float attackRange = 5f;

    [Header("카메라 연출")]
    public Camera mainCam;
    public float focusZoomSize = 2f;
    public float normalZoomSize = 5f;
    public float cameraShakeDuration = 0.1f;
    public float cameraShakeMagnitude = 0.3f;

    [Header("이펙트 설정")]
    public GameObject focusAttackEffectPrefab;
    public Vector2 effectOffset = new Vector2(1f, 0f);
    public float effectDuration = 0.5f;

    [Header("집념 스택")]
    public int maxStack = 5;
    public int currentStack = 0;
    public Slider stackSlider;

    private bool isFocusing = false;
    private float focusTimer = 0f;
    private int focusShotsRemaining = 0;
    private float shotCooldownTimer = 0f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector3 originalCamPos;
    private bool isShaking = false;

    public bool IsFocusing { get { return isFocusing; } }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        {
            mainCam.orthographicSize = focusZoomSize;
        }

        animator?.SetTrigger("EnterFocus");
    }

    void ExitFocusMode(bool interrupted = false)
    {
        isFocusing = false;
        Debug.Log("집중 상태 종료");

        if (mainCam != null)
        {
            mainCam.orthographicSize = normalZoomSize;
        }

        animator?.SetTrigger("ExitFocus");
    }

    void ExecuteFocusShot(int direction)
    {
        focusShotsRemaining--;
        focusTimer = 0f;
        shotCooldownTimer = 1.5f;

        currentStack = Mathf.Max(0, currentStack - 1);
        UpdateStackUI();

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;

        FocusAttack(direction);

        animator?.SetTrigger("FocusShoot");

        if (focusAttackEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(effectOffset.x * direction, effectOffset.y, 0f);
            GameObject effect = Instantiate(focusAttackEffectPrefab, spawnPos, Quaternion.identity);

            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * direction;
            effect.transform.localScale = effectScale;

            Destroy(effect, effectDuration);
        }

        StartCoroutine(ReturnToHoldAfterShoot());

        if (mainCam != null)
        {
            StartCoroutine(ShakeCamera());
        }

        if (focusShotsRemaining <= 0 || currentStack <= 0)
        {
            ExitFocusMode();
        }
    }

    IEnumerator ReturnToHoldAfterShoot()
    {
        yield return new WaitForSeconds(0.3f);
        if (isFocusing)
        {
            animator?.SetTrigger("BackToHold");
        }
    }

    void FocusAttack(int direction)
    {
        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(direction, 0f);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, attackRange, enemyLayer);
        if (hit.collider != null)
        {
            hit.collider.GetComponent<Enemy>()?.TakeDamage(focusDamage);
            Debug.Log("집중 공격 적 명중: " + hit.collider.name);
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
        currentStack = Mathf.Clamp(currentStack + 1, 0, maxStack);
        UpdateStackUI();
        Debug.Log($"[집념] 스택 획득: {currentStack}");
    }

    private void UpdateStackUI()
    {
        if (stackSlider != null)
        {
            stackSlider.maxValue = maxStack;
            stackSlider.value = currentStack;
        }
    }
}
