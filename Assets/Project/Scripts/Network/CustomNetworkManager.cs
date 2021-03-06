﻿using System.Collections;
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

    private HUDControllerNetwork hud;

    public override void Awake() {
        hud = FindObjectOfType<HUDControllerNetwork>();
        if (networkAddressInput) networkAddressInput.GetComponent<Text>().text = "192.168.0.15";
        base.Awake();
    }

    public void CustomStartHost() {
        string newIP = networkAddressInput.GetComponent<Text>().text;
        Debug.Log("Network Address: " + newIP);
        networkAddress = newIP;
        base.StartHost();
    }

    void SpawnObjects() {
        foreach (Spawner spawner in FindObjectsOfType<Spawner>()) {
            spawner.Start();
        }
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
