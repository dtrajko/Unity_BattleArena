using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Vector3 translationOffset;
    [SerializeField] private Vector3 followOffset = new Vector3(0, 1.5f, -3.5f);

    // Start is called before the first frame update
    void Start()
    {
        translationOffset = new Vector3(0, target.GetComponent<CapsuleCollider>().height * 1.0f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // 0 - 180 - 360
        float yAngle = target.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, yAngle, 0);

        transform.position = target.transform.position + rotation * followOffset;
        transform.LookAt(target.transform.position + translationOffset);
    }
}
