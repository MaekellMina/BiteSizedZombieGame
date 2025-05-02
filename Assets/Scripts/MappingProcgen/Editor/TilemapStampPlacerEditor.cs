using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapStampPlacerEditor : EditorWindow
{
    private TilemapStamp stamp;
    private GameObject tilemapRoot;
    private Vector3Int hoverPos;
    private bool placing = false;

    private Dictionary<string, Tilemap> layerMap = new();

    [MenuItem("Tools/Tilemap Stamp Placer")]
    public static void ShowWindow()
    {
        GetWindow<TilemapStampPlacerEditor>("Stamp Placer");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        stamp = (TilemapStamp)EditorGUILayout.ObjectField("Stamp", stamp, typeof(TilemapStamp), false);
        tilemapRoot = (GameObject)EditorGUILayout.ObjectField("Tilemap Root", tilemapRoot, typeof(GameObject), true);

        if (GUILayout.Toggle(placing, "Placing Mode", "Button"))
        {
            placing = true;
            BuildLayerMap();
        }
        else
        {
            placing = false;
        }
    }

    void BuildLayerMap()
    {
        if (tilemapRoot == null) return;

        layerMap.Clear();
        foreach (var tm in tilemapRoot.GetComponentsInChildren<Tilemap>())
        {
            layerMap[tm.name] = tm;
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!placing || stamp == null || tilemapRoot == null)
            return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;
        Vector3 worldMousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        Vector3Int cell = Vector3Int.FloorToInt(worldMousePos);

        hoverPos = cell;

        // Draw preview
        DrawPreview(cell);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            PlaceStamp(cell);
            e.Use();
        }
    }

    void DrawPreview(Vector3Int pos)
    {
        foreach (var layer in stamp.layers)
        {
            for (int x = 0; x < layer.size.x; x++)
            {
                for (int y = 0; y < layer.size.y; y++)
                {
                    int i = x + y * layer.size.x;
                    var tile = layer.tiles[i];
                    if (tile == null) continue;

                    Vector3 tileWorld = pos + new Vector3Int(x, y, 0);
                    Handles.color = new Color(1, 1, 1, 0.4f);
                    Handles.DrawWireCube(tileWorld + Vector3.one * 0.5f, Vector3.one);
                }
            }
        }
    }

    void PlaceStamp(Vector3Int pos)
    {
        foreach (var layer in stamp.layers)
        {
            if (!layerMap.TryGetValue(layer.layerName, out Tilemap tm))
            {
                Debug.LogWarning($"Tilemap '{layer.layerName}' not found.");
                continue;
            }

            for (int x = 0; x < layer.size.x; x++)
            {
                for (int y = 0; y < layer.size.y; y++)
                {
                    int i = x + y * layer.size.x;
                    var tile = layer.tiles[i];
                    if (tile == null) continue;

                    tm.SetTile(pos + new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
