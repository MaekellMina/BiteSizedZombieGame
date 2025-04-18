using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;


[System.Serializable]

[RequireComponent(typeof(BoxCollider2D))]
public class HouseRoofFader : MonoBehaviour
{
    public Tilemap roofTilemap;           
    public Tilemap accesoryMap;            
    public float fadeDuration = 0.5f;
    [Range(0f, 1f)] public float hiddenAlpha = 0.2f;

    [Header("Bounds Setup")]
    [SerializeField] private BoundsInt houseBounds;

    private BoundsInt houseTileBounds;
    private Dictionary<Vector3Int, Color> originalColors = new();
    private Dictionary<Vector3Int, Color> accessoryColors = new();

    public BoundsInt HouseBounds { get => houseBounds; set => houseBounds = value; }

    public UnityEvent e_OnShowInside = new UnityEvent();
    public UnityEvent e_OnHideInside = new UnityEvent();

    void Start()
    {
        if (roofTilemap == null)
        {
            Debug.LogError("Roof Tilemap not assigned.");
            enabled = false;
            return;
        }

        houseTileBounds = CalculateTileBounds();
        CacheOriginalTileColors();

        foreach (var pos in roofTilemap.cellBounds.allPositionsWithin)
        {
            if (roofTilemap.HasTile(pos))
            {
                roofTilemap.SetTileFlags(pos, TileFlags.None); // gotta set this or else the color might get locked
              //  roofTilemap.SetColor(pos,new Color32(0,0,0,0));
            }
        }
        foreach (var pos in accesoryMap.cellBounds.allPositionsWithin)
        {
            if (accesoryMap.HasTile(pos))
            {
                accesoryMap.SetTileFlags(pos, TileFlags.None); // gotta set this or else the color might get locked
                                                               //  roofTilemap.SetColor(pos,new Color32(0,0,0,0));
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure player has the tag
        {
            Debug.Log("PLAYER ENTER");

            FadeTile(hiddenAlpha);
           
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER EXT");
            FadeTile(1);
           
           
        }
    }

    BoundsInt CalculateTileBounds()
    {

        var worldBounds = houseBounds;
        Vector3Int min = roofTilemap.WorldToCell(worldBounds.min);
        Vector3Int max = roofTilemap.WorldToCell(worldBounds.max);
        Vector3Int size = max - min + Vector3Int.one;
        return new BoundsInt(min, size);
    }

    void CacheOriginalTileColors()
    {
        originalColors.Clear();
        accessoryColors.Clear();
        foreach (var pos in houseTileBounds.allPositionsWithin)
        {
            if (roofTilemap.HasTile(pos))
                originalColors[pos] = roofTilemap.GetColor(pos);
            if (accesoryMap.HasTile(pos))
                accessoryColors[pos] = accesoryMap.GetColor(pos);
        }

      
    }

    public void FadeTile(float targetAlpha)
    {       
        Dictionary<Vector3Int, Color> startColors = new();

        foreach (var kv in originalColors)
        {
            startColors[kv.Key] = roofTilemap.GetColor(kv.Key);
        }

        foreach (var kv in originalColors)
        {
            if (!roofTilemap.HasTile(kv.Key))
            { continue; }

           // Debug.Log($"Fading tile at {kv.Key} to alpha {targetAlpha}");
            Color start = startColors[kv.Key];
            Color target = new Color(1, 1, 1, targetAlpha);

            Tween.Custom(start, target, duration: fadeDuration, (x) => roofTilemap.SetColor(kv.Key, x))
                .OnComplete(()=>roofTilemap.SetColor(kv.Key,target));

        }

        foreach (var kv in accessoryColors)
        {
            startColors[kv.Key] = accesoryMap.GetColor(kv.Key);
        }

        foreach (var kv in accessoryColors)
        {
            if (!accesoryMap.HasTile(kv.Key))
            { continue; }

            //Debug.Log($"Fading tile at {kv.Key} to alpha {targetAlpha}");
            Color start = startColors[kv.Key];
            Color target = new Color(1, 1, 1, targetAlpha);

            Tween.Custom(start, target, duration: fadeDuration, (x) => accesoryMap.SetColor(kv.Key, x))
                .OnComplete(() => accesoryMap.SetColor(kv.Key, target));

        }

       
        if (targetAlpha > 0) e_OnHideInside?.Invoke();
        else e_OnShowInside?.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(houseBounds.center, houseBounds.size); 
    }

}
