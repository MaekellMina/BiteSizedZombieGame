using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject flashHolder;
    public Sprite[] flashSprites;
    public SpriteRenderer spriteRenderer;

    public float flashTime;

    private void Start()
    {
        Deactivate();
    }

    public void Activate()
    {
        flashHolder.SetActive(true);

        int flashSpriteIndex = Random.Range(0, flashSprites.Length);
        spriteRenderer.sprite = flashSprites[flashSpriteIndex];

        Invoke("Deactivate", flashTime);
    }

    public void Deactivate()
    {
        flashHolder.SetActive(false);
    }
}
