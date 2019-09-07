using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Rocket : NetworkBehaviour
{
    [SerializeField] private float speed = 15.0f;
    [SerializeField] private float lifetime;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRange;
    [SerializeField] private float explosionDamage;

    private Rigidbody rocketRigidbody;
    private float timer;

    // Start is called before the first frame update
    void Start() {}

    void Awake() {
        rocketRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) {
            Explode();
        }
    }

    public void Shoot(Vector3 direction) {
        transform.forward = direction;
        rocketRigidbody.velocity = direction * speed;
    }

    public void OnTriggerEnter(Collider otherCollider) {
        Explode();
    }

    private void Explode() {
        if (!isServer) return;

        CmdAddExplosion();
        Destroy(gameObject);
    }

    [Command]
    private void CmdAddExplosion() {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = transform.position;
        NetworkServer.Spawn(explosion);
        explosion.GetComponent<Explosion>().Explode(explosionRange, explosionDamage);
    }
}

