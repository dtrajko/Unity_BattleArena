using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using Mirror;

public class PlayerNetwork : Player, IDamageable
{
    // Start is called before the first frame update
    protected override void Start()
    {
        DontDestroyOnLoad(this);

        energyFallingSpeed = -6.0f;
        energyMovingSpeed = 16.0f;

        transform.LookAt(new Vector3(0, transform.position.y, 0), Vector3.up);

        spawnPositions = FindObjectsOfType<NetworkStartPosition>();

        // Initialize values
        Initialize();

        if (isServer && stormManager != null)
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
    public override void OnServerStartMatch()
    {
        if (!isServer) return;

        ShouldAllowEnergyMovement = true;
        if (stormManager != null)
        {
            stormManager.ShouldShrink = true;
        }
        CmdReSpawn(gameObject);
    }

    protected override void FixedUpdate()
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
    protected override void LateUpdate()
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

    protected override void Update()
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

    [ClientRpc]
    public override void RpcAllowMovement()
    {
        if (!isLocalPlayer) return;

        ShouldAllowEnergyMovement = true;
    }

    protected override void OnTriggerEnter(Collider otherCollider)
    {
        if (!isLocalPlayer) return;

        if (otherCollider.GetComponent<ItemBox>() != null)
        {
            ItemBox itemBox = otherCollider.gameObject.GetComponent<ItemBox>();
            GiveItem(itemBox.Type, itemBox.Amount);
            CmdCollectBox(otherCollider.gameObject);
        }
    }
}
