using System;
using System.Collections;
using UnityEngine;

public class MomController : MonoBehaviour
{
    [Header("Vision")]
    public VisionCone vision;         // the cone on the child object (VisionOrigin)

    [Header("Path")]
    public Transform entryPoint;
    public Transform standPoint;
    public float walkSpeed = 3f;

    [Header("Scan angles (Z rotation, 0 = up, counterclockwise)")]
    public float centerAngle = 0f;
    public float rightAngle = -60f;   // screen-right when she faces up
    public float leftAngle = 60f;     // screen-left when she faces up

    [Header("Timing")]
    public float startDelay = 1f;         // wait after the timer ends, before she walks in
    public float arriveDelay = 0.5f;      // pause once she's in place, cone still off
    public float lookCenterTime = 0.8f;   // pause looking at the middle
    public float toRightDuration = 1.5f;  // center -> right
    public float pauseAtSide = 0.4f;
    public float toLeftDuration = 3f;     // right -> left (the long sweep)
    public float pauseAtEnd = 0.5f;

    public event Action Spotted;
    public event Action ScanFinished;
    public Transform CaughtTarget { get; private set; }

    Transform cone;                   // the object that actually rotates
    bool caught;

    void Awake()
    {
        cone = vision.transform;
        vision.active = false;
        if (entryPoint) transform.position = entryPoint.position;
        cone.rotation = Quaternion.Euler(0, 0, centerAngle);
    }

    public void StartScan() => StartCoroutine(ScanRoutine());

    IEnumerator ScanRoutine()
    {
        // 0. short beat before she appears
        yield return new WaitForSeconds(startDelay);

        // 1. walk in
        while (Vector2.Distance(transform.position, standPoint.position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, standPoint.position, walkSpeed * Time.deltaTime);
            yield return null;
        }

        // 1b. pause after arriving, cone still off
        yield return new WaitForSeconds(arriveDelay);

        // 2. look at the middle first
        vision.active = true;
        yield return Hold(lookCenterTime);
        if (caught) { Spotted?.Invoke(); yield break; }

        // 3. center -> right
        yield return RotateTo(centerAngle, rightAngle, toRightDuration);
        if (caught) { Spotted?.Invoke(); yield break; }
        yield return Hold(pauseAtSide);
        if (caught) { Spotted?.Invoke(); yield break; }

        // 4. right -> left
        yield return RotateTo(rightAngle, leftAngle, toLeftDuration);
        if (caught) { Spotted?.Invoke(); yield break; }

        yield return Hold(pauseAtEnd);
        if (caught) Spotted?.Invoke();
        else ScanFinished?.Invoke();
    }

    IEnumerator RotateTo(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, t / duration));
            cone.rotation = Quaternion.Euler(0, 0, a);
            if (CheckCaught()) yield break;
            yield return null;
        }
        cone.rotation = Quaternion.Euler(0, 0, to);
    }

    IEnumerator Hold(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (CheckCaught()) yield break;
            yield return null;
        }
    }

    bool CheckCaught()
    {
        if (!vision.CanSeeTarget) return false;
        caught = true;
        CaughtTarget = vision.SeenTarget ? vision.SeenTarget.transform : null;
        return true;
    }
}