using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 1;
    public float lifetime = 1;
    public LayerMask collisionMask;

    float speed = 10;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.right * moveDistance);  //instead of Vector forward, use Vector right since it's 2d

        lifetime -= Time.deltaTime;

        if (lifetime < 0)
            Destroy(gameObject);
    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.right);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, moveDistance, collisionMask);
        if (hit.collider != null)
            OnHitObject(hit);
    }

    void OnHitObject(RaycastHit2D hit)
    {
        IDamageable damageableObj = hit.collider.GetComponentInParent<IDamageable>();
        if (damageableObj != null)
            damageableObj.TakeHit(damage, hit);
        Destroy(gameObject);
    }
}
