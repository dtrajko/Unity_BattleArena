using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObject : MonoBehaviour
{
    [SerializeField] private int resourceAmount;
    [SerializeField] private int amountOfHits;

    private int hits;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Interact() {
        hits++;
        if (hits >= amountOfHits) {
            Destroy(gameObject);
        }
    }
}
