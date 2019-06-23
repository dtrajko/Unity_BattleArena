using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private GameObject target = null;
    [SerializeField] private GameObject rotationAnchorObject = null;

    [SerializeField] private Vector3 translationOffset = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 followOffset = new Vector3(0, 0.4f, -6.0f);

    private float verticalRotationAngle;

    // Start is called before the first frame update
    void Start()
    {
        // translationOffset = new Vector3(0, target.GetComponent<CapsuleCollider>().height * 1.0f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // Make the camera look at the target
        float yAngle = target.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, yAngle, 0);

        transform.position = target.transform.position + rotation * followOffset;
        transform.LookAt(target.transform.position + translationOffset);

        // Make the camera look up or down
        verticalRotationAngle += Input.GetAxis("Mouse Y");
        transform.RotateAround(target.transform.position, rotationAnchorObject.transform.right, -verticalRotationAngle);
    }
}
