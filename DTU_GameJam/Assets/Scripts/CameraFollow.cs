
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Collider2D stageBounds;

    Camera cachedCamera;

    void Awake()
    {
        cachedCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        if (stageBounds != null)
            targetPosition = ClampToStage(targetPosition);

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = stageBounds != null
            ? ClampToStage(smoothedPosition)
            : smoothedPosition;
    }

    Vector3 ClampToStage(Vector3 position)
    {
        Bounds bounds = stageBounds.bounds;
        float halfHeight = cachedCamera.orthographicSize;
        float halfWidth = halfHeight * cachedCamera.aspect;

        position.x = ClampToViewport(
            bounds.min.x + halfWidth,
            bounds.max.x - halfWidth,
            bounds.center.x,
            position.x
        );
        position.y = ClampToViewport(
            bounds.min.y + halfHeight,
            bounds.max.y - halfHeight,
            bounds.center.y,
            position.y
        );

        return position;
    }

    static float ClampToViewport(float min, float max, float center, float value)
    {
        return min > max ? center : Mathf.Clamp(value, min, max);
    }
}
