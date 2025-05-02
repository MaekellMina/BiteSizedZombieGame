using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;

public class TilemapStampCreatorEditor : EditorWindow
{
    private GameObject tileMapsParent;
    private Vector3Int startCell;
    private Vector3Int endCell;
    private bool selecting;
    private bool hasSelection;

    [MenuItem("Tools/Tilemap Stamp Creator")]
    public static void ShowWindow()
    {
        GetWindow<TilemapStampCreatorEditor>("Stamp Creator");
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
        EditorGUILayout.HelpBox("Left-click to set the start of the rectangle." +
            "\nRelease to set the end of the rectangle." +
            "\nYou'll see a glowing orange grid.", MessageType.Info);

        tileMapsParent = (GameObject)EditorGUILayout.ObjectField("Tilemap Root", tileMapsParent, typeof(GameObject), true);

        if (hasSelection)
        {
            EditorGUILayout.LabelField("Selected Bounds:");
            EditorGUILayout.LabelField($"From: {startCell} To: {endCell}");
        }

        if (GUILayout.Button("Clear Selection"))
        {
            hasSelection = false;
        }

        GUI.enabled = hasSelection && tileMapsParent != null;

        if (GUILayout.Button("Create Stamp from Selection"))
        {
            CreateStamp();
        }

        GUI.enabled = true;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (tileMapsParent == null) return;

        Event e = Event.current;
        Vector3 mouseWorld = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        Vector3Int cell = Vector3Int.FloorToInt(mouseWorld);

        if (e.type == EventType.MouseDown && e.button == 0 && !selecting)
        {
            startCell = cell;
            selecting = true;
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0 && selecting)
        {
            endCell = cell;
            selecting = false;
            hasSelection = true;
            e.Use();
        }

        if (selecting || hasSelection)
        {
            var min = Vector3Int.Min(startCell, endCell);
            var max = Vector3Int.Max(startCell, endCell);

            Handles.color = new Color(1, 0.5f, 0, 0.8f);
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    Handles.DrawWireCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector3.one);
                }
            }
        }
    }

    private void CreateStamp()
    {
        var stamp = ScriptableObject.CreateInstance<TilemapStamp>();
        var tilemaps = tileMapsParent.GetComponentsInChildren<Tilemap>();
        var layers = new List<TilemapStamp.LayerData>();

        var min = Vector3Int.Min(startCell, endCell);
        var max = Vector3Int.Max(startCell, endCell);
        Vector3Int size = new(max.x - min.x + 1, max.y - min.y + 1);

        foreach (var tm in tilemaps)
        {
            var tiles = new TileBase[size.x * size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3Int worldCell = min + new Vector3Int(x, y, 0);
                    Vector3Int localCell = tm.WorldToCell((Vector3)worldCell + Vector3.one * 0.5f);
                    tiles[x + y * size.x] = tm.GetTile(localCell);
                }
            }

            layers.Add(new TilemapStamp.LayerData
            {
                layerName = tm.name,
                size = size,
                tiles = tiles
            });
        }

        stamp.layers = layers;

        string path = EditorUtility.SaveFilePanelInProject("Save Stamp", "NewTilemapStamp", "asset", "Save stamp as asset");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(stamp, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = stamp;
        }
    }
}
