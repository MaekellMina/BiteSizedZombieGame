using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RandomTreeSpawner : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer tressRenderer;

    [SerializeField]
    private List<Sprite> PossibleTrees = new List<Sprite>();

    [Header("Transparency Settings")]
    [Range(0f, 1f)] public float transparentAlpha = 0.4f;
    private float originalAlpha = 1f;

    Tween Fader;
    private void Awake()
    {
        if (tressRenderer == null)
            tressRenderer = GetComponent<SpriteRenderer>();

        originalAlpha = tressRenderer.color.a;
    }

    public void RandomizeSetUp()
    {
        tressRenderer.sprite = PossibleTrees[Random.Range(0, PossibleTrees.Count)];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetTransparency(transparentAlpha);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetTransparency(originalAlpha);
        }
    }

    private void SetTransparency(float alpha)
    {
        Color color = tressRenderer.color;
        color.a = alpha;

        if (Fader.isAlive)
            Fader.Stop();

        Fader = Tween.Color(tressRenderer, color, .2f, Ease.Linear).OnComplete(() => tressRenderer.color = color);

       
    }
}
