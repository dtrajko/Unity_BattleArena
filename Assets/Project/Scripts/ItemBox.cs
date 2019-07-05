using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private string itemName;
    [SerializeField] private int itemAmount;

    [Header("Visuals")]
    [SerializeField] private float rotationAngle;
    [SerializeField] private float verticalRange;
    [SerializeField] private float verticalSpeed;

    private GameObject floatingObject;
    private float verticalAngle;

    // Start is called before the first frame update
    void Start()
    {
        floatingObject = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        verticalAngle += verticalSpeed * Time.deltaTime;
        floatingObject.transform.Rotate(0, rotationAngle * Time.deltaTime, 0);
        floatingObject.transform.localPosition = new Vector3(
            0, 
            Mathf.Cos(verticalAngle) * verticalRange, 
            0
        );
    }
}
