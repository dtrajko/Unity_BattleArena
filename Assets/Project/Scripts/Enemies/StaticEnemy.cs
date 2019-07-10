using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float health;
    [SerializeField] private float hitSmoothness;

    private float targetScale = 1.0f;

    // Update is called once per frame
    void Update()
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
            transform.localScale = Vector3.one * 0.9f;
        }
        health -= amount;

        if (health <= 0) {
            targetScale = 0;
            Destroy(gameObject, 1.0f);
        }
        return 0;
    }
}
