using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;

public class HUDControllerNetwork : HUDController
{
    [Header("Screens")]
    [SerializeField] protected GameObject serverScreen;
    [SerializeField] protected GameObject clientScreen;
    [SerializeField] protected GameObject spawnScreen;
    [SerializeField] protected GameObject multiplayerScreen;

    public override int Players
    {
        set
        {
            serverPlayersText.text = "Players: " + value;
            clientPlayersText.text = "Players: " + value;
        }
    }

    public override void ShowScreen(string screenName)
    {
        regularScreen.SetActive(screenName == "regular");
        gameOverScreen.SetActive(screenName == "gameOver");
        serverScreen.SetActive(screenName == "server");
        clientScreen.SetActive(screenName == "client");
        spawnScreen.SetActive(screenName == "spawn");
        multiplayerScreen.SetActive(screenName == "multiplayer");
    }
}
