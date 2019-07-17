using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Obsolete]
public class Health : NetworkBehaviour
{
    // Event
    public delegate void HealthChangedHandler(float health);
    public event HealthChangedHandler OnHealthChanged;

    private const float defaultHealth = 100;

    [SyncVar]
    [System.Obsolete]
    private float health = defaultHealth;

    public void Damage(float amount) {
        health -= amount;
        if (health < 0) health = 0;

        if (OnHealthChanged != null) { // If somebody subscribed to this event
            OnHealthChanged(health);
        }
    }
}
