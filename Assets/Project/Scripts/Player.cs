using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using Mirror;

public class Player : NetworkBehaviour, IDamageable
{

    public enum PlayerTool
    {
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

    [Header("Health")]
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private Canvas healthBarCanvas;

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
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject shootOrigin;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject modelAxe;
    [SerializeField] private GameObject modelPistol;
    [SerializeField] private GameObject modelMachineGun;
    [SerializeField] private GameObject modelShotgun;
    [SerializeField] private GameObject modelSniper;
    [SerializeField] private GameObject modelRocketLauncher;
    [SerializeField] private GameObject modelRocketLauncherUnloaded;
    [SerializeField] private float rocketLauncherCooldown = 5.0f; // by dtrajko

    [Header("Audio")]
    [SerializeField] private AudioSource soundInterface;
    [SerializeField] private AudioSource[] soundsWeapons;
    [SerializeField] private AudioSource[] soundsFootsteps;
    [SerializeField] private AudioSource soundJump;
    [SerializeField] private AudioSource soundLand;
    [SerializeField] private float stepInterval = 0.25f;
    [SerializeField] private AudioSource soundHit;

    [Header("Visuals")]
    [SerializeField] private GameObject characterContainer;
    [SerializeField] private GameObject energyBall;

    [Header("EnergyMode")]
    [SerializeField] private float energyFallingSpeed;
    [SerializeField] private float energyMovingSpeed;

    private bool isFocalPointOnLeft = true;
    private int resources = 0;
    private float resourceCollectionCooldownTimer = 0;
    private float obstaclePlacementCooldownTimer = 0;
    private float rocketLauncherCooldownTimer = 0; // by dtrajko
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
    private float stepTimer;
    private Animator playerAnimator;
    private NetworkAnimator playerNetworkAnimator;
    private string modelName; // Current weapon or tool the player is holding
    private Rigidbody playerRigidbody;
    private float stormDamageTimer = 1;
    private StormManager stormManager;
    private NetworkStartPosition[] spawnPositions;
    private bool shouldAllowEnergyMovement;
    private int weaponIndex = -1; // by dtrajko
    private bool isCursorLocked;

    public bool ShouldAllowEnergyMovement
    {
        get { return shouldAllowEnergyMovement; }
        set
        {
            shouldAllowEnergyMovement = value;
            if (value)
            {
                // Cursor.lockState = CursorLockMode.Locked;
                GetHUD().ShowScreen("spawn");
            }
        }
    }

    private bool isInEnergyMode;
    public bool IsInEnergyMode
    {
        get
        {
            return isInEnergyMode;
        }
        set
        {
            isInEnergyMode = value;
            if (value == true)
            {
                playerRigidbody.useGravity = false;
                energyBall.SetActive(true);
                characterContainer.transform.localScale = Vector3.zero;
                healthBarCanvas.transform.localScale = Vector3.zero;
            }
            else
            {
                playerRigidbody.useGravity = true;
                energyBall.SetActive(false);
                characterContainer.transform.localScale = Vector3.one;
                healthBarCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 1);
                playerAnimator.updateMode = AnimatorUpdateMode.Normal;
                if (hud != null)
                {
                    hud.ShowScreen("regular");
                    if (Application.platform == RuntimePlatform.WindowsPlayer)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }
            }
        }
    }

