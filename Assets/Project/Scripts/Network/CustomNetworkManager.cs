using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [Header("UI Info")]
    [SerializeField] private RectTransform networkAddressInput;

    private HUDController hud;

    public override void Awake() {
        hud = FindObjectOfType<HUDController>();
        networkAddressInput.GetComponent<Text>().text = "192.168.0.10";
        base.Awake();
    }

    public void CustomStartHost() {
        string newIP = networkAddressInput.GetComponent<Text>().text;
        Debug.Log("Network Address: " + newIP);
        networkAddress = newIP;
        base.StartHost();
    }

    public void CustomStartClient()
    {
        string newIP = networkAddressInput.GetComponent<Text>().text;
        Debug.Log("CustomStartClient IP: " + newIP);
        networkAddress = newIP;
        base.StartClient();
    }

    public void SetNetworkAddress(string ip) {
        networkAddress = ip;
    }

    public override void OnStopHost() {
        hud.ShowScreen("multiplayer");
        base.OnStopHost();
    }
}
