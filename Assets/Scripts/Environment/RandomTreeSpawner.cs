using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTreeSpawner : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer tressRenderer;
    [Space]
    [SerializeField]
    List<Sprite> PossibleTrees = new List<Sprite>();

    public void RandomizeSetUp()
    {
        tressRenderer.sprite = PossibleTrees[Random.Range(0, PossibleTrees.Count)];


    }
}
