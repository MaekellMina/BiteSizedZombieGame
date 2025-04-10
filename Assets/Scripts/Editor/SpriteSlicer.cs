using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SpriteSlicer : EditorWindow
{
    enum PivotPreset { Center, Bottom, Top, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight, Custom }

    string[] animationNames = new string[]
    {
        "Idle", "Kick", "Attack", "Damage",
        "Walk", "Run", "Push", "Pull",
        "Jump", "Win", "Die", "Sit"
    };

    int columns = 8;
    int rows = 6;
    int cellWidth = 32;
    int cellHeight = 32;
    int pixelsPerUnit = 16;
    FilterMode selectedFilterMode = FilterMode.Point;

    PivotPreset selectedPivot = PivotPreset.Center;
    Vector2 customPivot = new Vector2(0.5f, 0.5f);

    List<string> folderNames = new List<string>();

    // Added new fields for texture import settings
    int maxTextureSize = 2048;
    TextureImporterFormat textureFormat = TextureImporterFormat.RGBA32;

    [MenuItem("Tools/Sprite Sheet Slicer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSlicer>("Sprite Sheet Slicer");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto-Slice All Sprite Sheets", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Enter folder names under Resources/Sprites/ it will automatically look for textures under the Color# folder", MessageType.Info);

        int removeIndex = -1;
        for (int i = 0; i < folderNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            folderNames[i] = EditorGUILayout.TextField(folderNames[i]);
            if (GUILayout.Button("X", GUILayout.Width(20)))
                removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
            folderNames.RemoveAt(removeIndex);

        if (GUILayout.Button("+ Add Folder"))
            folderNames.Add("");

        GUILayout.Space(10);
        GUILayout.Label("Texture Import Settings", EditorStyles.boldLabel);
        selectedFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", selectedFilterMode);
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit);
        pixelsPerUnit = Mathf.Max(1, pixelsPerUnit); // Prevent zero or negative

        // Added max texture size field
        maxTextureSize = EditorGUILayout.IntField("Max Texture Size", maxTextureSize);
        maxTextureSize = Mathf.Max(1, maxTextureSize); // Prevent zero or negative

        // Added texture format selection
        textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);

        GUILayout.Space(10);
        GUILayout.Label("Cell Size", EditorStyles.boldLabel);
        cellWidth = EditorGUILayout.IntField("Cell Width", cellWidth);
        cellHeight = EditorGUILayout.IntField("Cell Height", cellHeight);

        GUILayout.Space(10);
        GUILayout.Label("Pivot Settings", EditorStyles.boldLabel);
        selectedPivot = (PivotPreset)EditorGUILayout.EnumPopup("Pivot Preset", selectedPivot);

        if (selectedPivot == PivotPreset.Custom)
        {
            customPivot = EditorGUILayout.Vector2Field("Custom Pivot (0-1)", customPivot);
            customPivot.x = Mathf.Clamp01(customPivot.x);
            customPivot.y = Mathf.Clamp01(customPivot.y);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Process All Found Textures"))
        {
            ProcessAll();
        }
    }

    void ProcessAll()
    {
        foreach (string folderName in folderNames)
        {
            int colorIndex = 1; // Start from 0 unless your folder structure starts from 1
            while (true)
            {
                string relativeFolder = $"Assets/Resources/Sprites/{folderName}/Color{colorIndex}";

                if (!Directory.Exists(relativeFolder))
                {
                    Debug.Log($"{relativeFolder} Does not exist!");
                    break;
                }

                Debug.Log($"Processing: {relativeFolder}");

                string[] files = Directory.GetFiles(relativeFolder, "*.png", SearchOption.TopDirectoryOnly);
                Debug.Log($"Found {files.Length} PNGs.");

                foreach (string filePath in files)
                {
                    // Convert full system path to relative asset path
                    string assetPath = filePath.Replace("\\", "/");

                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (tex != null)
                    {
                        SliceSpriteSheet(tex, assetPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load Texture2D from path: {assetPath}");
                    }
                }

                colorIndex++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All slicing complete!");
    }

    void SliceSpriteSheet(Texture2D spriteSheet, string assetPath)
    {
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = selectedFilterMode;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.isReadable = true;

        // Override texture settings
        importer.maxTextureSize = maxTextureSize;
        importer.textureFormat = textureFormat;

        List<SpriteMetaData> metas = new List<SpriteMetaData>();

        // Define animations and their frame positions (row index from top to bottom)
        //the one on top will have the highest row;
        var slices = new (int row, int startCol, int endCol, string name)[]
        {
        (5, 0, 1, "Idle"),
        (5, 2, 3, "Kick"),
        (5, 4, 5, "Attack"),
        (5, 6, 7, "Damage"),

        (4, 0, 3, "Walk"),
        (4, 4, 7, "Run"),

        (3, 0, 3, "Push"),
        (3, 4, 7, "Pull"),

        (2, 0, 7, "Jump"),

        (1, 0, 3, "Win"),
        (1, 4, 7, "Die"),

        (0, 0, 1, "Sit"),
        };

        foreach (var slice in slices)
        {
            int y = slice.row * cellHeight;

            Debug.Log($"Calculated Y for {slice.name}: {y}");

            for (int x = slice.startCol; x <= slice.endCol; x++)
            {
                SpriteMetaData meta = new SpriteMetaData();
                meta.rect = new Rect(x * cellWidth, y, cellWidth, cellHeight);
                meta.name = $"{slice.name}_{x - slice.startCol}";
                meta.pivot = GetPivotPoint();
                metas.Add(meta);
            }
        }

        importer.spritesheet = metas.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log($"Sliced (Top-Down): {assetPath}");
    }

    Vector2 GetPivotPoint()
    {
        switch (selectedPivot)
        {
            case PivotPreset.Center: return new Vector2(0.5f, 0.5f);
            case PivotPreset.Bottom: return new Vector2(0.5f, 0f);
            case PivotPreset.Top: return new Vector2(0.5f, 1f);
            case PivotPreset.Left: return new Vector2(0f, 0.5f);
            case PivotPreset.Right: return new Vector2(1f, 0.5f);
            case PivotPreset.TopLeft: return new Vector2(0f, 1f);
            case PivotPreset.TopRight: return new Vector2(1f, 1f);
            case PivotPreset.BottomLeft: return new Vector2(0f, 0f);
            case PivotPreset.BottomRight: return new Vector2(1f, 0f);
            case PivotPreset.Custom: return customPivot;
        }
        return new Vector2(0.5f, 0.5f); // fallback
    }
}
