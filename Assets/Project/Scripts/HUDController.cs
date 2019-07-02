using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Interface Elements")]
    [SerializeField] private Text resourcesText = null;

    [Header("Tool Selector")]
    [SerializeField] private GameObject toolFocus = null;
    [SerializeField] private GameObject[] tools = null;

    public int Resources {
        set {
            resourcesText.text = "Resources: " + value;
        }
    }

    public Player.PlayerTool Tool {
        set {
            toolFocus.transform.position = new Vector3(
                tools[(int)value].transform.position.x,
                toolFocus.transform.position.y
            );
        }
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {}
}
