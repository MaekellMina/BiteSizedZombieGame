using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
public class BasicPipeRotatePuzzle : PuzzleWrapper
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
            var neighborInThatDirection = NeigboringPieces[i];
        
            if (neighborInThatDirection.Neighbor == null)
                continue;
                   
            if(EntryPoints.Contains(neighborInThatDirection.Direction) == false)
            {
                neighborInThatDirection.Neighbor.DisconnectPiece(neighborInThatDirection.Direction.GetOpposite());
                DisconnectPiece(neighborInThatDirection.Direction);
                continue;
            }
         
            foreach (var _EntryPoints in EntryPoints)
            {
                if(neighborInThatDirection.Direction.ToVector() + _EntryPoints.ToVector() == Vector2.zero ) // means the direction should connect
                {
                    ConnectDirection(neighborInThatDirection.Direction);
                    neighborInThatDirection.Neighbor.ConnectDirection(_EntryPoints);
                    continue;
                }
                             
            }
                     

        }

        e_OnCheck?.Invoke();
    }

    
}
