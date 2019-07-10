using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    [SerializeField] private float distanceFromFloor = 2.0f;
    [SerializeField] private float bounceAmplitude = 0.125f;
    [SerializeField] private float bounceSpeed = 2f;

    private float bounceAngle = 60.0f;
    private GameObject floatingObject;

    // Start is called before the first frame update
    void Start()
    {
        floatingObject = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        bool isHit = Physics.Raycast(transform.position, Vector3.down, out hit);
        if (isHit) {

            // Get the ground position
            Vector3 targetPosition = hit.point;

            // Move the enemy up
            targetPosition = new Vector3(
                targetPosition.x,
                targetPosition.y + distanceFromFloor,
                targetPosition.z
            );

            // Swing the enemy
            bounceAngle += Time.deltaTime * bounceSpeed;
            float offset = Mathf.Cos(bounceAngle) * bounceAmplitude;
            targetPosition = new Vector3(
                targetPosition.x,
                targetPosition.y + offset,
                targetPosition.z
            );

            // Apply the position
            transform.position = targetPosition;
        }
    }
}
