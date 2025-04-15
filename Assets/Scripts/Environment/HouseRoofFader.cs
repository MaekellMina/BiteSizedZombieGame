using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]

[RequireComponent(typeof(BoxCollider2D))]
public class HouseRoofFader : MonoBehaviour
{
    public Tilemap roofTilemap;            // Assign your shared roof tilemap here
    public float fadeDuration = 0.5f;
    [Range(0f, 1f)] public float hiddenAlpha = 0.2f;

    private BoundsInt houseTileBounds;
    private Dictionary<Vector3Int, Color32> originalColors = new();
    private Coroutine fadeRoutine;

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
                roofTilemap.SetTileFlags(pos, TileFlags.None);
              //  roofTilemap.SetColor(pos,new Color32(0,0,0,0));
            }
        }


    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure player has the tag
        {
            Debug.Log("PLAYER ENTER");
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeTiles(hiddenAlpha));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER EXT");
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeTiles(1f)); // Restore full alpha
        }
    }

    BoundsInt CalculateTileBounds()
    {
        var worldBounds = GetComponent<BoxCollider2D>().bounds;
        Vector3Int min = roofTilemap.WorldToCell(worldBounds.min);
        Vector3Int max = roofTilemap.WorldToCell(worldBounds.max);
        Vector3Int size = max - min + Vector3Int.one;
        return new BoundsInt(min, size);
    }

    void CacheOriginalTileColors()
    {
        originalColors.Clear();
        foreach (var pos in houseTileBounds.allPositionsWithin)
        {
            if (roofTilemap.HasTile(pos))
                originalColors[pos] = roofTilemap.GetColor(pos);
        }
    }

    IEnumerator FadeTiles(float targetAlpha)
    {
        float time = 0f;
        Dictionary<Vector3Int, Color> startColors = new();

        foreach (var kv in originalColors)
        {
            startColors[kv.Key] = roofTilemap.GetColor(kv.Key);
       
        }

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            foreach (var kv in originalColors)
            {
                if (!roofTilemap.HasTile(kv.Key))
                {   continue; }

                Debug.Log($"Fading tile at {kv.Key} to alpha {targetAlpha}");
                Color32 start = startColors[kv.Key];
                Color32 target = new Color32(1, 1, 1,0);
             

                Color32 blended = Color.Lerp(start, target, t);
               roofTilemap.SetColor(kv.Key, blended);
               // roofTilemap.SetColor(kv.Key,Color.clear);
            }

            yield return null;
        }

        // Ensure final alpha is set exactly
        foreach (var kv in originalColors)
        {
            if (!roofTilemap.HasTile(kv.Key)) continue;

            Color finalColor = kv.Value;
            finalColor.a = targetAlpha;
            roofTilemap.SetColor(kv.Key, finalColor);
        }

        fadeRoutine = null;
    }
}
