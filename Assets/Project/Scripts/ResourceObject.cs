using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObject : MonoBehaviour
{
    [SerializeField] private int resourceAmount;
    [SerializeField] private int amountOfHits;
    [SerializeField] private float hitScale = 0.8f;
    [SerializeField] private float hitSmoothness = 10.0f;

    private int hits;
    private float targetScale;

    // Start is called before the first frame update
    void Start()
    {
        targetScale = 1;
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
    public int Collect() {
        hits++;
        transform.localScale = Vector3.one * hitScale;
        if (hits >= amountOfHits) {
            Destroy(gameObject, 1.0f);
            targetScale = 0;
            return resourceAmount;
        }
        return 0;
    }
}
