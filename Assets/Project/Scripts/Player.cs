using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Focal Point Variables")]
    [SerializeField] private GameObject focalPoint = null;
    [SerializeField] private float focalDistance = -0.3f;
    [SerializeField] private float focalSmoothness = 4f;
    [SerializeField] private KeyCode changeFocalSideKey = KeyCode.Q;

    [Header("Interaction")]
    [SerializeField] private GameCamera gameCamera;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 6f;

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
        float smoothX = Mathf.Lerp(focalPoint.transform.localPosition.x, targetX, focalSmoothness * Time.deltaTime);
        focalPoint.transform.localPosition = new Vector3(
            smoothX,
            focalPoint.transform.localPosition.y,
            focalPoint.transform.localPosition.z
        );

        // Interaction logic
        // #if UNITY_EDITOR
        // Draw interaction line
        Debug.DrawLine(gameCamera.transform.position, gameCamera.transform.position + gameCamera.transform.forward * interactionDistance, Color.green);
        // #endif
        if (Input.GetKeyDown(interactionKey)) {
            RaycastHit hit;
            bool isHit = Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out hit, (int) interactionDistance);
            if (isHit) {
                // Debug.Log("Hit object: " + hit.transform.name);
                if (hit.transform.GetComponent<Door>()) {
                    hit.transform.GetComponent<Door>().Interact();
                }
            }
        }
    }
}
