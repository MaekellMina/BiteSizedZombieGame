using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using cc.Interaction.Interface;

[System.Serializable]
///Directions up clockwise
public enum DIRECTIONS
{
    NORTH = 0,
    EAST = 1,
    SOUTH = 2,
    WEST = 3,
}
public static class DirectionExtensions
{
    public static Vector2Int ToVector(this DIRECTIONS dir)
    {
        switch (dir)
        {
            case DIRECTIONS.NORTH: return new Vector2Int(0, 1);
            case DIRECTIONS.EAST: return new Vector2Int(1, 0);
            case DIRECTIONS.SOUTH: return new Vector2Int(0, -1);
            case DIRECTIONS.WEST: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }

    public static DIRECTIONS GetOpposite(this DIRECTIONS dir)
    {
        switch (dir)
        {
            case DIRECTIONS.NORTH: return DIRECTIONS.SOUTH;
            case DIRECTIONS.SOUTH: return DIRECTIONS.NORTH;
            case DIRECTIONS.EAST: return DIRECTIONS.WEST;
            case DIRECTIONS.WEST: return DIRECTIONS.EAST;
            default: throw new System.ArgumentOutOfRangeException();
        }
    }
}

[System.Serializable]
public class Neighbors
{
    public DIRECTIONS Direction;
    public PuzzleWrapper Neighbor;
}

[System.Serializable]
public class PuzzleNeighbor
{
    public DIRECTIONS Direction;
    public PuzzleWrapper Neighbor;

    public PuzzleNeighbor(DIRECTIONS direction)
    {

        Direction = direction;
    }

    public PuzzleNeighbor(DIRECTIONS direction, PuzzleWrapper neighbor)
    {
        Direction = direction;
        Neighbor = neighbor;
    }
}
public class BasicPipeRotatePuzzle : PuzzleWrapper,IInteractable
{
   
    [System.Serializable]
    public class RotatePuzzlePiece
    {
        public DIRECTIONS direction;
        public Sprite puzzleSprite;
        public List<DIRECTIONS> ConnectionPoint = new List<DIRECTIONS>();
        public RotatePuzzlePiece(DIRECTIONS direction)
        {
            this.direction = direction;
        }
    }

    [SerializeField]
    SpriteRenderer pieceSprite;

    public List<RotatePuzzlePiece> PuzzlePieces = new List<RotatePuzzlePiece>()
    {
        new RotatePuzzlePiece(DIRECTIONS.NORTH),
        new RotatePuzzlePiece(DIRECTIONS.EAST),
        new RotatePuzzlePiece(DIRECTIONS.SOUTH),
        new RotatePuzzlePiece(DIRECTIONS.WEST)
    };

 


    [SerializeField]
    DIRECTIONS CurrentDirection = DIRECTIONS.NORTH;

    private void Start()
    {
      
        pieceSprite.sprite = PuzzlePieces[(int)CurrentDirection].puzzleSprite;
        EntryPoints = PuzzlePieces[(int)CurrentDirection].ConnectionPoint;      
     

        Check();
    }

    [Button]
    public void Rotate()
    {
        var idx = (int)CurrentDirection;
        idx = (idx+1) % PuzzlePieces.Count ;
        //Debug.Log(idx);
        CurrentDirection = (DIRECTIONS)idx;
        pieceSprite.sprite = PuzzlePieces[idx].puzzleSprite;
        EntryPoints = PuzzlePieces[idx].ConnectionPoint;

        Check();

    }

    public override void Check()
    {
        for (int i = 0; i < NeigboringPieces.Count; i++)
        {
            var neighbor = NeigboringPieces[i];
            var dir = neighbor.Direction;
            var oppDir = dir.GetOpposite();
            var neighborPiece = neighbor.Neighbor;

            if (neighborPiece == null || !EntryPoints.Contains(dir))
            {
                neighborPiece?.DisconnectDirection(oppDir);
                DisconnectDirection(dir);
                continue;
            }

            bool connects = false;
            foreach (var _NeighborEntryPoint in neighborPiece.EntryPoints)
            {
                if (dir.ToVector() + _NeighborEntryPoint.ToVector() == Vector2.zero) // means the direction should connect
                {
                    ConnectDirection(dir);
                    neighborPiece.ConnectDirection(_NeighborEntryPoint);
                    connects = true;
                    break;
                }
            }
            if (!connects)
            {
                neighborPiece.DisconnectDirection(oppDir);
                DisconnectDirection(dir);
            }
        }

        e_OnCheck?.Invoke();
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("HERE");
            InteractionManager.instance?.Register(this);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InteractionManager.instance?.Unregister(this);
        }
    }

    public void OnInteract()
    {
        Debug.Log($"Attempt rotate {gameObject.name}");
        Rotate();
    }

    public GameObject GetTargetObject() => gameObject;

  
}
