using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using NaughtyAttributes;

[ExecuteAlways]
public class ForestGenerator : MonoBehaviour
{
    public enum NoiseType { Perlin, ValueNoise }

    [Header("Map Dimensions")]
    [Tooltip("Width of the generated map in tiles.")]
    public int width = 100;
    [Tooltip("Height of the generated map in tiles.")]
    public int height = 100;

    [Header("Noise Settings")]
    [Tooltip("Type of noise used for generation: Perlin or ValueNoise.")]
    public NoiseType noiseType = NoiseType.Perlin;
    [Tooltip("Controls how zoomed in the terrain noise is. Higher = more frequent terrain variation.")]
    public float terrainScale = 10f;
    [Tooltip("Controls how zoomed in the forest density noise is. Higher = more clumped trees.")]
    public float forestScale = 5f;
    [Tooltip("Threshold above which terrain is considered suitable for features like forests.")]
    [Range(0f, 1f)] public float terrainThreshold = 0.3f;
    [Tooltip("Threshold above which trees are placed in valid terrain.")]
    [Range(0f, 1f)] public float forestThreshold = 0.6f;

    [Header("Path Settings")]
    [Tooltip("Controls how zoomed in the dirt path noise is. Higher = more winding paths.")]
    public float pathScale = 15f;
    [Tooltip("Threshold below which a tile becomes part of a dirt path.")]
    [Range(0f, 1f)] public float pathThreshold = 0.2f;

    [Header("Tilemap References")]
    [Tooltip("Tilemap where ground tiles like grass and dirt are placed.")]
    public Tilemap groundMap;
    [Tooltip("Tilemap for decorative object tiles like flowers.")]
    public Tilemap objectMap;

    [Header("Tiles")]
    [Tooltip("Base grass tile.")]
    public TileBase grassTile;
    [Tooltip("Tile placed in terrain areas not occupied by tree canopy.")]
    public TileBase dirtTile;
    [Tooltip("Array of decorative object tiles (e.g., flowers, rocks) placed randomly on grass.")]
    public TileBase[] decorativeTiles;
    [Tooltip("Probability of placing a decorative tile on an empty grass or dirt tile.")]
    [Range(0f, 1f)] public float decorationDensity = 0.1f;

    [Header("Canopy Sprites")]
    [Tooltip("Array of tree canopy prefabs to randomly instantiate. Each prefab represents one tree.")]
    public GameObject[] canopyPrefabs;
    [Tooltip("Optional parent object to keep the hierarchy organized.")]
    public Transform canopyContainer;

    private HashSet<Vector3Int> canopyPositions = new HashSet<Vector3Int>();

    // Noise offsets for each layer (randomized)
    private int terrainOffsetX, terrainOffsetY;
    private int forestOffsetX, forestOffsetY;
    private int pathOffsetX, pathOffsetY;

    [SerializeField]
    List<GameObject> allGameObjects = new List<GameObject>();
   
    private void Start()
    {
        //Generate();
    }

    #region Map Starts

    [Button("Generate Forest")]
    public void VanillaGenerate()
    {
        terrainOffsetX = 0;
        terrainOffsetY = 0;
        forestOffsetX = 0;
        forestOffsetY = 0;
        pathOffsetX = 0;
        pathOffsetY = 0;

        Generate();

    }

    [Button("Clear Objects")]
    public void ClearTrees()
    {
        canopyPositions.Clear();

        if (canopyContainer != null)
        {
            foreach (GameObject child in allGameObjects)
            {
                DestroyImmediate(child);
            }
            allGameObjects.Clear();
        }

    }
    [Button("Generate  with Random Noise Offsets")]
    public void RollTheDieGenerate()
    {
        RandomizeOffsets();
        Generate();
    }

    #endregion

    public void Generate()
    {       

        if (groundMap == null || objectMap == null)
        {
            Debug.LogError("Please assign all Tilemap references in the Inspector.");
            return;
        }

        groundMap.ClearAllTiles();
        objectMap.ClearAllTiles();
        canopyPositions.Clear();

        if (canopyContainer != null)
        {
            foreach (GameObject child in allGameObjects)
            {
                DestroyImmediate(child);
            }
            allGameObjects.Clear();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                float terrainValue = GetNoise(x + terrainOffsetX, y + terrainOffsetY, terrainScale);
                float forestValue = GetNoise(x + forestOffsetX, y + forestOffsetY, forestScale);
                float pathValue = GetNoise(x + pathOffsetX, y + pathOffsetY, pathScale);

                bool isForest = terrainValue > terrainThreshold && forestValue > forestThreshold;
                bool isPath = pathValue < pathThreshold;

                bool hasCanopy = false;

                // Place tree if it's forest and not path
                if (isForest && !isPath && canopyPrefabs != null && canopyPrefabs.Length > 0)
                {
                    GameObject prefab = canopyPrefabs[Random.Range(0, canopyPrefabs.Length)];
                    Vector3 worldPos = groundMap.CellToWorld(cellPos + Vector3Int.up);
                    GameObject canopyInstance = Instantiate(prefab, worldPos, Quaternion.identity, canopyContainer);
                    allGameObjects.Add(canopyInstance);
                    canopyInstance.gameObject.SetActive(true);
                    var randomizer = canopyInstance.GetComponent<RandomTreeSpawner>();
                    randomizer?.RandomizeSetUp();

                    canopyInstance.name = $"Canopy_{x}_{y}";
                    canopyPositions.Add(cellPos);
                    hasCanopy = true;
                }

                // Ground tile placement
                if (hasCanopy || isPath)
                {
                    groundMap.SetTile(cellPos, dirtTile);
                }
                else
                {
                    groundMap.SetTile(cellPos, grassTile);

                    if (decorativeTiles != null && decorativeTiles.Length > 0 && Random.value < decorationDensity)
                    {
                        TileBase decorTile = decorativeTiles[Random.Range(0, decorativeTiles.Length)];
                        objectMap.SetTile(cellPos, decorTile);
                    }
                }
            }
        }
    }

    private float GetNoise(int x, int y, float scale)
    {
        switch (noiseType)
        {
            case NoiseType.ValueNoise:
                return Mathf.Abs(Mathf.Sin((x * 12.9898f + y * 78.233f) * 43758.5453f)) % 1f;
            case NoiseType.Perlin:
            default:
                return Mathf.PerlinNoise(x / scale, y / scale);
        }
    }


    private void RandomizeOffsets()
    {
        terrainOffsetX = Random.Range(0, 100);
        terrainOffsetY = Random.Range(0, 100);
        forestOffsetX = Random.Range(0, 100);
        forestOffsetY = Random.Range(0, 100);
        pathOffsetX = Random.Range(0, 100);
        pathOffsetY = Random.Range(0, 100);
       
    }
}