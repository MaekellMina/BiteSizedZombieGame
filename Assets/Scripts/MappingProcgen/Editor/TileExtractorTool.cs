using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System.Drawing.Printing;

public class TileExtractorTool : EditorWindow
{
    private Tilemap sourceTilemap;
    private string filterName = "Wall";
    private string savePath = "Assets/Resources/Stamps/";
    private string stampName = "ExtractedStamp";

    [MenuItem("Tools/Tilemap/Extract Tiles to Stamp")]
    public static void OpenWindow()
    {
        GetWindow<TileExtractorTool>("Tile Extractor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tile Extractor Tool", EditorStyles.boldLabel);
        sourceTilemap = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", sourceTilemap, typeof(Tilemap), true);
        filterName = EditorGUILayout.TextField("Filter (Name Contains)", filterName);
        stampName = EditorGUILayout.TextField("New Stamp Name", stampName);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Extract and Save Stamp"))
        {
            if (sourceTilemap != null)
            {
                ExtractTiles();
            }
        }
    }

    private void ExtractTiles()
    {
        BoundsInt bounds = sourceTilemap.cellBounds;
        Vector3Int origin = bounds.position;
        Vector3Int size = new Vector3Int(bounds.size.x, bounds.size.y);
        TileBase[] tiles = new TileBase[size.x * size.y];

        int index = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector3Int pos = new Vector3Int(origin.x + x, origin.y + y, 0);
                TileBase tile = sourceTilemap.GetTile(pos);

                if (tile != null && tile.name.Contains(filterName))
                {
                    tiles[index] = tile;
                }
                else
                {
                    tiles[index] = null;
                }

                index++;
            }
        }

        // Create layer
        TilemapStamp.LayerData layer = new TilemapStamp.LayerData();
        {
            layer.size = size;
            layer.tiles = tiles;
        };

        // Create stamp
        TilemapStamp stamp = ScriptableObject.CreateInstance<TilemapStamp>();
        stamp.name = stampName;
        stamp.layers.Add(layer);

        // Save asset
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string fullPath = Path.Combine(savePath, $"{stampName}.asset");
        AssetDatabase.CreateAsset(stamp, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Extracted stamp saved to: {fullPath}");
    }
}
