using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool opensInwards = true;
    [SerializeField] private float openingSpeed = 2.0f;

    private bool isOpen;
    private float targetAngle;
    private float initialAngle;

    // Start is called before the first frame update
    void Start()
    {
        // Interact();
        initialAngle = transform.localRotation.eulerAngles.y;
        targetAngle = initialAngle;
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("targetAngle: " + targetAngle);
        Quaternion smoothRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, targetAngle, 0), openingSpeed * Time.deltaTime);
        transform.localRotation = smoothRotation;
    }

    public void Interact() {
        isOpen = !isOpen;
        if (isOpen) {
            targetAngle = initialAngle + 90 * (opensInwards ? -1 : 1);
        } else {
            targetAngle = initialAngle;
        }
    }
}
