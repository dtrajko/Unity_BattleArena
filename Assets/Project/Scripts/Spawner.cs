using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 positionOffset;

    public GameObject Prefab {
        get {
            return prefab;
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        GameObject instance = Instantiate(prefab, transform.position + positionOffset, transform.rotation); // 4th param: transform.parent
        NetworkServer.Spawn(instance);
        // Destroy(gameObject);
    }
}
