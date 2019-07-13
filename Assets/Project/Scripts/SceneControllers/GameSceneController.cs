using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private Player player;

    private float gameOverCooldownDuration = 3.0f;
    private float gameOverCooldownTimer;

    // Start is called before the first frame update
    void Start()
    {
        player.OnPlayerDied += OnPlayerDied;
        gameOverCooldownTimer = gameOverCooldownDuration;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) {
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
