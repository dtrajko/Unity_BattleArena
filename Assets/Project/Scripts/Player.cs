using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public enum PlayerTool {
        Pickaxe,
        ObstacleVertical,
        ObstacleRamp,
        ObstacleHorizontal,
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
    [SerializeField] private int initialResourceCount = 100;
    [SerializeField] private float resourceCollectionCooldown = 0.4f;

    [Header("Obstacles")]
    [SerializeField] private GameObject obstaclePlacementContainer;
    [SerializeField] private GameObject obstacleContainer;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float obstaclePlacementCooldown = 0.4f;


    private bool isFocalPointOnLeft = true;
    private int resources = 0;
    private float resourceCollectionCooldownTimer = 0;
    private float obstaclePlacementCooldownTimer = 0;
    private GameObject currentObstacle;
    private bool obstaclePlacementLock;

    private List<Weapon> weapons;
    private Weapon weapon;

    private bool isUsingTools = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        resources = initialResourceCount;
        hud.Resources = resources;
        tool = PlayerTool.Pickaxe;
        hud.Tool = tool; // PlayerTool: Pickaxe
        hud.UpdateWeapon(null);

        weapons = new List<Weapon>();
    }
 
    // Update is called once per frame
    void Update()
    {
        // Update timers
        resourceCollectionCooldownTimer -= Time.deltaTime;
        obstaclePlacementCooldownTimer -= Time.deltaTime;

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

        // Select weapons
        if (Input.GetKeyDown("1")) {
            SwitchWeapon(0);
        } else if (Input.GetKeyDown("2")) {
            SwitchWeapon(1);
        } else if(Input.GetKeyDown("3")) {
            SwitchWeapon(2);
        } else if (Input.GetKeyDown("4")) {
            SwitchWeapon(3);
        } else if (Input.GetKeyDown("5")) {
            SwitchWeapon(4);
        }

        // Tool switch logic
        hud.Tool = tool;
        if (Input.GetKeyDown(toolSwitchKey))
        {
            SwitchTool();
        }

        // Preserving the obstacles' horizontal rotation
        if (currentObstacle != null) {
            currentObstacle.transform.eulerAngles = new Vector3(
                0,
                currentObstacle.transform.eulerAngles.y,
                currentObstacle.transform.eulerAngles.z
            );
        }

        // Tool usage logic (continuous)
        if (Input.GetAxis("Fire1") > 0.1f) {
            UseToolContinuous();
        }

        // Tool usage logic (trigger)
        if (Input.GetAxis("Fire1") > 0.1f) {
            if (!obstaclePlacementLock) // it doesn't work properly, using resourceCollectionCooldownTimer instead
            {
                obstaclePlacementLock = true;
                UseToolTrigger();
            } else {
                obstaclePlacementLock = false;
            }
        }

        UpdateWeapon();
    }

    private void SwitchWeapon(int index)
    {
        if (index < weapons.Count) {
            isUsingTools = false;

            weapon = weapons[index];
            hud.UpdateWeapon(weapon);

            tool = PlayerTool.None;
            hud.Tool = tool;

            if (currentObstacle != null) Destroy(currentObstacle);
        }
    }

    private void SwitchTool() {

        isUsingTools = true;

        weapon = null;
        hud.UpdateWeapon(weapon);

        // Cycle between the available tools
        int currentToolIndex = (int)tool;
        currentToolIndex++;
        if (currentToolIndex == System.Enum.GetNames(typeof(PlayerTool)).Length)
        {
            currentToolIndex = 0;
        }

        // Get the new tool
        tool = (PlayerTool)currentToolIndex;
        hud.Tool = tool;

        // Check for obstacle placement logic
        int obstacleToAddIndex = -1;
        if (tool == PlayerTool.ObstacleVertical) obstacleToAddIndex = 0;
        else if (tool == PlayerTool.ObstacleRamp) obstacleToAddIndex = 1;
        else if (tool == PlayerTool.ObstacleHorizontal) obstacleToAddIndex = 2;

        if (currentObstacle != null) Destroy(currentObstacle);
        if (obstacleToAddIndex >= 0)
        {
            currentObstacle = Instantiate(obstaclePrefabs[obstacleToAddIndex]);
            currentObstacle.transform.SetParent(obstaclePlacementContainer.transform);
            currentObstacle.transform.localPosition = Vector3.zero;
            currentObstacle.transform.localRotation = Quaternion.identity;

            hud.UpdateResourcesRequirement(currentObstacle.GetComponent<Obstacle>().Cost, resources);

            currentObstacle.GetComponentInChildren<Collider>().enabled = true;

        }
    }

    private void UseToolContinuous() {
        if (tool == PlayerTool.Pickaxe)
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out hit, (int)interactionDistance);
            if (isHit)
            {
                if (resourceCollectionCooldownTimer <= 0
                    && hit.transform.GetComponentInChildren<ResourceObject>() != null)
                {
                    resourceCollectionCooldownTimer = resourceCollectionCooldown;
                    ResourceObject resourceObject = hit.transform.GetComponent<ResourceObject>();
                    int collectedResources = resourceObject.Collect();
                    resources += collectedResources;
                    hud.Resources = resources;
                }
            }
        }
    }

    private void UseToolTrigger() {
        if (obstaclePlacementCooldownTimer <= 0 &&
            currentObstacle != null &&
            resources >= currentObstacle.GetComponent<Obstacle>().Cost)
        {
            resources -= currentObstacle.GetComponent<Obstacle>().Cost;
            hud.Resources = resources;
            hud.UpdateResourcesRequirement(currentObstacle.GetComponent<Obstacle>().Cost, resources);

            obstaclePlacementCooldownTimer = obstaclePlacementCooldown;
            GameObject newObstacle = Instantiate(currentObstacle);
            newObstacle.transform.SetParent(obstacleContainer.transform);
            newObstacle.transform.position = currentObstacle.transform.position;
            newObstacle.transform.rotation = currentObstacle.transform.rotation;
            newObstacle.GetComponent<Obstacle>().Place();
        }
    }

    private void OnTriggerEnter(Collider otherCollider) {
        if (otherCollider.GetComponent<ItemBox>() != null) {
            ItemBox itemBox = otherCollider.gameObject.GetComponent<ItemBox>();
            if (itemBox.Type == ItemBox.ItemType.Pistol) {
                GiveItem(itemBox.Type, itemBox.Amount);
            }

            Destroy(otherCollider.gameObject);
        }
    }

    private void GiveItem(ItemBox.ItemType type, int amount) {
        if (type == ItemBox.ItemType.Pistol) {
            // Create a weapon reference
            Weapon currentWeapon = null;

            // Check if we already have an instance of this weapon
            for (int i = 0; i < weapons.Count; i++) {
                if (weapons[i] is Pistol) {
                    currentWeapon = weapons[i];
                }
            }

            // If we don't have a weapon of this type, create one and
            // add it to the weapon list
            if (currentWeapon == null) {
                currentWeapon = new Pistol();
                weapons.Add(currentWeapon);
            }

            currentWeapon.AddAmmunition(amount);
            currentWeapon.LoadClip();

            if (currentWeapon == weapon) {
                hud.UpdateWeapon(weapon);
            }
        }
    }

    private void UpdateWeapon()
    {
        if (weapon != null) {

            if (Input.GetKeyDown(KeyCode.R)) {
                weapon.Reload();
            }

            float timeElapsed = Time.deltaTime;
            bool isPressingTrigger = Input.GetAxis("Fire1") > 0.1f;

            bool hasShot = weapon.Update(timeElapsed, isPressingTrigger);
            hud.UpdateWeapon(weapon);
        }
    }
}
