using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Interface Elements")]
    [SerializeField] private Text resourcesText;
    [SerializeField] private Text resourcesRequirementText;
    [SerializeField] private Text weaponNameText;
    [SerializeField] private Text weaponAmmunitionText;

    [Header("Tool Selector")]
    [SerializeField] private GameObject toolFocus;
    [SerializeField] private GameObject toolContainer;
    [SerializeField] private float focusSmoothness = 10.0f;

    private float targetFocusX = 0;

    public int Resources {
        set {
            resourcesText.text = "Resources: " + value;
        }
    }

    public Player.PlayerTool Tool {
        set {
            if (value != Player.PlayerTool.None) {
                toolFocus.SetActive(true);
                targetFocusX = toolContainer.transform.GetChild((int)value).transform.position.x;
            } else {
                toolFocus.SetActive(false);
            }
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

    // Start is called before the first frame update
    void Start() {
        targetFocusX = toolContainer.transform.GetChild(0).transform.position.x;
        toolFocus.transform.position = new Vector3(
            targetFocusX,
            toolFocus.transform.position.y
        );
        // weaponNameText.enabled = false;
        // weaponAmmunitionText.enabled = false;
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
        }
        else {
            weaponNameText.text = weapon.Name;
            weaponAmmunitionText.text = weapon.ClipAmmunition + " / " + weapon.TotalAmmunition;
            weaponNameText.enabled = true;
            weaponAmmunitionText.enabled = true;
        }
    }
}
