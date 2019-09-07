using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HUDController : NetworkBehaviour
{
    public delegate void StartMatchHandler();
    public event StartMatchHandler OnStartMatch;

    [Header("Screens")]
    [SerializeField] private GameObject regularScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject serverScreen;
    [SerializeField] private GameObject clientScreen;
    [SerializeField] private GameObject spawnScreen;

    [Header("Interface Elements")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text resourcesText;
    [SerializeField] private Text resourcesRequirementText;
    [SerializeField] private Text weaponNameText;
    [SerializeField] private Text weaponAmmunitionText;
    [SerializeField] private RectTransform weaponReloadBar;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject sniperAim;
    [SerializeField] private Text serverPlayersText;
    [SerializeField] private Text clientPlayersText;
    [SerializeField] private Text alertText;
    [SerializeField] private RectTransform mobileUI;
    [SerializeField] private RectTransform turnAndLookTouchpad;
    [SerializeField] private RectTransform buttonSwitchTool;
    

    [Header("Tool Selector")]
    [SerializeField] private GameObject toolFocus;
    [SerializeField] private GameObject toolContainer;
    [SerializeField] private float focusSmoothness = 10.0f;

    private float targetFocusX = 0;

    public float Health
    {
        set
        {
            healthText.text = "Health: " + Mathf.CeilToInt(value);
        }
    }

    public int Resources {
        set {
            resourcesText.text = "Resources: " + value;
        }
    }

    public int Players {
        set {
            serverPlayersText.text = "Players: " + value;
            clientPlayersText.text = "Players: " + value;
        }
    }

    public Player.PlayerTool Tool {
        set {
            /* Disabling the code from the lesson 251. Selecting Weapons
            if (value != Player.PlayerTool.None)
            {
                toolFocus.SetActive(true);
                targetFocusX = toolContainer.transform.GetChild((int)value).transform.position.x;
            }
            else
            {
                toolFocus.SetActive(false);
            } */
            targetFocusX = toolContainer.transform.GetChild((int)value).transform.position.x;
            if (value != Player.PlayerTool.ObstacleVertical &&
                value != Player.PlayerTool.ObstacleRamp &&
                value != Player.PlayerTool.ObstacleHorizontal)
            {
                resourcesRequirementText.enabled = false;
            }
            else
            {
                resourcesRequirementText.enabled = true;
            }
        }
    }

    public bool SniperAimVisibility { set { sniperAim.SetActive(value); } }

    // Start is called before the first frame update
    void Start() {
        ShowScreen("");

        // Display mobile UI only on mobile devices
        switch (Application.platform) {
            case RuntimePlatform.Android:
            case RuntimePlatform.WindowsEditor:
                mobileUI.gameObject.SetActive(true);
                break;
            default:
                mobileUI.gameObject.SetActive(false);
                break;
        }

        targetFocusX = toolContainer.transform.GetChild(0).transform.position.x;
        toolFocus.transform.position = new Vector3(
            targetFocusX,
            toolFocus.transform.position.y
        );
        // weaponNameText.enabled = false;
        // weaponAmmunitionText.enabled = false;

        // Hide the sniper aim
        sniperAim.SetActive(false);

        // Hide the alert text
        alertText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {

        toolFocus.transform.position = new Vector3(
            Mathf.Lerp(toolFocus.transform.position.x, targetFocusX, Time.deltaTime * focusSmoothness),
            toolFocus.transform.position.y
        );
    }

    public void UpdateResourcesRequirement(int cost, int balance) {
        resourcesRequirementText.text = "Requires: " + cost;
        if (cost > balance)
        {
            resourcesRequirementText.color = Color.red;
        }
        else {
            resourcesRequirementText.color = Color.white;
        }
    }

    public void UpdateWeapon(Weapon weapon) {
        if (weapon == null)
        {
            weaponNameText.enabled = false;
            weaponAmmunitionText.enabled = false;
            weaponReloadBar.localScale = new Vector3(0, 1, 1);
        }
        else {
            weaponNameText.text = weapon.Name;
            weaponAmmunitionText.text = weapon.ClipAmmunition + " / " + weapon.TotalAmmunition;
            weaponNameText.enabled = true;
            weaponAmmunitionText.enabled = true;

            if (weapon.ReloadTimer > 0) {
                weaponReloadBar.localScale = new Vector3(weapon.ReloadTimer / weapon.ReloadDuration, 1, 1);
            } else {
                weaponReloadBar.localScale = new Vector3(0, 1, 1);
            }
        }
    }
    public void UpdateHealthBar(float health)
    {
        healthBar.localScale = new Vector3(health, 1, 1);
    }

    public void ShowScreen(string screenName) {
        regularScreen.SetActive(screenName == "regular");
        gameOverScreen.SetActive(screenName == "gameOver");
        serverScreen.SetActive(screenName == "server");
        clientScreen.SetActive(screenName == "client");
        spawnScreen.SetActive(screenName == "spawn");
    }

    public void OnPressedStartMatch() {
        if (OnStartMatch != null) {
            OnStartMatch();
        }
    }

    public void Alert() {
        alertText.gameObject.SetActive(true);
        Invoke("HideAlert", 3);
    }

    public void HideAlert() {
        alertText.gameObject.SetActive(false);
    }

    public void OnPressedRestart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        if (isServer) ShowScreen("server");
        else if (isClient) ShowScreen("client");
        Cursor.lockState = CursorLockMode.None;
    }
}
