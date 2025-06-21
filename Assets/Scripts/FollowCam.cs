using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 카메라 오프셋
    public float smoothTime = 0.1f; // 작을수록 더 빠르게 따라감

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