    public bool IsCursorLocked
    {
        get
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                isCursorLocked = true;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                isCursorLocked = false;
            }
            return isCursorLocked;
        }
        set
        {
            isCursorLocked = value;
            if (value == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (value == false)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public float Health { get { return health.Value; } }

    public Canvas HealthBarCanvas { get { return healthBarCanvas; } }

    HUDController GetHUD() {
        if (hud == null) {
            hud = FindObjectOfType<HUDController>();
        }
        return hud;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);

        energyFallingSpeed = -5.0f;
        energyMovingSpeed = 16.0f;

        transform.LookAt(new Vector3(0, transform.position.y, 0), Vector3.up);

        spawnPositions = FindObjectsOfType<NetworkStartPosition>();

        // Initialize values
        Initialize();

        if (isServer)
        {
            stormManager = FindObjectOfType<StormManager>();
            stormManager.OnShrink += OnStormShrink;
        }

        tool = PlayerTool.Pickaxe;

        IsInEnergyMode = true;

        if (isLocalPlayer)
        {
            // Game camera
            gameCamera = FindObjectOfType<GameCamera>();
            obstaclePlacementContainer = gameCamera.ObstaclePlacementContainer;
            gameCamera.Target = focalPoint;
            gameCamera.RotationAnchorObject = rotationPoint;

            // HUD elements
            hud = FindObjectOfType<HUDController>();
            hud.ShowScreen("");

            if (isServer) hud.ShowScreen("server");
            else if (isClient) hud.ShowScreen("client");
            Cursor.lockState = CursorLockMode.None;

            hud.Health = health.Value;
            hud.Resources = resources;
            hud.Tool = tool; // PlayerTool: Pickaxe
            hud.UpdateWeapon(null);
            hud.OnStartMatch += OnServerStartMatch;

            // Listen to events
            GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().OnFootstep += OnFootstep;
            GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>().OnJump += OnJump;

            // Get animator
            playerAnimator = GetComponent<Animator>();
            playerNetworkAnimator = GetComponent<NetworkAnimator>();

            // Show no models
            CmdShowModel(gameObject, "Pickaxe");
            CmdRefreshModels();
        }

        // Obstacle container
        obstacleContainer = GameObject.Find("ObstacleContainer");
    }

    public void OnServerStartMatch()
    {
        if (!isServer) return;

        ShouldAllowEnergyMovement = true;
        stormManager.ShouldShrink = true;
        CmdReSpawn(gameObject);
    }

    public void OnStormShrink()
    {
        RpcAlertShrink();
    }

    [ClientRpc]
    public void RpcAllowMovement()
    {
        if (!isLocalPlayer) return;

        ShouldAllowEnergyMovement = true;
    }

    [ClientRpc]
    public void RpcAlertShrink()
    {
        if (!isLocalPlayer) return;

        hud.Alert();
    }


    private void FixedUpdate()
    {

        if (!isLocalPlayer) return;

        if (IsInEnergyMode)
        {
            if (ShouldAllowEnergyMovement)
            {
                float horizontalSpeed = CrossPlatformInputManager.GetAxis("Horizontal") * energyMovingSpeed;
                float depthSpeed = CrossPlatformInputManager.GetAxis("Vertical") * energyMovingSpeed;

                Vector3 cameraForward = Vector3.Scale(gameCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 moveVector = (horizontalSpeed * gameCamera.transform.right) + (depthSpeed * cameraForward);

                playerRigidbody.velocity = new Vector3(
                    moveVector.x,
                    energyFallingSpeed,
                    moveVector.z
                );
            }
            else
            {
                playerRigidbody.velocity = Vector3.zero;
            }
        }
    }

    //Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        if (isLocalPlayer)
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                // You might want to change Vector3.back to Vector3.forward, depending on the initial orientation of your object
                Vector3 orientation = Vector3.forward; // player.isLocalPlayer ? Vector3.back : Vector3.forward;
                player.HealthBarCanvas.transform.LookAt(
                    player.HealthBarCanvas.transform.position + gameCamera.transform.rotation * orientation,
                    gameCamera.transform.rotation * Vector3.up);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            IsCursorLocked = !IsCursorLocked;
        }

        if (!ShouldAllowEnergyMovement)
        {
            hud.Players = FindObjectsOfType<Player>().Length;
        }

        if (IsInEnergyMode)
        {
            // Check if touched the floor
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 2.0f))
            {
                if (hitInfo.transform.GetComponent<Player>() == null)
                {
                    IsInEnergyMode = false;
                    CmdDeactivateEnergyBall(gameObject);
                }
            }
        }

        // Update timers
        resourceCollectionCooldownTimer -= Time.deltaTime;
        obstaclePlacementCooldownTimer -= Time.deltaTime;
        rocketLauncherCooldownTimer -= Time.deltaTime;
        stepTimer -= Time.deltaTime;
        stormDamageTimer -= Time.deltaTime;

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
        if (Input.GetKeyDown("1"))
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown("2"))
        {
            SwitchWeapon(1);
        }
        else if (Input.GetKeyDown("3"))
        {
            SwitchWeapon(2);
        }
        else if (Input.GetKeyDown("4"))
        {
            SwitchWeapon(3);
        }
        else if (Input.GetKeyDown("5"))
        {
            SwitchWeapon(4);
        }

        // by dtrajko: use mouse scrollwheel to switch weapons
        if (Input.mouseScrollDelta.y < -0.05f)
        {
            weaponIndex++;
            if (weaponIndex >= weapons.Count) weaponIndex = 0;
            SwitchWeapon(weaponIndex);
        }
        if (Input.mouseScrollDelta.y > 0.05f)
        {
            weaponIndex--;
            if (weaponIndex < 0) weaponIndex = weapons.Count > 0 ? weapons.Count - 1 : 0;
            SwitchWeapon(weaponIndex);
        }

        // dtrajko - Cross-platform input: Switch weapon on Fire2 (yellow) button
        if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
            && CrossPlatformInputManager.GetButtonDown("Fire2"))
        {
            weaponIndex++;
            if (weaponIndex >= weapons.Count) weaponIndex = 0;
            SwitchWeapon(weaponIndex);
        }

        // Tool switch logic
        hud.Tool = tool;
        if (Input.GetKeyDown(toolSwitchKey) || CrossPlatformInputManager.GetButtonDown("Fire3"))
        {
            SwitchTool();
        }

        // Preserving the obstacles' horizontal rotation
        if (currentObstacle != null)
        {
            currentObstacle.transform.eulerAngles = new Vector3(
                0,
                currentObstacle.transform.eulerAngles.y,
                currentObstacle.transform.eulerAngles.z
            );
        }

        // Tool usage logic (continuous)
        if (CrossPlatformInputManager.GetAxis("Fire1") > 0.1f ||
            CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            UseToolContinuous();
        }

        // Tool usage logic (trigger)
        if (CrossPlatformInputManager.GetAxis("Fire1") > 0.1f ||
            CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            if (!obstaclePlacementLock) // it doesn't work properly, using resourceCollectionCooldownTimer instead
            {
                obstaclePlacementLock = true;
                UseToolTrigger();
            }
            else
            {
                obstaclePlacementLock = false;
            }
        }

        UpdateWeapon();
    }

    private void Initialize()
    {
        resources = initialResourceCount;
        weapons = new List<Weapon>();
        health = GetComponent<Health>();
        health.ResetHealth();
        health.OnHealthChanged += OnHealthChanged;
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void AnimateWeaponHold(string weaponName)
    {
        playerAnimator.SetTrigger("Hold" + weaponName);
        playerNetworkAnimator.SetTrigger("Hold" + weaponName);
    }

    private void AnimateShoot()
    {
        playerAnimator.SetTrigger("Shoot");
        playerNetworkAnimator.SetTrigger("Shoot");
    }

    private void AnimateUnequip()
    {
        playerAnimator.SetTrigger("HoldNothing");
        playerNetworkAnimator.SetTrigger("HoldNothing");
    }

    private void AnimateMelee()
    {
        playerAnimator.SetTrigger("MeleeSwing");
        playerNetworkAnimator.SetTrigger("MeleeSwing");
    }

    private void SwitchWeapon(int index)
    {
        if (index < weapons.Count)
        {

            soundInterface.Play();

            weapon = weapons[index];
            hud.UpdateWeapon(weapon);

            // Show animations
            if (weapon is Pistol) AnimateWeaponHold("Pistol");
            else if (weapon is MachineGun) AnimateWeaponHold("Rifle");
            else if (weapon is Shotgun) AnimateWeaponHold("Rifle");
            else if (weapon is Sniper) AnimateWeaponHold("Rifle");
            else if (weapon is RocketLauncher) AnimateWeaponHold("Rocket");

            // Show models
            if (weapon is Pistol) CmdShowModel(gameObject, "Pistol");
            else if (weapon is MachineGun) CmdShowModel(gameObject, "MachineGun");
            else if (weapon is Shotgun) CmdShowModel(gameObject, "Shotgun");
            else if (weapon is Sniper) CmdShowModel(gameObject, "Sniper");
            else if (weapon is RocketLauncher) CmdShowModel(gameObject, "RocketLauncher");

            tool = PlayerTool.None;
            hud.Tool = tool;

            if (currentObstacle != null) Destroy(currentObstacle);

            // Zoom out
            if (!(weapon is Sniper))
            {
                gameCamera.ZoomOut();
                hud.SniperAimVisibility = false;
            }
        }
        weaponIndex = index; // by dtrajko
    }

    private void SwitchTool()
    {

        if (IsInEnergyMode) return;

        soundInterface.Play();

        AnimateUnequip();

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

        if (tool == PlayerTool.Pickaxe)
        {
            CmdShowModel(gameObject, "Pickaxe");
        }
        else
        {
            CmdShowModel(gameObject, "");
        }

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

    private void UseToolContinuous()
    {
        if (tool == PlayerTool.Pickaxe)
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(gameCamera.transform.position, gameCamera.transform.forward, out hit, (int)interactionDistance);
            if (isHit)
            {
                if (resourceCollectionCooldownTimer <= 0
                    && hit.transform.GetComponentInChildren<ResourceObject>() != null)
                {
                    CmdHit(gameObject);

                    AnimateMelee();

                    resourceCollectionCooldownTimer = resourceCollectionCooldown;

                    CmdDamage(hit.transform.gameObject, 1);
                    ResourceObject resourceObject = hit.transform.GetComponent<ResourceObject>();

                    int collectedResources = 0;
                    float resourceHealth = resourceObject.HealthValue - 1; // dirty fix - return value must be adjusted because the Health value is updates with delay
                    if (resourceHealth < 0.1f)
                    {
                        collectedResources = resourceObject.ResourceAmount;
                    }
                    resources += collectedResources;
                    hud.Resources = resources;
                }
            }
        }
    }

    private void UseToolTrigger()
    {
        if (obstaclePlacementCooldownTimer <= 0 && currentObstacle != null && resources >= currentObstacle.GetComponent<Obstacle>().Cost)
        {
            CmdHit(gameObject);

            int cost = currentObstacle.GetComponent<Obstacle>().Cost;
            resources -= cost;

            hud.Resources = resources;
            hud.UpdateResourcesRequirement(currentObstacle.GetComponent<Obstacle>().Cost, resources);

            obstaclePlacementCooldownTimer = obstaclePlacementCooldown;

            CmdPlaceObstacle(obstacleToAddIndex, currentObstacle.transform.position, currentObstacle.transform.rotation);
        }
    }

    [Command]
    void CmdPlaceObstacle(int index, Vector3 position, Quaternion rotation)
    {
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

        if (otherCollider.GetComponent<ItemBox>() != null)
        {
            ItemBox itemBox = otherCollider.gameObject.GetComponent<ItemBox>();
            GiveItem(itemBox.Type, itemBox.Amount);
            CmdCollectBox(otherCollider.gameObject);
        }
    }

    [Command]
    void CmdCollectBox(GameObject box)
    {
        Destroy(box);
    }

    private void GiveItem(ItemBox.ItemType type, int amount)
    {
        if (type == ItemBox.ItemType.FirstAid)
        {
            GiveItemFirstAid(type, amount);
        }
        else
        {
            GiveItemWeapon(type, amount);
        }
    }

    private void GiveItemWeapon(ItemBox.ItemType type, int amount)
    {
        CmdHit(gameObject);

        // Create a weapon reference
        Weapon currentWeapon = null;

        // Check if we already have an instance of this weapon
        for (int i = 0; i < weapons.Count; i++)
        {
            if (type == ItemBox.ItemType.Pistol && weapons[i] is Pistol) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.MachineGun && weapons[i] is MachineGun) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.Shotgun && weapons[i] is Shotgun) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.Sniper && weapons[i] is Sniper) currentWeapon = weapons[i];
            else if (type == ItemBox.ItemType.RocketLauncher && weapons[i] is RocketLauncher) currentWeapon = weapons[i];
        }

        // If we don't have a weapon of this type, create one and
        // add it to the weapon list
        if (currentWeapon == null)
        {
            if (type == ItemBox.ItemType.Pistol) currentWeapon = new Pistol();
            else if (type == ItemBox.ItemType.MachineGun) currentWeapon = new MachineGun();
            else if (type == ItemBox.ItemType.Shotgun) currentWeapon = new Shotgun();
            else if (type == ItemBox.ItemType.Sniper) currentWeapon = new Sniper();
            else if (type == ItemBox.ItemType.RocketLauncher) currentWeapon = new RocketLauncher();
            weapons.Add(currentWeapon);
            weapons.Sort();
        }

        // Automatically equip if weapon still not available
        if (weapon == null) SwitchWeapon(0);

        currentWeapon.AddAmmunition(amount);
        currentWeapon.LoadClip();

        if (currentWeapon == weapon)
        {
            hud.UpdateWeapon(weapon);
        }
    }

    private void GiveItemFirstAid(ItemBox.ItemType type, int amount)
    {
        CmdHit(gameObject);
        CmdHeal(gameObject, amount);
    }

    private void UpdateWeapon()
    {
        if (weapon != null)
        {
            // by dtrajko - display loaded RocketLauncher model instead of RocketLauncherUnloaded model
            if (weapon is RocketLauncher && rocketLauncherCooldownTimer <= 0)
            {
                if (weapon.ClipAmmunition > 0) CmdShowModel(gameObject, "RocketLauncher");
                else CmdShowModel(gameObject, "RocketLauncherUnloaded");
                rocketLauncherCooldownTimer = rocketLauncherCooldown;
            }

            if (Input.GetKeyDown(KeyCode.R) ||
                CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                bool reloaded = weapon.Reload();
            }

            float timeElapsed = Time.deltaTime;
            bool isPressingTrigger = CrossPlatformInputManager.GetAxis("Fire1") > 0.1f;

            // dtrajko - Cross-platform input
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                isPressingTrigger = CrossPlatformInputManager.GetButtonDown("Fire1");
            }

            bool hasShot = weapon.Update(timeElapsed, isPressingTrigger);
            hud.UpdateWeapon(weapon);

            if (hasShot)
            {
                Shoot();
            }

            // Zoom logic
            Visibility = true;
            if (IsZoomModeTriggered(weapon))
            {
                gameCamera.TriggerZoom();
                hud.SniperAimVisibility = gameCamera.IsZoomedIn;
                Visibility = !gameCamera.IsZoomedIn;
            }
        }
    }

    private bool IsZoomModeTriggered(Weapon weapon)
    {

        if (!(weapon is Sniper)) return false;

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) return true;
                if (CrossPlatformInputManager.GetButtonDown("Cancel")) return true;
                break;
            case RuntimePlatform.WindowsPlayer:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) return true;
                break;
            case RuntimePlatform.Android:
                if (CrossPlatformInputManager.GetButtonDown("Cancel")) return true;
                break;
            default:
                return false;
        }
        return false;
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

    private void Shoot()
    {
        int amountOfBullets = 1;
        if (weapon is Shotgun)
        {
            amountOfBullets = ((Shotgun)weapon).AmountOfBullets;
        }

        if (weapon is Pistol) CmdPlayWeaponSound(gameObject, (int)WeaponSound.Pistol);
        if (weapon is MachineGun) CmdPlayWeaponSound(gameObject, (int)WeaponSound.MachineGun);
        if (weapon is Shotgun) CmdPlayWeaponSound(gameObject, (int)WeaponSound.Shotgun);
        if (weapon is Sniper) CmdPlayWeaponSound(gameObject, (int)WeaponSound.Sniper);

        AnimateShoot();

        for (int i = 0; i < amountOfBullets; i++)
        {
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
                        CmdAddBullet(shootHit.point);

                        if (shootHit.transform.GetComponent<IDamageable>() != null)
                        {
                            CmdDamage(shootHit.transform.gameObject, weapon.Damage);
                        }
                        else if (shootHit.transform.GetComponentInParent<IDamageable>() != null)
                        {
                            CmdDamage(shootHit.transform.parent.gameObject, weapon.Damage);
                        }
                        Debug.DrawLine(shootOrigin.transform.position, shootOrigin.transform.position + shootDirection * 100, Color.red);
                    }
                }
                else
                {
                    CmdSpawnRocket(shootDirection);
                }
            }
        }
    }

    [Command]
    private void CmdSpawnRocket(Vector3 shootDirection)
    {
        GameObject rocket = Instantiate(rocketPrefab);
        rocket.transform.position = shootOrigin.transform.position + shootDirection;
        rocket.GetComponent<Rocket>().Shoot(shootDirection);
        NetworkServer.Spawn(rocket);
        CmdShowModel(gameObject, "RocketLauncherUnloaded");
    }

    // Damage methods
    [Command]
    private void CmdDamage(GameObject target, float damage)
    {
        if (!isServer) return;
        RpcDamage(target, damage);
    }

    [ClientRpc]
    void RpcDamage(GameObject caller, float damage)
    {
        if (caller != null)
        {
            caller.GetComponent<IDamageable>().Damage(damage);
        }
    }

    public int Damage(float amount)
    {
        GetComponent<Health>().Damage(amount);
        healthBar.sizeDelta = new Vector2(health.Value * 2, healthBar.sizeDelta.y);
        return 0;
    }

    // Heal methods (opposite of damage)
    [Command]
    private void CmdHeal(GameObject target, float amount)
    {
        if (!isServer) return;
        RpcHeal(target, amount);
    }

    [ClientRpc]
    void RpcHeal(GameObject caller, float amount)
    {
        if (caller != null)
        {
            caller.GetComponent<Health>().Heal(amount);
            healthBar.sizeDelta = new Vector2(health.Value * 2, healthBar.sizeDelta.y);
        }
    }

    public void StormDamage()
    {
        if (!isLocalPlayer) return;

        if (stormDamageTimer <= 0)
        {
            stormDamageTimer = 1;
            CmdDamage(gameObject, 2);
        }
    }

    public void OnHealthChanged(float newHealth)
    {

        if (!isLocalPlayer) return;

        hud.Health = newHealth;

        if (newHealth < 0.01f)
        {
            Cursor.lockState = CursorLockMode.None;
            hud.ShowScreen("gameOver");
            CmdDestroy();
        }
    }

    [Command]
    public void CmdDestroy()
    {
        Destroy(gameObject);
    }

    // Network weapon sound
    [Command]
    void CmdPlayWeaponSound(GameObject caller, int index)
    {
        if (!isServer) return;

        RpcPlayWeaponSound(caller, index);
    }

    [ClientRpc]
    void RpcPlayWeaponSound(GameObject caller, int index)
    {
        caller.GetComponent<Player>().PlayWeaponSound(index);
    }

    public void PlayWeaponSound(int index)
    {
        soundsWeapons[index].Play();
    }

    // Network footstep sound
    [Command]
    void CmdPlayFootstepSound(GameObject caller)
    {
        if (!isServer) return;

        RpcPlayFootstepSound(caller);
    }

    [ClientRpc]
    void RpcPlayFootstepSound(GameObject caller)
    {
        caller.GetComponent<Player>().PlayFootstepSound();
    }

    public void PlayFootstepSound()
    {
        soundsFootsteps[UnityEngine.Random.Range(0, soundsFootsteps.Length)].Play();
    }

    // This event is emitted in the ThirdPersonCharacter
    void OnFootstep(float forwardAmount)
    {
        if (forwardAmount > 0.6f && stepTimer <= 0)
        {
            stepTimer = stepInterval;
            CmdPlayFootstepSound(gameObject);
        }
    }

    void OnJump()
    {
        CmdJump(gameObject);
    }

    // Network jump sound
    [Command]
    void CmdJump(GameObject caller)
    {
        if (!isServer) return;

        RpcJump(caller);
    }

    [ClientRpc]
    void RpcJump(GameObject caller)
    {
        caller.GetComponent<Player>().PlayJumpSound();
    }
    public void PlayJumpSound()
    {
        soundJump.Play();
    }

    // Network hit sound
    [Command]
    void CmdHit(GameObject caller)
    {
        if (!isServer) return;

        RpcHit(caller);
    }

    [ClientRpc]
    void RpcHit(GameObject caller)
    {
        caller.GetComponent<Player>().PlayHitSound();
    }

    public void PlayHitSound()
    {
        soundHit.Play();
    }

    // Network show model
    [Command]
    void CmdShowModel(GameObject caller, string newModel)
    {
        if (!isServer) return;

        RpcShowModel(caller, newModel);
    }

    [ClientRpc]
    void RpcShowModel(GameObject caller, string newModel)
    {
        caller.GetComponent<Player>().ShowModel(newModel);
    }

    public void ShowModel(string newModel)
    {
        modelName = newModel;
        modelAxe.SetActive(newModel == "Pickaxe");
        modelPistol.SetActive(newModel == "Pistol");
        modelMachineGun.SetActive(newModel == "MachineGun");
        modelShotgun.SetActive(newModel == "Shotgun");
        modelSniper.SetActive(newModel == "Sniper");
        modelRocketLauncher.SetActive(newModel == "RocketLauncher");
        modelRocketLauncherUnloaded.SetActive(newModel == "RocketLauncherUnloaded");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdRefreshModels();
    }

    [Command]
    void CmdRefreshModels()
    {
        if (!isServer) return;

        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.RpcRefreshModels();
        }
    }

    [ClientRpc]
    public void RpcRefreshModels()
    {
        CmdShowModel(gameObject, modelName);
    }

    // Network energy balls
    [Command]
    void CmdDeactivateEnergyBall(GameObject caller)
    {
        RpcDeactivateEnergyBall(caller);
    }

    [ClientRpc]
    private void RpcDeactivateEnergyBall(GameObject caller)
    {
        DeactivateEnergyBall(caller);
    }

    void DeactivateEnergyBall(GameObject caller)
    {
        caller.GetComponent<Player>().IsInEnergyMode = false;
    }

    [Command]
    void CmdAddBullet(Vector3 bulletPosition)
    {
        GameObject bulletInstance = Instantiate(bulletPrefab);
        bulletInstance.transform.position = bulletPosition;
        NetworkServer.Spawn(bulletInstance);
        Destroy(bulletInstance, 0.5f);
    }

    // Player re-spawn position
    [Command]
    public void CmdReSpawn(GameObject caller)
    {
        if (!isServer) return;

        RpcReSpawn(caller);
    }

    [ClientRpc]
    void RpcReSpawn(GameObject caller)
    {
        if (!isLocalPlayer) return;

        Initialize();

        if (spawnPositions != null && spawnPositions.Length > 0)
        {
            Vector3 spawnPosition = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Length)].transform.position;
            caller.transform.position = spawnPosition;
        }
    }
}
