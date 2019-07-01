using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Text resourcesText;

    public int Resources {
        set {
            resourcesText.text = "Resources: " + value;
        }
    }

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}
}
