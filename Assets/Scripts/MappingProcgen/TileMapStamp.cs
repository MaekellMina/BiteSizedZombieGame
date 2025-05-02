using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Tilemap/Stamp")]
[System.Serializable]
public class TilemapStamp : ScriptableObject
{
    public List<LayerData> layers = new List<LayerData>();

    [System.Serializable]
    public class LayerData
    {
        public string layerName;
        public Vector3Int size;
        public TileBase[] tiles; // Stored row by row
    }
}
