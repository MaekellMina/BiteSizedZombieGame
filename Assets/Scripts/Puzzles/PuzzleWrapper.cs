using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class PuzzleWrapper: MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    public List<DIRECTIONS> EntryPoints = new List<DIRECTIONS>();
    [SerializeField]
    public List<DIRECTIONS> ConnectedPoints = new List<DIRECTIONS>();

    public UnityAction e_OnCheck;

    public List<PuzzleNeighbor> NeigboringPieces = new List<PuzzleNeighbor>()
    {
        new PuzzleNeighbor(DIRECTIONS.NORTH),
        new PuzzleNeighbor(DIRECTIONS.EAST),
        new PuzzleNeighbor(DIRECTIONS.SOUTH),
        new PuzzleNeighbor(DIRECTIONS.WEST)
    };

  
    public List<DIRECTIONS> GetEntryPoints()
    {
        return EntryPoints;
    }

  
    public virtual void DisconnectPiece(DIRECTIONS dir)
    {
        if (ConnectedPoints.Contains(dir))
            ConnectedPoints.Remove(dir);

    }

    public virtual void ConnectDirection(DIRECTIONS dir)
    {
        var s = "";
        for (int i = 0; i < EntryPoints.Count; i++)
        {
            s += EntryPoints[i].ToString();
            s += " ,";

        }
        Debug.Log($"Try connect {dir} : {s}");
        if (ConnectedPoints.Contains(dir))
            return;
        else
            ConnectedPoints.Add(dir);

    }
    public virtual void Check()
    {
        e_OnCheck?.Invoke();
    }


   

#if UNITY_EDITOR
    [Button]
    public void AutoLinkNeighbors(float tileSize = 1f)
    {
        NeigboringPieces.Clear();
        Vector2 myPos = transform.position;

        foreach (DIRECTIONS dir in System.Enum.GetValues(typeof(DIRECTIONS)))
        {
            var offset = dir.ToVector();
            Vector2 targetPos = myPos + offset;

            PuzzleWrapper found = FindNeighborAt(targetPos);
            if (found != null)
            {
                NeigboringPieces.Add(new PuzzleNeighbor(dir,found));

            }
            else
            {
                NeigboringPieces.Add(new PuzzleNeighbor(dir));

            }
        }
    }

    private PuzzleWrapper FindNeighborAt(Vector2 position)
    {
        foreach (PuzzleWrapper wrapper in FindObjectsOfType<PuzzleWrapper>())
        {
            if (wrapper == this) continue;
            if (Vector2.Distance(wrapper.transform.position, position) < 0.2f)
                return wrapper;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (NeigboringPieces == null || NeigboringPieces.Count == 0) return;

        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;

        foreach (var neighbor in NeigboringPieces)
        {
            if (neighbor.Neighbor == null) continue;

            Vector3 end = neighbor.Neighbor.transform.position;
            Gizmos.DrawLine(start, end);

            // Draw arrowhead
            Vector3 direction = (end - start).normalized;
            Vector3 right = Quaternion.Euler(0, 0, 135) * direction * 0.2f;
            Vector3 left = Quaternion.Euler(0, 0, -135) * direction * 0.2f;

            Gizmos.DrawLine(end, end - right);
            Gizmos.DrawLine(end, end - left);
        }
    }
#endif

}
