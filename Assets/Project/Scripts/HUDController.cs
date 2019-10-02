using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class HUDController : NetworkBehaviour
{
    public delegate void StartMatchHandler();
    public event StartMatchHandler OnStartMatch;

    [Header("Screens")]
    [SerializeField] protected GameObject regularScreen;
    [SerializeField] protected GameObject gameOverScreen;

    [Header("Interface Elements")]
    [SerializeField] protected Text healthText;
    [SerializeField] protected Text resourcesText;
    [SerializeField] protected Text resourcesRequirementText;
    [SerializeField] protected Text weaponNameText;
    [SerializeField] protected Text weaponAmmunitionText;
    [SerializeField] protected RectTransform weaponReloadBar;
    [SerializeField] protected RectTransform healthBar;
    [SerializeField] protected GameObject sniperAim;
    [SerializeField] protected Text serverPlayersText;
    [SerializeField] protected Text clientPlayersText;
    [SerializeField] protected Text alertText;
    [SerializeField] protected RectTransform mobileUI;
    [SerializeField] protected RectTransform turnAndLookTouchpad;
    [SerializeField] protected RectTransform buttonSwitchTool;

    [Header("Tool Selector")]
    [SerializeField] protected GameObject toolFocus;
    [SerializeField] protected GameObject toolContainer;
    [SerializeField] protected float focusSmoothness = 10.0f;

    protected float targetFocusX = 0;

    public float Health
    {
        set
        {
            healthText.text = "Health: " + Mathf.CeilToInt(value);
        }
    }

    public int Resources
    {
        set
        {
            resourcesText.text = "Resources: " + value;
        }
    }

    public virtual int Players { set { } }

    public Player.PlayerTool Tool
    {
        set
        {
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
    protected void Start()
    {
        ShowScreen("");

        // Display mobile UI only on mobile devices
        switch (Application.platform)
        {
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
    protected void Update()
    {
        toolFocus.transform.position = new Vector3(
            Mathf.Lerp(toolFocus.transform.position.x, targetFocusX, Time.deltaTime * focusSmoothness),
            toolFocus.transform.position.y
        );
    }

    public void UpdateResourcesRequirement(int cost, int balance)
    {
        resourcesRequirementText.text = "Requires: " + cost;
        if (cost > balance)
        {
            resourcesRequirementText.color = Color.red;
        }
        else
        {
            resourcesRequirementText.color = Color.white;
        }
    }

    public void UpdateWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            weaponNameText.enabled = false;
            weaponAmmunitionText.enabled = false;
            weaponReloadBar.localScale = new Vector3(0, 1, 1);
        }
        else
        {
            weaponNameText.text = weapon.Name;
            weaponAmmunitionText.text = weapon.ClipAmmunition + " / " + weapon.TotalAmmunition;
            weaponNameText.enabled = true;
            weaponAmmunitionText.enabled = true;

            if (weapon.ReloadTimer > 0)
            {
                weaponReloadBar.localScale = new Vector3(weapon.ReloadTimer / weapon.ReloadDuration, 1, 1);
            }
            else
            {
                weaponReloadBar.localScale = new Vector3(0, 1, 1);
            }
        }
    }
    public void UpdateHealthBar(float health)
    {
        healthBar.localScale = new Vector3(health, 1, 1);
    }

    public virtual void ShowScreen(string screenName)
    {
        regularScreen.SetActive(screenName == "regular");
        gameOverScreen.SetActive(screenName == "gameOver");
    }

    public virtual void OnPressedStartMatch()
    {
        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.OnServerStartMatch();
            player.RpcAllowMovement();
        }
    }

    public void Alert()
    {
        alertText.gameObject.SetActive(true);
        Invoke("HideAlert", 3);
    }

    public void HideAlert()
    {
        alertText.gameObject.SetActive(false);
    }
}
