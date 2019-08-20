using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private HUDController hud;

    private float gameOverCooldownDuration = 3.0f;
    private float gameOverCooldownTimer;

    private Player player; // remove after implementing multiplayer

    // Start is called before the first frame update
    void Start()
    {
        gameOverCooldownTimer = gameOverCooldownDuration;
        hud.ShowScreen("");
    }

    // Update is called once per frame
    void Update()
    {
        if (false && player == null) {
            gameOverCooldownTimer -= Time.deltaTime;
            if (Input.anyKey && gameOverCooldownTimer <= 0)
            {
                ReloadScene();
            }
        }
    }
    private void OnPlayerDied() {
        // Invoke("ReloadScene", 10.0f);
    }

    private void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
