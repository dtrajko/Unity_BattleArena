using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public enum PlayerTool {
        Pickaxe,
        Grenade,
        None
    }

    [Header("Focal Point Variables")]
    [SerializeField] private GameObject focalPoint = null;
    [SerializeField] private float focalDistance = -0.3f;
    [SerializeField] private float focalSmoothness = 4f;
    [SerializeField] private KeyCode changeFocalSideKey = KeyCode.Q;

    [Header("Interaction")]
    [SerializeField] private GameCamera gameCamera;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 6f;

    [Header("Interface")]
    [SerializeField] private HUDController hud;

    [Header("Gameplay")]
    [SerializeField] private KeyCode toolSwitchKey = KeyCode.Tab;
    [SerializeField] private PlayerTool tool;
    [SerializeField] private float resourceCollectionCooldown = 0.5f;

    private bool isFocalPointOnLeft = true;
    private int resources = 0;
    private float resourceCollectionCooldownTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        hud.Resources = resources;
        tool = PlayerTool.Pickaxe;
        hud.Tool = tool; // PlayerTool: Pickaxe
    }
 
    // Update is called once per frame
    void Update()
    {
        // Update timers
        resourceCollectionCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(changeFocalSideKey))
        {
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
        if (Input.GetKeyDown(interactionKey))
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out hit, (int)interactionDistance);
            if (isHit)
            {
                // Debug.Log("Hit object: " + hit.transform.name);
                if (hit.transform.GetComponent<Door>())
                {
                    hit.transform.GetComponent<Door>().Interact();
                }
            }
        }

        // Tool switch logic
        hud.Tool = tool;
        if (Input.GetKeyDown(toolSwitchKey))
        {
            int currentToolIndex = (int)tool;
            currentToolIndex++;
            if (currentToolIndex == System.Enum.GetNames(typeof(PlayerTool)).Length) {
                currentToolIndex = 0;
            }
            tool = (PlayerTool)currentToolIndex;
            hud.Tool = tool;
        }

        // Tool usage logic
        if (Input.GetAxis("Fire1") > 0) {
            if (tool == PlayerTool.Pickaxe) {
                RaycastHit hit;
                bool isHit = Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out hit, (int)interactionDistance);
                if (isHit)
                {
                    if (resourceCollectionCooldownTimer <= 0
                        && hit.transform.GetComponent<ResourceObject>() != null)
                    {
                        resourceCollectionCooldownTimer = resourceCollectionCooldown;
                        ResourceObject resourceObject = hit.transform.GetComponent<ResourceObject>();
                        int collectedResources = resourceObject.Collect();
                        resources += collectedResources;
                        hud.Resources = resources;
                        // Debug.Log("Hit the object!");
                    }
                }
            }
        }
    }
}
