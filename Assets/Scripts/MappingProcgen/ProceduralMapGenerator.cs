using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 100;
    public int height = 100;
    public float noiseScale = 10f;

    [Header("Tilemap References")]
    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    void Start()
    {
        GeneratePerlinMap();
    }

    void GeneratePerlinMap()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Sample Perlin noise
                float sample = Mathf.PerlinNoise(x / noiseScale, y / noiseScale);

                // Threshold to decide floor vs. wall
                TileBase chosen = (sample > 0.5f) ? floorTile : wallTile;

                tilemap.SetTile(new Vector3Int(x, y, 0), chosen);
            }
        }
    }
}
