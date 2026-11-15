
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Collider2D stageBounds;

    Camera cachedCamera;
    
    public float zoomSpeed = 2f;
    float targetSize;
    
    public void SetTarget(Transform newTarget) => target = newTarget;
    public void ZoomTo(float size) => targetSize = size;
    
    Vector3 shakeOffset;
    Vector3 appliedShake;
    Coroutine shakeRoutine;

    void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        targetSize = cachedCamera.orthographicSize;
    }

    void LateUpdate()
    {
        transform.position -= appliedShake;

        cachedCamera.orthographicSize = Mathf.Lerp(
            cachedCamera.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);

        if (target != null)
        {
            Vector3 targetPosition = new Vector3(
                target.position.x, target.position.y, transform.position.z);

            if (stageBounds != null)
                targetPosition = ClampToStage(targetPosition);

            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            transform.position = stageBounds != null
                ? ClampToStage(smoothedPosition)
                : smoothedPosition;
        }

        appliedShake = shakeOffset;
        transform.position += appliedShake;
    }

    public void SnapZoom(float size)
    {
        targetSize = size;
        cachedCamera.orthographicSize = size;
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            Vector2 r = Random.insideUnitCircle * magnitude * (1f - t / duration);
            shakeOffset = new Vector3(r.x, r.y, 0f);
            yield return null;
        }
        shakeOffset = Vector3.zero;
        shakeRoutine = null;
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
