using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Obsolete]
public class ResourceObject : NetworkBehaviour, IDamageable
{
    [SerializeField] private int resourceAmount = 10;
    [SerializeField] private float amountOfHits = 5;
    [SerializeField] private float hitScale = 0.8f;
    [SerializeField] private float hitSmoothness = 10.0f;

    private float hits;
    private float targetScale;
    private Health health;

    public float HealthValue { get { return health.Value; } }
    public int ResourceAmount { get { return resourceAmount; } }

    // Start is called before the first frame update
    void Start()
    {
        targetScale = 1;
        health = GetComponent<Health>();
        health.Value = amountOfHits;
        health.OnHealthChanged += OnHealthChanged;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(
            Mathf.Lerp(transform.localScale.x, targetScale, Time.deltaTime * hitSmoothness),
            Mathf.Lerp(transform.localScale.y, targetScale, Time.deltaTime * hitSmoothness),
            Mathf.Lerp(transform.localScale.z, targetScale, Time.deltaTime * hitSmoothness)
        );
    }

    public int Damage(float amount)
    {
        health.Damage(amount);
        if (health.Value < 0.01f) {
            return (int)health.Value;
        }
        else return 0;
    }

    public void OnHealthChanged(float newHealth)
    {
        transform.localScale = Vector3.one * hitScale;

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
