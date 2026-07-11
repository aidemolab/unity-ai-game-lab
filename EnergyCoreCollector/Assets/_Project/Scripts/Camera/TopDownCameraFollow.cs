using UnityEngine;

public sealed class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 12f, -10f);
    [SerializeField, Min(0f)] private float followSmoothTime = 0.18f;
    [SerializeField] private Vector3 lookAtTargetOffset = new Vector3(0f, 1f, 0f);

    private Vector3 followVelocity;

    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }

    public Vector3 CameraOffset
    {
        get { return cameraOffset; }
        set { cameraOffset = value; }
    }

    public float FollowSmoothTime
    {
        get { return followSmoothTime; }
        set { followSmoothTime = Mathf.Max(0f, value); }
    }

    public Vector3 LookAtTargetOffset
    {
        get { return lookAtTargetOffset; }
        set { lookAtTargetOffset = value; }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + cameraOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, followSmoothTime);

        Vector3 lookAtPosition = target.position + lookAtTargetOffset;
        Vector3 lookDirection = lookAtPosition - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }
    }

    private void OnValidate()
    {
        followSmoothTime = Mathf.Max(0f, followSmoothTime);
    }
}
