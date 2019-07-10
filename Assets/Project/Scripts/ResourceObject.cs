using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceObject : MonoBehaviour, IDamageable
{
    [SerializeField] private int resourceAmount = 10;
    [SerializeField] private float amountOfHits = 5;
    [SerializeField] private float hitScale = 0.8f;
    [SerializeField] private float hitSmoothness = 10.0f;

    private float hits;
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

    public int Damage(float amount)
    {
        hits += amount;
        transform.localScale = Vector3.one * hitScale;
        if (hits >= amountOfHits)
        {
            Destroy(gameObject, 1.0f);
            targetScale = 0;
            return resourceAmount;
        }

        return 0;
    }
}
