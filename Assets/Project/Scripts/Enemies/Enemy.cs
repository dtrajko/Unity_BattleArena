using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Obsolete]
public class Enemy : NetworkBehaviour, IDamageable
{
    [SerializeField] private float initialHealth = 25.0f;
    [SerializeField] private float hitSmoothness = 10.0f;
    [SerializeField] protected float damage = 1.0f;

    protected Rigidbody enemyRigidbody;
    protected float damageScale = 0.8f;
    private float targetScale = 1.0f;

    private Health health;

    void Awake() {
        enemyRigidbody = transform.GetComponent<Rigidbody>();

        health = GetComponent<Health>();
        health.Value = initialHealth;
        health.OnHealthChanged += OnHealthChanged;
    }

    // Update is called once per frame
    protected void Update()
    {
        transform.localScale = new Vector3(
            Mathf.Lerp(transform.localScale.x, targetScale, hitSmoothness * Time.deltaTime),
            Mathf.Lerp(transform.localScale.y, targetScale, hitSmoothness * Time.deltaTime),
            Mathf.Lerp(transform.localScale.z, targetScale, hitSmoothness * Time.deltaTime)
        );
    }
    public int Damage(float amount)
    {
        health.Damage(amount);
        return 0;
    }

    public void OnHealthChanged(float newHealth)
    {
        transform.localScale = Vector3.one * damageScale;

        if (newHealth < 0.01f)
        {
            targetScale = 0;

            if (isServer)
            {
                CmdDestroy();
            }
        }
    }

    [Command]
    [System.Obsolete]
    public void CmdDestroy()
    {
        Destroy(gameObject, 1.0f);
    }
}
