using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageVolume : MonoBehaviour
{
    List<IDamageable> targets;

    public int damage;

    void Awake()
    {
        targets = new List<IDamageable>();
    }

    void FixedUpdate()
    {
        foreach (IDamageable health in targets) health.TakeDamage(damage);
    }

    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<IDamageable>();
        if (health != null) targets.Add(health);
    }

    void OnTriggerExit(Collider other)
    {
        var health = other.GetComponent<IDamageable>();
        if (health != null) targets.Remove(health);
    }
}
