using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormObjectTop : MonoBehaviour
{
    [SerializeField] private float initialDistance;
    [SerializeField] private float shrinkSmoothness;

    private float targetDistance;

    // Update is called once per frame
    void Update()
    {
        if (targetDistance <= 0) return;

        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            new Vector3(
                targetDistance / (initialDistance + 30),
                transform.localScale.y,
                targetDistance / (initialDistance + 30)),
            Time.deltaTime * shrinkSmoothness
        );
    }

    public void MoveToDistance(float distance)
    {
        targetDistance = distance;
    }

}
