using UnityEngine;
using System.Collections;

public class PlayerStatusManager : MonoBehaviour
{
    private PlayerController player;
    private Rigidbody2D rb;

    [Header("카메라 연출")]
    public Camera mainCam;
    public float zoomInSize = 3f;
    public float zoomDuration = 0.5f;
    private Vector3 originalCamPos;
    private float originalZoom;
    private bool isZooming = false;
    private Coroutine cameraRoutine = null;

    // ✅ 여명 디버프 관련 변수
    private float lastDawnApplyTime = -999f;
    private float dawnCooldown = 10f;
    private int dawnStacks = 0;
    private Coroutine dawnCoroutine = null;
    private readonly int maxDawnStacks = 3;
    private Color dawnColor = new Color(1f, 0.75f, 0.2f); // 태양색

    void Start()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TriggerCameraShakeAndZoom()
    {
        if (mainCam == null || isZooming) return;

        if (cameraRoutine != null)
            StopCoroutine(cameraRoutine);

        cameraRoutine = StartCoroutine(CameraZoomAndShake());
    }

    private IEnumerator CameraZoomAndShake()
    {
        isZooming = true;
        originalCamPos = mainCam.transform.position;
        originalZoom = mainCam.orthographicSize;

        mainCam.orthographicSize = zoomInSize;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            mainCam.transform.position = originalCamPos + (Vector3)(Random.insideUnitCircle * 0.1f);
            yield return null;
        }

        mainCam.orthographicSize = originalZoom;
        mainCam.transform.position = originalCamPos;
        isZooming = false;
        cameraRoutine = null;
    }

    // ✅ [여명] 디버프 적용
    public void ApplyDawn(int baseDamage = 10, float baseDuration = 3f)
    {
        if (dawnStacks >= maxDawnStacks)
        {
            if (Time.time - lastDawnApplyTime < dawnCooldown)
            {
                Debug.Log("[여명] 최대 스택 & 쿨타임 중");
                return;
            }

            Debug.Log("[여명] 최대 스택 쿨타임 끝 - 디버프 재적용 가능");
            dawnStacks = 0;
        }

        dawnStacks++;
        lastDawnApplyTime = (dawnStacks >= maxDawnStacks) ? Time.time : lastDawnApplyTime;

        int totalDamage = baseDamage * dawnStacks;
        float totalDuration = baseDuration * dawnStacks;

        if (dawnCoroutine != null)
            StopCoroutine(dawnCoroutine);

        dawnCoroutine = StartCoroutine(DawnDOT(totalDamage, totalDuration));
        Debug.Log($"[여명] 적용됨 - 스택 {dawnStacks}, 데미지 {totalDamage}, 지속시간 {totalDuration}초");

        if (player != null)
            player.SetHPBarColor(dawnColor);
    }

    private IEnumerator DawnDOT(int damage, float duration)
    {
        float tickInterval = 1f;
        int tickCount = Mathf.FloorToInt(duration / tickInterval);

        for (int i = 0; i < tickCount; i++)
        {
            ApplyDotDamage(damage);
            yield return new WaitForSeconds(tickInterval);
        }

        dawnStacks = 0;
        dawnCoroutine = null;

        if (player != null)
            player.ResetHPBarColor(); // ✅ 디버프 종료 시 색상 복구
    }

    public void ApplyDotDamage(int damage)
    {
        if (player.IsDead) return;

        Debug.Log($"[여명 DOT] {damage} 피해 적용 (남은 체력: {player.hpSlider.value - damage})");

        player.TakeDamage(damage); // TakeDamage 내부에서 clamp 처리
    }
}
