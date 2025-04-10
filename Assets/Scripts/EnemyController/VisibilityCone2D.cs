using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisibilityCone2D : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public int edgeRayCount = 20;
    [Header("Graphic Settings")]
    [SerializeField]
    Color coneColor = new Color(1f, 1f, 0f, 0.4f);

    [Header("References")]
    public LayerMask obstacleMask;
    [Tooltip("Source of Vision")]
    public Transform enemyTransform;
    public Transform target;         

    private Mesh mesh;
    private MeshFilter mf;
    private MeshRenderer mr;

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mesh = new Mesh { name = "Visibility Mesh" };
        mf.mesh = mesh;
        mr = GetComponent<MeshRenderer>();
        mr.material.color = coneColor; 
    }

    void LateUpdate()
    {
        if (enemyTransform == null) return;
        AimCone();
        BuildVisibilityMesh();
    }

    void AimCone()
    {      
        Vector2 dir = (target != null)
            ? ((Vector2)target.position - (Vector2)enemyTransform.position).normalized
            : enemyTransform.right;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.position = enemyTransform.position;
        transform.rotation = Quaternion.Euler(0, 0, ang);   

    }

    private void FixedUpdate()
    {
        CheckTargetVisibility();
    }

    void BuildVisibilityMesh()
    {
        //Collect all ray angles: cone edges + obstacle vertices
        List<float> angles = new List<float>();

        float half = viewAngle * 0.5f;
        for (int i = 0; i <= edgeRayCount; i++)
        {
            angles.Add(-half + viewAngle * i / edgeRayCount);
        }

        // find obstacle colliders in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(enemyTransform.position, viewRadius, obstacleMask);
        foreach (var col in hits)
        {
            // for each vertex in collider shape
            if (col is PolygonCollider2D poly)
            {
                foreach (var p in poly.points)
                {
                    Vector2 worldPt = poly.transform.TransformPoint(p);
                    Vector2 dir = (worldPt - (Vector2)enemyTransform.position).normalized;
                    float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    // only if within cone
                    if (Mathf.Abs(Mathf.DeltaAngle(a, transform.eulerAngles.z)) <= half)
                        angles.Add(Mathf.DeltaAngle(transform.eulerAngles.z, a));
                }
            }
            else if (col is BoxCollider2D box)
            {
                Vector2[] pts = new Vector2[4];
                // compute world corners
                Vector2 sz = box.size * 0.5f;
                pts[0] = new Vector2(-sz.x, -sz.y);
                pts[1] = new Vector2(-sz.x, sz.y);
                pts[2] = new Vector2(sz.x, sz.y);
                pts[3] = new Vector2(sz.x, -sz.y);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 worldPt = box.transform.TransformPoint(pts[i] + box.offset);
                    Vector2 dir = (worldPt - (Vector2)enemyTransform.position).normalized;
                    float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (Mathf.Abs(Mathf.DeltaAngle(a, transform.eulerAngles.z)) <= half)
                        angles.Add(Mathf.DeltaAngle(transform.eulerAngles.z, a));
                }
            }
        }

        //Raycast each angle, record hit points
        List<Vector3> verts = new List<Vector3> { Vector3.zero }; // center
        List<int> tris = new List<int>();
        angles.Sort();

        int idx = 1;
        foreach (float a in angles)
        {
            float worldAng = transform.eulerAngles.z + a;
            Vector3 dir = new Vector3(Mathf.Cos(worldAng * Mathf.Deg2Rad), Mathf.Sin(worldAng * Mathf.Deg2Rad));
            RaycastHit2D hit = Physics2D.Raycast(enemyTransform.position, dir, viewRadius, obstacleMask);
            float dist = hit.collider ? hit.distance : viewRadius;
            Vector3 localPt = transform.InverseTransformPoint((Vector2)enemyTransform.position + (Vector2)dir * dist);
            verts.Add(localPt);

            if (idx > 1)
            {
                tris.Add(0);
                tris.Add(idx - 1);
                tris.Add(idx);
            }
            idx++;
        }

        //Apply to mesh
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
    }

    private void CheckTargetVisibility()
    {
        if (target == null) return;

        Vector2 origin = transform.position;
        Vector2 toTarget = (Vector2)target.position - origin;
        float distance = toTarget.magnitude;    
        if (distance > viewRadius) return;
        float angle = Vector2.Angle(transform.right, toTarget.normalized);
        if (angle > viewAngle / 2f) return;
   
        RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, distance, obstacleMask);
        if (hit.collider == null)
        {
            Debug.Log("Target in sight!");
            
        }
    }
}
