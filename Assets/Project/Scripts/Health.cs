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

    // Health
    private const float defaultHealth = 100;
    [SyncVar(hook = "OnHealthSynced")]
    [System.Obsolete]
    private float health = defaultHealth;

    // Properties
    public float Value { get { return health; } set { health = value; } }

    public void Damage(float amount) {
        health -= amount;
        if (health < 0) health = 0;
    }

    private void OnHealthSynced(float newHealth) {
        if (OnHealthChanged != null)
        { // If somebody subscribed to this event
            OnHealthChanged(health);
        }
    }
}
