using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VisionCone : MonoBehaviour
{
    [Header("Vision")]
    public float viewDistance = 5f;
    [Range(1, 360)] public float viewAngle = 70f;
    public LayerMask targetMask;     // what the NPC can notice (Player, Crate)
    public LayerMask obstacleMask;   // what blocks sight (Wall)

    [Header("Movement")]
    public float sweepSpeed = 30f;   // degrees/sec, set 0 for a fixed NPC

    [Header("Indicator")]
    public GameObject alertIndicator;
    public Color idleColor = new Color(1f, 0.9f, 0.2f, 0.9f);
    public Color alertColor = new Color(1f, 0.2f, 0.2f, 0.9f);

    public bool CanSeeTarget { get; private set; }

    LineRenderer line;
    const int arcSegments = 24;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;       // cone rotates with the NPC
        line.loop = true;
        line.widthMultiplier = 0.06f;
        line.sortingOrder = 5;
        line.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        if (sweepSpeed != 0f)
            transform.Rotate(0f, 0f, sweepSpeed * Time.deltaTime);

        CanSeeTarget = ScanForTargets();

        Color c = CanSeeTarget ? alertColor : idleColor;
        line.startColor = c;
        line.endColor = c;
        if (alertIndicator) alertIndicator.SetActive(CanSeeTarget);

        DrawCone();
    }

    bool ScanForTargets()
    {
        Vector2 origin = transform.position;
        Vector2 forward = transform.up;   // the NPC "faces" its local up axis

        foreach (Collider2D col in Physics2D.OverlapCircleAll(origin, viewDistance, targetMask))
        {
            Vector2 toTarget = (Vector2)col.bounds.center - origin;

            // 1. inside the cone angle?
            if (Vector2.Angle(forward, toTarget) > viewAngle * 0.5f) continue;

            // 2. is a wall in the way?
            if (Physics2D.Raycast(origin, toTarget.normalized, toTarget.magnitude, obstacleMask))
                continue;

            return true;
        }
        return false;
    }

    void DrawCone()
    {
        // outline: origin -> arc -> back to origin
        line.positionCount = arcSegments + 2;
        line.SetPosition(0, Vector3.zero);
        for (int i = 0; i <= arcSegments; i++)
        {
            float a = Mathf.Lerp(-viewAngle * 0.5f, viewAngle * 0.5f, i / (float)arcSegments);
            line.SetPosition(i + 1, Quaternion.Euler(0, 0, a) * Vector3.up * viewDistance);
        }
    }
}