using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel; // Panel UI pour le game over
    [SerializeField] private TextMeshProUGUI gameOverText; // Texte du game over
    [SerializeField] private bool pauseGameOnGameOver = true;

    [Header("Game Over Actions")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    private bool gameIsOver = false;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Cacher le panel de game over au départ
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Si le jeu est terminé, écouter les touches pour restart/quit
        if (gameIsOver)
        {
            if (Input.GetKeyDown(restartKey))
            {
                RestartGame();
            }
            else if (Input.GetKeyDown(quitKey))
            {
                QuitGame();
            }
        }
    }

    public void TriggerGameOver(string reason = "Game Over!")
    {
        if (gameIsOver)
            return; // Éviter de déclencher plusieurs fois

        gameIsOver = true;

        Debug.Log($"Game Over: {reason}");

        // Arrêter le timer si présent
        if (SpeedrunTimer.Instance != null)
        {
            SpeedrunTimer.Instance.StopTimer();
        }

        // Afficher le panel de game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Mettre à jour le texte
        if (gameOverText != null)
        {
            gameOverText.text = reason;
        }

        // Mettre le jeu en pause
        if (pauseGameOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        // Rétablir le temps normal
        Time.timeScale = 1f;

        // Recharger la scène actuelle
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
        // Rétablir le temps normal
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool IsGameOver()
    {
        return gameIsOver;
    }
}
