using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForegroundTransparencyController : MonoBehaviour
{
    [Header("Objects")]
    public List<SpriteRenderer> foregrounds;
    public List<SpriteRenderer> backgrounds;

    [Header("Opacity Settings")]
    [Range(0f, 1f)] public float minOpacity = 0.2f;
    [Range(0f, 1f)] public float maxOpacity = 1f;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0f;

    [Header("Advanced Options")]
    public bool checkPlayerVisibility = false;
    public LayerMask visibilityMask;

    // New: cache colliders for each foreground
    private Dictionary<SpriteRenderer, Collider2D[]> _fgColliders = new Dictionary<SpriteRenderer, Collider2D[]>();

    private Coroutine fadeCoroutine;
    private int playerCount = 0;
    private bool isPlayerVisible = true;

    private void Awake()
    {
        // Build collider cache
        foreach (var sr in foregrounds)
        {
            if (sr == null) continue;
            _fgColliders[sr] = sr.GetComponents<Collider2D>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerCount++;
        if (checkPlayerVisibility)
            StartCoroutine(CheckPlayerVisibility(other.transform));

        StartFade(minOpacity);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerCount = Mathf.Max(0, playerCount - 1);
        if (playerCount == 0)
            StartFade(maxOpacity);
    }

    void StartFade(float targetOpacity)
    {
        //if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        //fadeCoroutine = StartCoroutine(FadeForegrounds(targetOpacity));

        var originalAlphas = new Dictionary<SpriteRenderer, Color>();
        foreach (var sr in foregrounds)
            if (sr != null)
                originalAlphas[sr] = sr.color;

        foreach (var sr in foregrounds)
        {
            if (sr == null) continue;
            Color startColor = originalAlphas[sr];
            Color targetColor = new Color(startColor.r, startColor.b, startColor.g, targetOpacity);

            Tween.Custom(startColor, targetColor, fadeDuration, (x) => sr.color = x).OnComplete(() => updateCollider(sr,targetColor,targetOpacity));
        }
    }

    void updateCollider(SpriteRenderer sr,Color targetColor,float targetOpacity)
    {
        sr.color = targetColor;
        // enable/disable colliders at endpoints
        if (_fgColliders.TryGetValue(sr, out var cols))
        {
            bool passable = Mathf.Approximately(targetOpacity, minOpacity);
            foreach (var col in cols)
                if (col != null)
                    col.enabled = !passable;
        }
    }

    IEnumerator FadeForegrounds(float targetOpacity)
    {
        if (fadeDelay > 0f) yield return new WaitForSeconds(fadeDelay);

        float time = 0f;
        // capture original alphas
        var originalAlphas = new Dictionary<SpriteRenderer, float>();
        foreach (var sr in foregrounds)
            if (sr != null)
                originalAlphas[sr] = sr.color.a;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            foreach (var sr in foregrounds)
            {
                if (sr == null) continue;
                float startA = originalAlphas[sr];
                float newA = Mathf.Lerp(startA, targetOpacity, t);
                Color c = sr.color;
                c.a = newA;
                sr.color = c;
            }

            yield return null;
        }

        // finalize
        foreach (var sr in foregrounds)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = targetOpacity;
            sr.color = c;

            // enable/disable colliders at endpoints
            if (_fgColliders.TryGetValue(sr, out var cols))
            {
                bool passable = Mathf.Approximately(targetOpacity, minOpacity);
                foreach (var col in cols)
                    if (col != null)
                        col.enabled = !passable;
            }
        }

        fadeCoroutine = null;
    }

    IEnumerator CheckPlayerVisibility(Transform player)
    {
        while (playerCount > 0)
        {
            Vector2 direction = player.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, direction.magnitude, visibilityMask);
            isPlayerVisible = (hit.collider == null);
            yield return new WaitForSeconds(0.25f);
        }
    }
}
