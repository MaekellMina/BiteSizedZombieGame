using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EjectedShell : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public float forceMin;
    public float forceMax;

    float lifetime = 0.4f;
    float fadetime = 0.2f;

    private void Start()
    {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.up * force);
        myRigidbody.AddTorque(force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);
        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        Color initialColor = renderer.color;
        
        while(percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            renderer.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }

        Destroy(gameObject);
    }
}
