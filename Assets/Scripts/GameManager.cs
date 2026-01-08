using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Anatidae;

public class GameManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel; // Panel UI pour le game over
    [SerializeField] private TextMeshProUGUI gameOverText; // Texte du game over
    [SerializeField] private bool pauseGameOnGameOver = true;

    [Header("Victory Settings")]
    [SerializeField] private GameObject victoryPanel; // Panel UI pour la victoire
    [SerializeField] private TextMeshProUGUI victoryTimeText; // Affichage du temps final

    [Header("Game Over Actions")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    private bool gameIsOver = false;
    private bool hasWon = false;

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
        // Cacher les panels au départ
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Charger et appliquer les paramètres du jeu
        StartCoroutine(InitializeGameSettings());
    }

    IEnumerator InitializeGameSettings()
    {
        yield return PoliceSpeedManager.FetchAndApplyPoliceSpeed();
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

    public bool HasWon()
    {
        return hasWon;
    }

    /// <summary>
    /// Déclenche la victoire et envoie le score à l'API
    /// </summary>
    public void TriggerVictory()
    {
        if (gameIsOver)
            return; // Éviter de déclencher plusieurs fois

        gameIsOver = true;
        hasWon = true;

        Debug.Log("🎉 VICTOIRE!");

        // Arrêter le timer et récupérer le temps final
        float finalTime = 0f;
        if (SpeedrunTimer.Instance != null)
        {
            SpeedrunTimer.Instance.PauseTimer();
            finalTime = SpeedrunTimer.Instance.ElapsedTime;
            Debug.Log($"Temps final: {SpeedrunTimer.FormatTime(finalTime)}");
        }

        // Afficher le panel de victoire
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // Afficher le temps
            if (victoryTimeText != null)
            {
                victoryTimeText.text = $"Temps: {SpeedrunTimer.FormatTime(finalTime)}";
            }
        }

        // Convertir le temps en centisecondes pour l'API
        int timeInCentiseconds = Mathf.RoundToInt(finalTime * 100f);

        // Envoyer le score à l'API
        StartCoroutine(ProcessHighscore(timeInCentiseconds));
    }

    /// <summary>
    /// Vérifie si c'est un highscore et affiche l'écran de saisie du nom si nécessaire
    /// </summary>
    IEnumerator ProcessHighscore(int time)
    {
        Debug.Log($"🔄 Vérification du highscore pour {time} centisecondes...");

        // Récupérer les highscores depuis l'API
        yield return HighscoreManager.FetchHighscores();

        if (HighscoreManager.HasFetchedHighscores)
        {
            // Vérifier si c'est un highscore
            if (HighscoreManager.IsHighscore(time))
            {
                Debug.Log("🏆 NOUVEAU HIGHSCORE!");

                // Cacher le panel de victoire
                if (victoryPanel != null)
                {
                    victoryPanel.SetActive(false);
                }

                // Afficher l'écran de saisie du nom
                HighscoreManager.ShowHighscoreInput(time);
            }
            else
            {
                Debug.Log("Pas un highscore cette fois, mais bien joué!");
            }
        }
        else
        {
            Debug.LogError("❌ Impossible de récupérer les highscores depuis l'API. Vérifiez que le backend est démarré!");
        }
    }
}
