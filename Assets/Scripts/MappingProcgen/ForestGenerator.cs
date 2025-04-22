using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;
using System.Collections.Generic;

[System.Serializable]
public class ClusterData
{
    public GameObject clusterPrefab;
    public Vector2Int position;
}

[ExecuteAlways]
public class ForestGenerator : MonoBehaviour
{
    public enum NoiseType { Perlin, ValueNoise }

    [Header("Map Settings")]
    public int width = 100;
    public int height = 100;

    [Header("Tilemaps")]
    public Tilemap groundMap;
    public Tilemap objectMap;

    [Header("Tiles")]
    public TileBase grassTile;
    public TileBase dirtTile;
    public TileBase waterTile;
    public TileBase[] decorativeTiles;

    [Header("Decoration Settings")]
    [Range(0f, 1f)] public float decorationDensity = 0.1f;

    [Header("Canopy Settings")]
    public GameObject[] canopyPrefabs;
    public Transform canopyContainer;

    [Header("Noise Settings")]
    public NoiseType noiseType = NoiseType.Perlin;

    public float terrainScale = 10f;
    public float forestScale = 5f;
    public float pathScale = 15f;
    public float waterScale = 7f;

    [Range(0f, 1f)] public float terrainThreshold = 0.3f;
    [Range(0f, 1f)] public float forestThreshold = 0.6f;
    [Range(0f, 1f)] public float pathThreshold = 0.2f;
    [Range(0f, 1f)] public float waterThreshold = 0.2f;

   // [Header("Cluster Injection")]
   // public List<ClusterData> clustersToInject;

    private HashSet<Vector3Int> canopyPositions = new();
    [SerializeField]
    private List<GameObject> allGameObjects = new();

    private int terrainOffsetX, terrainOffsetY;
    private int forestOffsetX, forestOffsetY;
    private int pathOffsetX, pathOffsetY;
    private int waterOffsetX, waterOffsetY;

    #region Buttons

    [Button("Generate Forest")]
    public void GenerateVanilla() => Generate(useRandomOffsets: false);

    [Button("Generate With Random Offsets")]
    public void GenerateWithRandomOffsets() => Generate(useRandomOffsets: true);

    [Button("Clear Canopy")]
    public void ClearCanopies()
    {
        canopyPositions.Clear();
        foreach (var obj in allGameObjects)
            DestroyImmediate(obj);
        allGameObjects.Clear();
    }

    #endregion

    public void Generate(bool useRandomOffsets)
    {
        if (!groundMap || !objectMap)
        {
            Debug.LogError("Assign tilemaps.");
            return;
        }

        bool[,] forestMap = new bool[width, height];

        if (useRandomOffsets)
            RandomizeOffsets();
        else
            ResetOffsets();

        ClearCanopies();
        groundMap.ClearAllTiles();
        objectMap.ClearAllTiles();

        //InjectClusters();

        GenerateWater();
        GenerateForests();
        GenerateGround();
        PlaceDecorations();
    }

    private void GenerateWater()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new(x, y, 0);
                if (GetNoise(x + waterOffsetX, y + waterOffsetY, waterScale) < waterThreshold)
                    groundMap.SetTile(pos, waterTile);
            }


    }



    private void GenerateForests()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new(x, y, 0);
                if (groundMap.GetTile(pos) == waterTile) continue;

                float terrain = GetNoise(x + terrainOffsetX, y + terrainOffsetY, terrainScale);
                float forest = GetNoise(x + forestOffsetX, y + forestOffsetY, forestScale);
                float path = GetNoise(x + pathOffsetX, y + pathOffsetY, pathScale);

                if (terrain > terrainThreshold && forest > forestThreshold && path > pathThreshold)
                {
                    GameObject prefab = canopyPrefabs[Random.Range(0, canopyPrefabs.Length)];
                    GameObject canopy = Instantiate(prefab, groundMap.CellToWorld(pos + Vector3Int.up), Quaternion.identity, canopyContainer);
                    canopy.name = $"Canopy_{x}_{y}";
                    canopy.SetActive(true);
                    allGameObjects.Add(canopy);

                    var randomizer = canopy.GetComponent<RandomTreeSpawner>();
                    randomizer?.RandomizeSetUp();

                    canopyPositions.Add(pos);
                    groundMap.SetTile(pos, dirtTile);
                }
            }
    }

    private void GenerateGround()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new(x, y, 0);
                if (groundMap.GetTile(pos) != null) continue;

                float path = GetNoise(x + pathOffsetX, y + pathOffsetY, pathScale);
                groundMap.SetTile(pos, path < pathThreshold ? dirtTile : grassTile);
            }
    }

    private void PlaceDecorations()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new(x, y, 0);
               
                if (canopyPositions.Contains(pos)) continue;

                TileBase currentTile = groundMap.GetTile(pos);

                if (currentTile == grassTile || currentTile == dirtTile)
                {
                    if (Random.value < decorationDensity && decorativeTiles.Length > 0)
                    {
                        TileBase decor = decorativeTiles[Random.Range(0, decorativeTiles.Length)];
                        objectMap.SetTile(pos, decor);
                    }
                }
            }
    }


    //private void InjectClusters()
    //{
    //    foreach (var cluster in clustersToInject)
    //    {
    //        if (cluster.clusterPrefab == null) continue;

    //        GameObject temp = Instantiate(cluster.clusterPrefab);
    //        Tilemap clusterMap = temp.GetComponentInChildren<Tilemap>();
    //        if (!clusterMap)
    //        {
    //            DestroyImmediate(temp);
    //            continue;
    //        }

    //        BoundsInt bounds = clusterMap.cellBounds;
    //        TileBase[] tiles = clusterMap.GetTilesBlock(bounds);

    //        for (int x = 0; x < bounds.size.x; x++)
    //            for (int y = 0; y < bounds.size.y; y++)
    //            {
    //                TileBase tile = tiles[x + y * bounds.size.x];
    //                if (tile == null) continue;

    //                Vector3Int local = new(x, y, 0);
    //                Vector3Int target = new(cluster.position.x + local.x, cluster.position.y + local.y, 0);
    //                groundMap.SetTile(target, tile);
    //            }

    //        DestroyImmediate(temp);
    //    }
    //}

    private float GetNoise(int x, int y, float scale)
    {
        return noiseType == NoiseType.Perlin
            ? Mathf.PerlinNoise(x / scale, y / scale)
            : Mathf.Abs(Mathf.Sin((x * 12.9898f + y * 78.233f) * 43758.5453f)) % 1f;
    }

    private void ResetOffsets()
    {
        terrainOffsetX = terrainOffsetY = 0;
        forestOffsetX = forestOffsetY = 0;
        pathOffsetX = pathOffsetY = 0;
        waterOffsetX = waterOffsetY = 0;
    }

    private void RandomizeOffsets()
    {
        terrainOffsetX = Random.Range(0, 100);
        terrainOffsetY = Random.Range(0, 100);
        forestOffsetX = Random.Range(0, 100);
        forestOffsetY = Random.Range(0, 100);
        pathOffsetX = Random.Range(0, 100);
        pathOffsetY = Random.Range(0, 100);
        waterOffsetX = Random.Range(0, 100);
        waterOffsetY = Random.Range(0, 100);
    }


    private bool HasMinAdjacentForestTiles(int x, int y, int minCount, bool[,] forestMap)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    if (forestMap[nx, ny]) count++;
                }
            }
        }

        return count >= minCount;
    }
}
