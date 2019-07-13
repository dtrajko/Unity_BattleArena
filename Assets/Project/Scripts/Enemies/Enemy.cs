using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 25.0f;
    [SerializeField] private float hitSmoothness = 10.0f;
    [SerializeField] protected float damage = 1.0f;

    protected Rigidbody enemyRigidbody;
    protected float damageScale = 0.8f;
    private float targetScale = 1.0f;

    void Awake() {
        enemyRigidbody = transform.GetComponent<Rigidbody>();
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
        if (health > 0) {
            transform.localScale = Vector3.one * damageScale;
        }
        health -= amount;

        if (health <= 0) {
            targetScale = 0;
            Destroy(gameObject, 1.0f);
        }
        return 0;
    }
}
