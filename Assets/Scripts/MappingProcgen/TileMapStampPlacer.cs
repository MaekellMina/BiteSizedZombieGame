using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapStampPlacer : MonoBehaviour
{
    public GameObject tilemapRoot;

    public void PlaceStamp(TilemapStamp stamp, Vector3Int position)
    {
        var tilemaps = new Dictionary<string, Tilemap>();
        foreach (var tm in tilemapRoot.GetComponentsInChildren<Tilemap>())
            tilemaps[tm.name] = tm;

        foreach (var layer in stamp.layers)
        {
            if (!tilemaps.TryGetValue(layer.layerName, out Tilemap tilemap))
            {
                Debug.LogWarning($"Tilemap '{layer.layerName}' not found.");
                continue;
            }

            for (int x = 0; x < layer.size.x; x++)
            {
                for (int y = 0; y < layer.size.y; y++)
                {
                    int index = x + y * layer.size.x;
                    Vector3Int pos = new Vector3Int(position.x + x, position.y + y, 0);
                    tilemap.SetTile(pos, layer.tiles[index]);
                }
            }
        }
    }
}
