using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SpriteImporter : EditorWindow
{
    enum PivotPreset
    {
        Center, Bottom, Top, Left, Right,
        TopLeft, TopRight, BottomLeft, BottomRight, Custom
    }

    List<string> folderNames = new List<string>();
    int pixelsPerUnit = 16;
    FilterMode filterMode = FilterMode.Point;
    PivotPreset selectedPivot = PivotPreset.Center;
    Vector2 customPivot = new Vector2(0.5f, 0.5f);
    TextureImporterCompression compression = TextureImporterCompression.Uncompressed;
    TextureImporterFormat format = TextureImporterFormat.RGBA32;

    TextAsset folderListFile;  // Field for dragging and dropping the text file
    bool showFolders = true;   // Flag to control the foldout visibility

    [MenuItem("Tools/Sprite Importer (Single)")]
    public static void ShowWindow()
    {
        GetWindow<SpriteImporter>("Sprite Importer (Single)");
    }

    void OnGUI()
    {
        GUILayout.Label("Sprite Importer (Single Mode)", EditorStyles.boldLabel);

        // Field for selecting the TextAsset (drag & drop the text file here)
        GUILayout.Label("Folder List Text File (Drag & Drop):");
        folderListFile = (TextAsset)EditorGUILayout.ObjectField(folderListFile, typeof(TextAsset), false);

        GUILayout.Space(10);

        // Folder selection in a foldout
        showFolders = EditorGUILayout.Foldout(showFolders, "Folders (under Assets/Resources or anywhere):");
        if (showFolders)
        {
            int removeIndex = -1;
            for (int i = 0; i < folderNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                folderNames[i] = EditorGUILayout.TextField(folderNames[i]);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIndex >= 0) folderNames.RemoveAt(removeIndex);
            if (GUILayout.Button("+ Add Folder")) folderNames.Add("");
        }

        GUILayout.Space(10);

        // Settings
        pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", pixelsPerUnit);
        filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", filterMode);
        compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", compression);
        format = (TextureImporterFormat)EditorGUILayout.EnumPopup("Format", format);

        selectedPivot = (PivotPreset)EditorGUILayout.EnumPopup("Pivot Preset", selectedPivot);
        if (selectedPivot == PivotPreset.Custom)
        {
            customPivot = EditorGUILayout.Vector2Field("Custom Pivot (0-1)", customPivot);
            customPivot.x = Mathf.Clamp01(customPivot.x);
            customPivot.y = Mathf.Clamp01(customPivot.y);
        }

        GUILayout.Space(10);

        // Buttons to process files
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Process All Textures"))
        {
            ProcessTextures();
        }

        if (GUILayout.Button("Load Folders From File"))
        {
            LoadFoldersFromFile();
        }

        GUILayout.EndHorizontal();
    }

    void ProcessTextures()
    {
        foreach (var folder in folderNames)
        {
            string fullPath = Path.Combine("Assets", folder.TrimStart('/').TrimStart('\\'));
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"Folder not found: {fullPath}");
                continue;
            }

            string[] texturePaths = Directory.GetFiles(fullPath, "*.png", SearchOption.AllDirectories);

            foreach (var texPath in texturePaths)
            {
                string assetPath = texPath.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = filterMode;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.textureCompression = compression;

                // Proper pivot setup for SINGLE sprites
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

                settings.spritePivot = GetPivotPoint();
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spriteMode = 1;

                importer.SetTextureSettings(settings);

                // Optional: set format/platform settings
                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.name = "Standalone";
                platformSettings.overridden = true;
                platformSettings.format = TextureImporterFormat.RGBA32;
                platformSettings.textureCompression = compression;

                importer.SetPlatformTextureSettings(platformSettings);

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"Updated: {assetPath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("All textures processed.");
    }

    void LoadFoldersFromFile()
    {
        if (folderListFile == null)
        {
            Debug.LogError("Please assign a folder list text file.");
            return;
        }

        folderNames.Clear();

        string[] lines = folderListFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            folderNames.Add(line.Trim());
        }

        Debug.Log($"Loaded folders from {folderListFile.name}");
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
        return new Vector2(0.5f, 0.5f);
    }
}
