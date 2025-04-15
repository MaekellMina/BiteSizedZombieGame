using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HouseRoofFader))]
public class HouseRoofFaderEditor : Editor
{
    void OnSceneGUI()
    {
        HouseRoofFader fader = (HouseRoofFader)target;

        // Get current center in world space
        Vector3 currentCenter = fader.HouseBounds.position + (Vector3)fader.HouseBounds.size / 2f;
        currentCenter += new Vector3(0.5f, 0.5f, 0f); // Tile center offset

        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.PositionHandle(currentCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(fader, "Move House Bounds");

            // Convert to new BoundsInt position by offsetting half the size
            Vector3Int newMin = Vector3Int.RoundToInt(newCenter - (Vector3)fader.HouseBounds.size / 2f);
            var newBounds = new BoundsInt(newMin, fader.HouseBounds.size);
            SerializedProperty boundsProp = serializedObject.FindProperty("houseBounds");
            boundsProp.FindPropertyRelative("m_Position.x").intValue = newBounds.position.x;
            boundsProp.FindPropertyRelative("m_Position.y").intValue = newBounds.position.y;
            boundsProp.FindPropertyRelative("m_Position.z").intValue = newBounds.position.z;
            serializedObject.ApplyModifiedProperties();
        }

        // Optional: draw wire cube for visualization
        Handles.color = Color.green;
        Handles.DrawWireCube(fader.HouseBounds.position + (Vector3)fader.HouseBounds.size / 2f, fader.HouseBounds.size);
    }
}
