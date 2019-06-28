using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool opensInwards;
    [SerializeField] private float openingSpeed = 2.0f;

    private bool isOpen;
    private float targetAngle;

    // Start is called before the first frame update
    void Start()
    {
        Interact();
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion smoothRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, targetAngle, 0), openingSpeed * Time.deltaTime);
        transform.localRotation = smoothRotation;
    }

    public void Interact() {
        isOpen = !isOpen;
        if (isOpen) {
            targetAngle = 90.0f * (opensInwards ? 1 : -1);
        } else {
            targetAngle = 0;
        }
    }
}
