using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using System.IO;
using System.Collections.Generic;

public class AutotileRuleTileCreator : EditorWindow
{
    private Texture2D tilesetTexture;
    private int tileSize = 32;
    private string saveFolder = "Assets/Tiles/GeneratedTiles";

    [MenuItem("Tools/Autotile/RuleTile Generator")]
    public static void ShowWindow()
    {
        GetWindow<AutotileRuleTileCreator>("Autotile RuleTile Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Autotile Setup", EditorStyles.boldLabel);
        tilesetTexture = (Texture2D)EditorGUILayout.ObjectField("Tileset", tilesetTexture, typeof(Texture2D), false);
        tileSize = EditorGUILayout.IntField("Tile Size (px)", tileSize);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        if (GUILayout.Button("Generate RuleTile"))
        {
            if (tilesetTexture == null)
            {
                Debug.LogWarning("No tileset selected.");
                return;
            }

            CreateRuleTileFromTileset();
        }
    }

    private void CreateRuleTileFromTileset()
    {
        string path = AssetDatabase.GetAssetPath(tilesetTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (!importer.isReadable || !importer.spriteImportMode.Equals(SpriteImportMode.Multiple))
        {
            importer.isReadable = true;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            List<SpriteMetaData> newData = new List<SpriteMetaData>();
            int w = tilesetTexture.width / tileSize;
            int h = tilesetTexture.height / tileSize;

            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++)
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.name = $"{tilesetTexture.name}_{x}_{y}";
                    smd.rect = new Rect(x * tileSize, y * tileSize, tileSize, tileSize);
                    smd.pivot = new Vector2(0.5f, 0.5f);
                    smd.alignment = (int)SpriteAlignment.Center;
                    newData.Add(smd);
                }
            }

            importer.spritesheet = newData.ToArray();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        List<Sprite> spriteList = new List<Sprite>();
        foreach (Object o in sprites)
        {
            if (o is Sprite sprite)
                spriteList.Add(sprite);
        }

        spriteList.Sort((a, b) => a.name.CompareTo(b.name));

        RuleTile ruleTile = ScriptableObject.CreateInstance<RuleTile>();
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>();

        for (int i = 0; i < spriteList.Count && i < RulePatterns.Length; i++)
        {
            var tilingRule = new RuleTile.TilingRule();
            tilingRule.m_Sprites[0] = spriteList[i];
            tilingRule.m_NeighborPositions = new List<Vector3Int>();
            tilingRule.m_Neighbors = new List<int>();

            foreach (var kvp in RulePatterns[i])
            {
                tilingRule.m_NeighborPositions.Add(kvp.Key);
                tilingRule.m_Neighbors.Add(kvp.Value);
            }

            tilingRule.m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single;
            ruleTile.m_TilingRules.Add(tilingRule);

        }

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string tilePath = Path.Combine(saveFolder, tilesetTexture.name + "_RuleTile.asset");
        AssetDatabase.CreateAsset(ruleTile, tilePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("RuleTile generated and saved to: " + tilePath);
    }

    // 15 patterns for 3x3 autotiles (like RPG Maker)
    private static readonly Dictionary<Vector3Int, int>[] RulePatterns = new Dictionary<Vector3Int, int>[]
    {
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 1}, {Vector3Int.down, 1}, {Vector3Int.left, 1}}, // full surrounded
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 1}, {Vector3Int.down, 1}, {Vector3Int.left, 1}},
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 0}, {Vector3Int.down, 1}, {Vector3Int.left, 1}},
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 1}, {Vector3Int.down, 0}, {Vector3Int.left, 1}},
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 1}, {Vector3Int.down, 1}, {Vector3Int.left, 0}},
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 0}, {Vector3Int.down, 1}, {Vector3Int.left, 1}},
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 0}, {Vector3Int.down, 0}, {Vector3Int.left, 1}},
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 1}, {Vector3Int.down, 0}, {Vector3Int.left, 0}},
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 1}, {Vector3Int.down, 1}, {Vector3Int.left, 0}},
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 0}, {Vector3Int.down, 0}, {Vector3Int.left, 1}},
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 0}, {Vector3Int.down, 0}, {Vector3Int.left, 0}},
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 1}, {Vector3Int.down, 0}, {Vector3Int.left, 0}},
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 0}, {Vector3Int.down, 1}, {Vector3Int.left, 0}},
        new() {{Vector3Int.up, 0}, {Vector3Int.right, 0}, {Vector3Int.down, 0}, {Vector3Int.left, 0}}, // isolated
        new() {{Vector3Int.up, 1}, {Vector3Int.right, 1}, {Vector3Int.down, 1}, {Vector3Int.left, 1}}, // fallback/override
    };
}
