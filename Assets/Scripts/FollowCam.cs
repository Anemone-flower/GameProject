using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (보통은 Player)
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 위치 오프셋
    public float smoothSpeed = 5f; // 따라가는 속도

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
