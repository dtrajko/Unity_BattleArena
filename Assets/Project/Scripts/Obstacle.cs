using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private int cost;

    private Collider obstacleCollider;

    public int Cost {
        get {
            return cost;
        }
    }

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() { }

    void Awake() {
        obstacleCollider = GetComponentInChildren<Collider>();
        // Start with the obstacle collider disabled
        obstacleCollider.enabled = false;
    }

    public void Place() {
        // Enable the collider
        obstacleCollider.enabled = true;
    }
}
