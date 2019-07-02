using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Interface Elements")]
    [SerializeField] private Text resourcesText;

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
            targetFocusX = toolContainer.transform.GetChild((int)value).transform.position.x;
        }
    }

    // Start is called before the first frame update
    void Start() {
        targetFocusX = toolContainer.transform.GetChild(0).transform.position.x;
        toolFocus.transform.position = new Vector3(
            targetFocusX,
            toolFocus.transform.position.y
        );
    }

    // Update is called once per frame
    void Update() {

        toolFocus.transform.position = new Vector3(
            Mathf.Lerp(toolFocus.transform.position.x, targetFocusX, Time.deltaTime * focusSmoothness),
            toolFocus.transform.position.y
        );
    }
}
