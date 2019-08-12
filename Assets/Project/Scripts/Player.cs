using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour, IDamageable {

    public enum PlayerTool {
        Pickaxe,
        ObstacleVertical,
        ObstacleRamp,
        ObstacleHorizontal,
        None
    }

    public enum WeaponSound
    {
        Pistol,
        MachineGun,
        Shotgun,
        Sniper
    }

    [Header("Focal Point Variables")]
    [SerializeField] private GameObject focalPoint = null;
    [SerializeField] private GameObject rotationPoint = null;
    [SerializeField] private float focalDistance = -0.3f;
    [SerializeField] private float focalSmoothness = 4f;
    [SerializeField] private KeyCode changeFocalSideKey = KeyCode.Q;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 6f;

    [Header("Gameplay")]
    [SerializeField] private KeyCode toolSwitchKey = KeyCode.Tab;
    [SerializeField] private PlayerTool tool;
    [SerializeField] private int initialResourceCount = 100;
    [SerializeField] private float resourceCollectionCooldown = 0.4f;

    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float obstaclePlacementCooldown = 0.4f;

    [Header("Weapons")]
    [SerializeField] private GameObject shootOrigin;
    [SerializeField] private GameObject rocketPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource soundInterface;
    [SerializeField] private AudioSource[] soundsWeapons;

    [Header("Debug")]
    [SerializeField] private GameObject debugPositionPrefab;

    private bool isFocalPointOnLeft = true;
    private int resources = 0;
    private float resourceCollectionCooldownTimer = 0;
    private float obstaclePlacementCooldownTimer = 0;
    private GameObject currentObstacle;
    private bool obstaclePlacementLock;
    private List<Weapon> weapons;
    private Weapon weapon;
    private HUDController hud;
    private GameCamera gameCamera;
    private GameObject obstaclePlacementContainer;
    private GameObject obstacleContainer;
    private int obstacleToAddIndex;
    private Health health;

    private int weaponIndex = -1; // by dtrajko

    public float Health { get { return health.Value; } }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize values
        health = GetComponent<Health>();
        health.OnHealthChanged += OnHealthChanged;
        resources = initialResourceCount;
        weapons = new List<Weapon>();
        tool = PlayerTool.Pickaxe;

        if (isLocalPlayer)
        {
            // Game camera
            gameCamera = FindObjectOfType<GameCamera>();
            obstaclePlacementContainer = gameCamera.ObstaclePlacementContainer;
            gameCamera.Target = focalPoint;
            gameCamera.RotationAnchorObject = rotationPoint;

            // HUD elements
            hud = FindObjectOfType<HUDController>();
            hud.ShowScreen("regular");
            hud.Health = health.Value;
            hud.Resources = resources;
            hud.Tool = tool; // PlayerTool: Pickaxe
            hud.UpdateWeapon(null);
        }

        // Obstacle container
        obstacleContainer = GameObject.Find("ObstacleContainer");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

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

        // by dtrajko: use mouse scrollwheel to switch weapons
        if (Input.mouseScrollDelta.y < -0.05f)
        {
            weaponIndex++;
            if (weaponIndex >= weapons.Count) weaponIndex = 0;
            SwitchWeapon(weaponIndex);
        }
        if (Input.mouseScrollDelta.y > 0.05f) {
            weaponIndex--;
            if (weaponIndex < 0) weaponIndex = weapons.Count > 0 ? weapons.Count - 1 : 0;
            SwitchWeapon(weaponIndex);
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

            soundInterface.Play();

            weapon = weapons[index];
            hud.UpdateWeapon(weapon);

            tool = PlayerTool.None;
            hud.Tool = tool;

            if (currentObstacle != null) Destroy(currentObstacle);

            // Zoom out
            if (!(weapon is Sniper)) {
                gameCamera.ZoomOut();
                hud.SniperAimVisibility = false;
            }
        }
        weaponIndex = index; // by dtrajko
    }

    private void SwitchTool() {

        soundInterface.Play();

        weapon = null;
        hud.UpdateWeapon(weapon);

        // Zoom the camera out
        gameCamera.ZoomOut();
        hud.SniperAimVisibility = false;

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
        obstacleToAddIndex = -1;
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

            currentObstacle.GetComponent<Obstacle>().SetPositioningMode();

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

                    int collectedResources = 0;
                    float resourceHealth = resourceObject.HealthValue;
                    if (resourceHealth < 0.01f) {
                        collectedResources = resourceObject.ResourceAmount;
                    }

                    CmdDamage(hit.transform.gameObject, 1);

                    resources += collectedResources;
                    hud.Resources = resources;

                    soundInterface.Play();
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

            CmdPlaceObstacle(obstacleToAddIndex, currentObstacle.transform.position, currentObstacle.transform.rotation);
        }
    }

    [Command]
    void CmdPlaceObstacle(int index, Vector3 position, Quaternion rotation) {
        GameObject newObstacle = Instantiate(obstaclePrefabs[index]);
        newObstacle.transform.SetParent(obstacleContainer.transform);
        newObstacle.transform.position = position;
        newObstacle.transform.rotation = rotation;
        newObstacle.GetComponent<Obstacle>().Place();

        NetworkServer.Spawn(newObstacle);
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (!isLocalPlayer) return;

        if (otherCollider.GetComponent<ItemBox>() != null) {
            ItemBox itemBox = otherCollider.gameObject.GetComponent<ItemBox>();
            GiveItem(itemBox.Type, itemBox.Amount);
            CmdCollectBox(otherCollider.gameObject);

            soundInterface.Play();
        }
    }

    [Command]
    void CmdCollectBox(GameObject box) {
        Destroy(box);
    }

    private void GiveItem(ItemBox.ItemType type, int amount) {
        // Create a weapon reference
        Weapon currentWeapon = null;

        // Check if we already have an instance of this weapon
        for (int i = 0; i < weapons.Count; i++) {
            if (type == ItemBox.ItemType.Pistol && weapons[i] is Pistol) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.MachineGun && weapons[i] is MachineGun) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.Shotgun && weapons[i] is Shotgun) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.Sniper && weapons[i] is Sniper) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.RocketLauncher && weapons[i] is RocketLauncher) currentWeapon = weapons[i];
        }

        // If we don't have a weapon of this type, create one and
        // add it to the weapon list
        if (currentWeapon == null) {
            if (type == ItemBox.ItemType.Pistol) currentWeapon = new Pistol();
            else if (type == ItemBox.ItemType.MachineGun) currentWeapon = new MachineGun();
            else if (type == ItemBox.ItemType.Shotgun) currentWeapon = new Shotgun();
            else if (type == ItemBox.ItemType.Sniper) currentWeapon = new Sniper();
            else if (type == ItemBox.ItemType.RocketLauncher) currentWeapon = new RocketLauncher();
            weapons.Add(currentWeapon);
            weapons.Sort();
        }

        currentWeapon.AddAmmunition(amount);
        currentWeapon.LoadClip();

        if (currentWeapon == weapon) {
            hud.UpdateWeapon(weapon);
        }
    }

    private void UpdateWeapon()
    {
        if (weapon != null)
        {

            if (Input.GetKeyDown(KeyCode.R))
            {
                weapon.Reload();
            }

            float timeElapsed = Time.deltaTime;
            bool isPressingTrigger = Input.GetAxis("Fire1") > 0.1f;

            bool hasShot = weapon.Update(timeElapsed, isPressingTrigger);
            hud.UpdateWeapon(weapon);

            if (hasShot)
            {
                Shoot();
            }

            // Zoom logic
            Visibility = true;
            if (weapon is Sniper) {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) {
                    gameCamera.TriggerZoom();
                    hud.SniperAimVisibility = gameCamera.IsZoomedIn;
                    Visibility = !gameCamera.IsZoomedIn;
                }
            }
        }
    }

    // Hide player (3rd person character) models in zoom in mode
    // Unhide player (3rd person character) models in zoom out mode
    public bool Visibility
    {
        set
        {
            Renderer[] playerRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (Renderer playerRenderer in playerRenderers)
            {
                playerRenderer.enabled = value;
            }
        }
    }

    [Command]
    void CmdPlayWeaponSound(GameObject caller, int index) {
        if (!isServer) return;

        RpcPlayWeaponSound(caller, index);
    }

    [ClientRpc]
    void RpcPlayWeaponSound(GameObject caller, int index) {
        caller.GetComponent<Player>().PlayWeaponSound(index);
    }

    public void PlayWeaponSound(int index)
    {
        soundsWeapons[index].Play();
    }

    private void Shoot()
    {
        int amountOfBullets = 1;
        if (weapon is Shotgun) {
            amountOfBullets = ((Shotgun)weapon).AmountOfBullets;
        }

        if (weapon is Pistol)     CmdPlayWeaponSound(gameObject, (int)WeaponSound.Pistol);
        if (weapon is MachineGun) CmdPlayWeaponSound(gameObject, (int)WeaponSound.MachineGun);
        if (weapon is Shotgun)    CmdPlayWeaponSound(gameObject, (int)WeaponSound.Shotgun);
        if (weapon is Sniper)     CmdPlayWeaponSound(gameObject, (int)WeaponSound.Sniper);

        for (int i = 0; i < amountOfBullets; i++) {
            float distanceFromCamera = Vector3.Distance(gameCamera.transform.position, transform.position);
            RaycastHit targetHit;
            bool isTargetHit = Physics.Raycast(
                gameCamera.transform.position + gameCamera.transform.forward * distanceFromCamera,
                gameCamera.transform.forward, out targetHit);
            if (isTargetHit)
            {
                Vector3 hitPosition = targetHit.point;
                Vector3 shootDirection = (hitPosition - shootOrigin.transform.position).normalized;
                shootDirection = new Vector3(
                    shootDirection.x + UnityEngine.Random.Range(-weapon.AimVariation, weapon.AimVariation),
                    shootDirection.y + UnityEngine.Random.Range(-weapon.AimVariation, weapon.AimVariation),
                    shootDirection.z + UnityEngine.Random.Range(-weapon.AimVariation, weapon.AimVariation)
                );
                shootDirection.Normalize();

                if (!(weapon is RocketLauncher))
                {
                    RaycastHit shootHit;
                    bool isShootHit = Physics.Raycast(shootOrigin.transform.position, shootDirection, out shootHit);
                    if (isShootHit)
                    {
                        GameObject debugPositionInstance = Instantiate(debugPositionPrefab);
                        debugPositionInstance.transform.position = shootHit.point;
                        Destroy(debugPositionInstance, 0.5f);

                        if (shootHit.transform.GetComponent<IDamageable>() != null)
                        {
                            CmdDamage(shootHit.transform.gameObject, weapon.Damage);
                        } else if (shootHit.transform.GetComponentInParent<IDamageable>() != null)
                        {
                            CmdDamage(shootHit.transform.parent.gameObject, weapon.Damage);
                        }
                        Debug.DrawLine(shootOrigin.transform.position, shootOrigin.transform.position + shootDirection * 100, Color.red);
                    }
                }
                else {
                    CmdSpawnRocket(shootDirection);
                }
            }
        }
    }

    [Command]
    private void CmdSpawnRocket(Vector3 shootDirection) {
        GameObject rocket = Instantiate(rocketPrefab);
        rocket.transform.position = shootOrigin.transform.position + shootDirection;
        rocket.GetComponent<Rocket>().Shoot(shootDirection);
        NetworkServer.Spawn(rocket);
    }

    [Command]
    private void CmdDamage(GameObject target, float damage) {
        if (target != null) {
            target.GetComponent<IDamageable>().Damage(damage);
        }
    }

    public int Damage(float amount)
    {
        GetComponent<Health>().Damage(amount);
        return 0;
    }

    public void OnHealthChanged(float newHealth) {

        if (!isLocalPlayer) return;

        hud.Health = newHealth;
        hud.UpdateHealthBar(health.Value / 100);

        if (newHealth < 0.01f) {
            hud.ShowScreen("gameOver");
            CmdDestroy();
        }
    }

    [Command]
    public void CmdDestroy() {
        Destroy(gameObject);
    }
}
