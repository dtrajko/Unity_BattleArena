using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IDamageable
{
    [SerializeField] private float health;
    [SerializeField] private int cost;
    [SerializeField] private float hitSmoothness;

    private Collider[] obstacleColliders;
    private Renderer obstacleRenderer;
    private int targetScale = 1;

    public int Cost {
        get {
            return cost;
        }
    }

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {
        transform.localScale = new Vector3(
            Mathf.Lerp(transform.localScale.x, targetScale, hitSmoothness * Time.deltaTime),
            Mathf.Lerp(transform.localScale.y, targetScale, hitSmoothness * Time.deltaTime),
            Mathf.Lerp(transform.localScale.z, targetScale, hitSmoothness * Time.deltaTime)
        );
    }

    void Awake() {
        obstacleColliders = GetComponentsInChildren<Collider>();
        // Start with the obstacle collider disabled
        foreach (Collider obstacleCollider in obstacleColliders) {
            obstacleCollider.enabled = false;
        }

        // Work with transparency
        obstacleRenderer = GetComponentInChildren<Renderer>();
        obstacleRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

    }

    public void Place() {
        // Enable the collider
        foreach (Collider obstacleCollider in obstacleColliders)
        {
            obstacleCollider.enabled = true;
        }

        // Make the obstacle opaque
        obstacleRenderer.material.color = Color.white;
    }

    public void Hit() {
    }

    public int Damage(float amount)
    {
        transform.localScale = Vector3.one * 0.8f;
        health -= amount;
        if (health <= 0)
        {
            targetScale = 0;
            Destroy(gameObject, 0.05f);
        }

        return 0;
    }
}
