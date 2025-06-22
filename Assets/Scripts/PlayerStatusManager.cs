using UnityEngine;
using System.Collections;

public class PlayerStatusManager : MonoBehaviour
{
    private PlayerController player;
    private Rigidbody2D rb;

    private bool isCorrupted = false;
    private Coroutine corruptionCoroutine;

    [Header("카메라 연출")]
    public Camera mainCam;
    public float zoomInSize = 3f;
    public float zoomDuration = 0.5f;
    public float shakeMagnitude = 0.5f;

    private Vector3 originalCamPos;
    private float originalZoom;
    private bool isZooming = false;
    private Coroutine cameraRoutine;

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
            float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
            float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);
            mainCam.transform.position = originalCamPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.position = originalCamPos;
        mainCam.orthographicSize = originalZoom;
        isZooming = false;
    }

    // ✅ 잠식 상태이상 적용
    public void ApplyCorruption(int totalDamage, float duration)
    {
        if (isCorrupted)
        {
            Debug.Log("[잠식] 이미 적용 중입니다. 중복 적용 방지.");
            return;
        }

        corruptionCoroutine = StartCoroutine(CorruptionRoutine(totalDamage, duration));
    }

    private IEnumerator CorruptionRoutine(int totalDamage, float duration)
    {
        Debug.Log("[잠식] 상태이상 시작됨");
        isCorrupted = true;

        player.SetHPBarColor(new Color(0.6f, 0.2f, 0.8f)); // 보라색 표시

        int ticks = Mathf.CeilToInt(duration);
        int damagePerTick = Mathf.Max(1, Mathf.RoundToInt((float)totalDamage / ticks));

        for (int i = 0; i < ticks; i++)
        {
            if (player == null || player.IsDead || !enabled)
            {
                Debug.LogWarning($"[잠식] {i + 1}/{ticks}번째 틱 → 데미지 스킵됨 (플레이어 상태 이상)");
                yield return new WaitForSeconds(1f);
                continue;
            }

            Debug.Log($"[잠식] {i + 1}/{ticks}번째 틱 → {damagePerTick} 피해 적용");
            player.ApplyDotDamage(damagePerTick);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("[잠식] 상태이상 종료됨");

        isCorrupted = false;
        corruptionCoroutine = null;
        player.ResetHPBarColor();
    }

    // ✅ 반격 전용 넉백
    public void ApplyCounterKnockback(Vector2 direction, float force, int damage)
    {
        Debug.Log("[반격] 데미지 + 넉백 처리");
        player.TakeDamage(damage);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    void OnDisable()
    {
        // 끊어진 상태 정리
        if (corruptionCoroutine != null)
        {
            StopCoroutine(corruptionCoroutine);
            corruptionCoroutine = null;
        }
        isCorrupted = false;
        player?.ResetHPBarColor();
        Debug.Log("[잠식] OnDisable → 상태 초기화됨");
    }
}
