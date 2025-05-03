using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapStampTool : EditorWindow
{
    private enum Mode { SelectAndSave, PlaceStamp }
    private Mode currentMode = Mode.SelectAndSave;

    private GameObject tilemapRoot;
    private TilemapStamp currentStamp;

    // Selection state
    private bool selecting = false;
    private Vector3Int selectStart, selectEnd;
    private bool hasSelection = false;

    // Placement state
    private bool placing = false;
    private Vector3Int hoverPosition;

    private Vector2 stampScroll;
    private List<TilemapStamp> availableStamps;

   


    [MenuItem("Tools/Tilemap Stamp Tool")]
    public static void ShowWindow()
    {
        GetWindow<TilemapStampTool>("Tilemap Stamp Tool");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

      
        availableStamps = new List<TilemapStamp>(Resources.LoadAll<TilemapStamp>(""));
        foreach (var stamp in availableStamps)
        {
            stamp.previewImage = GeneratePreview(stamp);
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Tilemap Stamp Tool", EditorStyles.boldLabel);

        currentMode = (Mode)EditorGUILayout.EnumPopup("Mode", currentMode);
        tilemapRoot = (GameObject)EditorGUILayout.ObjectField("Tilemap Root", tilemapRoot, typeof(GameObject), true);

        if (currentMode == Mode.SelectAndSave)
        {
            DrawSelectionGUI();
        }
        else if (currentMode == Mode.PlaceStamp)
        {
            DrawPlacementGUI();
        }
    }

    private void DrawSelectionGUI()
    {
        EditorGUILayout.HelpBox("Left-click to set the start of the rectangle." +
            "\nRelease to set the end of the rectangle." +
            "\nYou'll see a glowing orange grid.", MessageType.Info);

        if (hasSelection)
        {
            EditorGUILayout.LabelField($"Selected Bounds: {selectStart} to {selectEnd}");
        }

        if (GUILayout.Button("Clear Selection"))
        {
            hasSelection = false;
        }

        GUI.enabled = hasSelection && tilemapRoot != null;

        if (GUILayout.Button("Create Stamp from Selection"))
        {
            CreateStampFromSelection();
        }

        GUI.enabled = true;
    }

    private void DrawPlacementGUI()
    {
        currentStamp = (TilemapStamp)EditorGUILayout.ObjectField("Stamp Asset", currentStamp, typeof(TilemapStamp), false);

        if (GUILayout.Button("Reload Stamps"))
        {
            availableStamps = new List<TilemapStamp>(Resources.LoadAll<TilemapStamp>(""));
        }

        if (currentStamp != null && GUILayout.Button("Start Placing"))
        {
            placing = true;
        }

        if (placing && GUILayout.Button("Stop Placing"))
        {
            placing = false;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Available Stamps:", EditorStyles.boldLabel);

        stampScroll = EditorGUILayout.BeginScrollView(stampScroll, GUILayout.Height(200));
        if (availableStamps != null)
        {
            foreach (var stamp in availableStamps)
            {
                if (stamp == null) continue;

                EditorGUILayout.BeginHorizontal(GUILayout.Height(50));

                // Show preview image
                Texture2D preview = stamp.previewImage;

                if (preview != null)
                {
                    GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
                }
                else
                {
                    GUILayout.Label("No Preview", GUILayout.Width(60));
                }

          
                if (GUILayout.Button(stamp.name))
                {
                    currentStamp = stamp;
                }

                EditorGUILayout.EndHorizontal();
            }

        }
        EditorGUILayout.EndScrollView();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (tilemapRoot == null)
            return;

        Event e = Event.current;
        Vector3 mouseWorld = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        Vector3Int cell = Vector3Int.FloorToInt(mouseWorld);

        if (currentMode == Mode.SelectAndSave)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && !selecting)
            {
                selectStart = cell;
                selecting = true;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && selecting)
            {
                selectEnd = cell;
                selecting = false;
                hasSelection = true;
                e.Use();
                sceneView.Repaint();
            }

            if (selecting || hasSelection)
            {
                DrawSelectionRect();
            }
        }
        else if (currentMode == Mode.PlaceStamp && placing && currentStamp != null)
        {
            hoverPosition = cell;
            DrawStampPreview();

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceStampAt(hoverPosition);
                e.Use();
            }

            sceneView.Repaint();
        }
    }

    private void DrawSelectionRect()
    {
        Vector3Int min = Vector3Int.Min(selectStart, selectEnd);
        Vector3Int max = Vector3Int.Max(selectStart, selectEnd);

        Handles.color = new Color(1, 0.5f, 0, 0.8f);
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Handles.DrawWireCube(new Vector3(x + 0.5f, y + 0.5f), Vector3.one);
            }
        }
    }

    private void DrawStampPreview()
    {
        if (currentStamp?.layers == null) return;

        foreach (var layer in currentStamp.layers)
        {
            var tm = FindTilemap(layer.layerName);
            if (tm == null) continue;

            Vector3Int size = layer.size;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    TileBase tile = layer.tiles[x + y * size.x];
                    if (tile == null) continue;

                    Vector3Int pos = new Vector3Int(hoverPosition.x + x, hoverPosition.y + y, 0);
                    Handles.color = new Color(0, 1, 1, 0.4f);
                    Handles.DrawSolidRectangleWithOutline(new[]
                    {
                        (Vector3)pos,
                        (Vector3)pos + new Vector3(1, 0),
                        (Vector3)pos + new Vector3(1, 1),
                        (Vector3)pos + new Vector3(0, 1)
                    }, new Color(0, 1, 1, 0.25f), Color.cyan);
                }
            }
        }
    }

    private void PlaceStampAt(Vector3Int basePos)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        foreach (var layer in currentStamp.layers)
        {
            var tm = FindTilemap(layer.layerName);
            if (tm == null)
            {
                Debug.LogWarning($"Tilemap layer not found: {layer.layerName}");
                continue;
            }

            Undo.RegisterCompleteObjectUndo(tm, "Place Tilemap Stamp");

            Vector3Int size = layer.size;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    TileBase tile = layer.tiles[x + y * size.x];
                    if (tile == null) continue;

                    Vector3Int pos = new Vector3Int(basePos.x + x, basePos.y + y, 0);
                    tm.SetTile(pos, tile);
                }
            }

            EditorUtility.SetDirty(tm);
        }

        Undo.CollapseUndoOperations(group);
    }


    private Tilemap FindTilemap(string name)
    {
        if (tilemapRoot == null) return null;

        foreach (var tm in tilemapRoot.GetComponentsInChildren<Tilemap>())
        {
            if (tm.name == name)
                return tm;
        }

        return null;
    }

    private void CreateStampFromSelection()
    {
        if (tilemapRoot == null || !hasSelection) return;

        var stamp = ScriptableObject.CreateInstance<TilemapStamp>();
        var tilemaps = tilemapRoot.GetComponentsInChildren<Tilemap>();
        var layers = new List<TilemapStamp.LayerData>();

        Vector3Int min = Vector3Int.Min(selectStart, selectEnd);
        Vector3Int max = Vector3Int.Max(selectStart, selectEnd);
        Vector3Int size = new Vector3Int(max.x - min.x + 1, max.y - min.y + 1);

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
       
        string path = EditorUtility.SaveFilePanelInProject("Save Tilemap Stamp", "NewTilemapStamp", "asset", "Save stamp asset","Resources/Stamps");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(stamp, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = stamp;
        }
    }

    private Texture2D GeneratePreview(TilemapStamp stamp)
    {
        if (stamp == null || stamp.layers == null || stamp.layers.Count == 0)
            return null;

        const int pixelsPerUnit = 16; // Common default in Unity
        const int tilePixelSize = 16;  // Adjust to your tile sprite resolution (e.g., 16, 32, 64)

        // Determine overall stamp bounds
        int width = 0, height = 0;
        foreach (var layer in stamp.layers)
        {
            width = Mathf.Max(width, layer.size.x);
            height = Mathf.Max(height, layer.size.y);
        }

        int texWidth = width * tilePixelSize;
        int texHeight = height * tilePixelSize;

        Texture2D preview = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
        Color32[] fill = new Color32[texWidth * texHeight];
        for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(0, 0, 0, 0);
        preview.SetPixels32(fill);

        foreach (var layer in stamp.layers)
        {
            for (int x = 0; x < layer.size.x; x++)
            {
                for (int y = 0; y < layer.size.y; y++)
                {
                    var tile = layer.tiles[x + y * layer.size.x];
                    if (tile == null) continue;

                    Sprite sprite = null;
                    if (tile is UnityEngine.Tilemaps.Tile t)
                    {
                        sprite = t.sprite;
                    }

                    if (sprite == null) continue;

                    // Extract pixels from sprite texture
                    Texture2D spriteTex = sprite.texture;
                    Rect rect = sprite.textureRect;
                    Color[] tilePixels = spriteTex.GetPixels(
                        (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height
                    );

                    // Blit onto preview texture (bottom-left origin)
                    int px = x * tilePixelSize;
                    int py = y * tilePixelSize;
                    for (int dx = 0; dx < rect.width; dx++)
                    {
                        for (int dy = 0; dy < rect.height; dy++)
                        {
                            int srcIdx = dx + dy * (int)rect.width;
                            int dstX = px + dx;
                            int dstY = py + dy;

                            if (dstX < texWidth && dstY < texHeight)
                            {
                                preview.SetPixel(dstX, dstY, tilePixels[srcIdx]);
                            }
                        }
                    }
                }
            }
        }

        preview.Apply();
        return preview;
    }


}
