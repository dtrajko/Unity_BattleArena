using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float speed = 15.0f;

    private Rigidbody rocketRigidbody;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() { }

    void Awake() {
        rocketRigidbody = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction) {
        transform.forward = direction;
        rocketRigidbody.velocity = direction * speed;
    }
}
