using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    public int maxHealth;
    int curHealth;

    public float invincibilitySeconds;
    float invincibilityTime;

    public Action<int> onDamaged = delegate { };
    public Action onKilled = delegate { };

    void Awake()
    {
        curHealth = maxHealth;    
    }

    void Update()
    {
        if(invincibilityTime > 0) invincibilityTime -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (invincibilityTime > 0) return;
        invincibilityTime = invincibilitySeconds;

        curHealth -= damage;
        if (curHealth <= 0) Kill();
        else onDamaged?.Invoke(curHealth);
    }

    public void Kill()
    {
        onKilled?.Invoke();
    }
}
