using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeLayoutManager : MonoBehaviour
{
    [System.Serializable]
    public class TreeData
    {
        public Vector3 localPosition;
    }


    [Header("requires RandomTreeSpawner")]
    public GameObject treePrefab; 
    public Transform parentTransform;
    [Space]
    [SerializeField] private List<TreeData> savedTrees = new List<TreeData>();


    [Button]

    public void SaveTreeLayout()
    {
        SaveLayout(parentTransform);
    }

    [Button]
    public void LoadTreeLayout()
    {
        LoadLayout(parentTransform);
    }

    [Button]
    public void ClearTrees()
    {
        foreach (Transform child in parentTransform)
        {
            DestroyImmediate(child.gameObject);
        }

        Debug.Log($"Cleared tree(s).");
    }
    public void SaveLayout(Transform root)
    {
        savedTrees.Clear();
        var spawners = root.GetComponentsInChildren<RandomTreeSpawner>();

        foreach (var spawner in spawners)
        {
            Vector3 localPos = root.InverseTransformPoint(spawner.transform.position);
            savedTrees.Add(new TreeData { localPosition = localPos });
        }

        Debug.Log($"Saved {savedTrees.Count} tree(s).");
    }

   
    public void LoadLayout(Transform newRoot)
    {      
        foreach (Transform child in newRoot)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (var treeData in savedTrees)
        {
            Vector3 worldPos = newRoot.TransformPoint(treeData.localPosition);
            GameObject tree = Instantiate(treePrefab, worldPos, Quaternion.identity, newRoot);
            var spawner = tree.GetComponent<RandomTreeSpawner>();
            spawner.RandomizeSetUp(); // Re-randomize tree sprite
        }

        Debug.Log($"Loaded {savedTrees.Count} tree(s).");
    }
}
