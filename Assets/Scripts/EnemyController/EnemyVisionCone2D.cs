using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnemyVisionCone2D : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public int rayCount = 40;

    [Header("References")]
    public Transform target;         // Player or patrol target
    public LayerMask obstacleMask;   // Wall/obstacle layer

    private Mesh visionMesh;
    private MeshFilter meshFilter;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        visionMesh = new Mesh();
        visionMesh.name = "Vision Cone Mesh";
        meshFilter.mesh = visionMesh;
    }

    private void Update()
    {
        if (transform == null) return;

        // 1. Align cone with enemy and (optionally) target
        UpdateConeTransform();

        // 2. Rebuild mesh based on obstacles
        DrawMesh();

        // 3. Check sight
        if (IsTargetInSight())
            Debug.Log("Target in sight!");
    }

    void UpdateConeTransform()
    {
        Vector2 dir = transform.right;
        if (target != null)
            dir = ((Vector2)target.position - (Vector2)transform.position).normalized;

        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.position = transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
    }

    void DrawMesh()
    {
        // Local-space mesh
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // center

        float halfAngle = viewAngle * 0.5f;
        float step = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -halfAngle + step * i;
            // Local direction
            Vector3 localDir = new Vector3(
                Mathf.Cos(Mathf.Deg2Rad * currentAngle),
                Mathf.Sin(Mathf.Deg2Rad * currentAngle),
                0f
            );

            // World direction: rotate by cone's transform
            Vector3 worldDir = transform.TransformDirection(localDir);

            // Raycast in world space
            Vector2 origin = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, worldDir, viewRadius, obstacleMask);
            float dist = hit.collider ? hit.distance : viewRadius;

            // Vertex stays in local space
            vertices[i + 1] = localDir * dist;

            // Triangles
            if (i < rayCount)
            {
                int ti = i * 3;
                triangles[ti + 0] = 0;
                triangles[ti + 1] = i + 1;
                triangles[ti + 2] = i + 2;
            }
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }

    public bool IsTargetInSight()
    {
        if (target == null) return false;

        Vector2 origin = transform.position;
        Vector2 toTarget = (Vector2)target.position - origin;
        float distance = toTarget.magnitude;
        if (distance > viewRadius) return false;

        // Angle check
        Vector2 facing = transform.right;
        float angle = Vector2.Angle(facing, toTarget.normalized);
        if (angle > viewAngle * 0.5f) return false;

        // Obstacle check
        RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, distance, obstacleMask);
        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        if (transform == null) return;

        Vector2 origin = transform.position;
        Vector2 facing = transform.right;
        float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        float halfAngle = viewAngle * 0.5f;
        float step = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float a = baseAngle - halfAngle + step * i;
            Vector3 dir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * a), Mathf.Sin(Mathf.Deg2Rad * a), 0f);
            Gizmos.color = IsTargetInSight() ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + (Vector2)dir * viewRadius);
        }
    }
}
