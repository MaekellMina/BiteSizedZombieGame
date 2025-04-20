using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LivingEntity : MonoBehaviour, IDamageable
{
    [SerializeField]
    protected float startingHealth;
    protected float health;

    public bool dead;

    public UnityEvent e_OnDeath = new UnityEvent();

    public virtual void Start()
    {
        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit2D hit)
    {
        // TODO: some stuff here with hit var like hit pos etc
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    public void Die()
    {
        dead = true;

        gameObject.SetActive(false);    //temp... TODO: death animation, etc
        e_OnDeath?.Invoke();
    }
}
