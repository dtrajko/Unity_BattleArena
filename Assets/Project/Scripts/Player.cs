using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Focal Point Variables")]
    [SerializeField] private GameObject focalPoint = null;
    [SerializeField] private float focalDistance = 0.65f;
    [SerializeField] private KeyCode changeFocalSideKey = KeyCode.Q;

    private bool isFocalPointOnLeft = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(changeFocalSideKey)) {
            isFocalPointOnLeft = !isFocalPointOnLeft;
        }

        float targetX = focalDistance * (isFocalPointOnLeft ? -1 : 1);
        focalPoint.transform.localPosition = new Vector3(
            targetX,
            focalPoint.transform.localPosition.y,
            focalPoint.transform.localPosition.z
        );
    }
}
