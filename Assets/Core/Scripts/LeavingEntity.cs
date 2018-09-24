using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingEntity : MonoBehaviour, IDamageable
{

    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;


    protected virtual void Start()
    {
        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        //Do some stuff here with hit var
        TakeDamage(damage);
    }

    protected void Died()
    {
        dead = true;
        if (OnDeath != null)
            OnDeath();
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
            Died();
    }
}
