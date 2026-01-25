using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Anatidae;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel; // Panel UI pour le game over
    [SerializeField] private TextMeshProUGUI gameOverText; // Texte du game over
    [SerializeField] private Image retourImage; // Image RETOUR.PNG
    [SerializeField] private Vector2 retourImageSize = new Vector2(200f, 200f); // Taille de l'image RETOUR
    [SerializeField] private bool pauseGameOnGameOver = true;

    [Header("Victory Settings")]
    [SerializeField] private GameObject victoryPanel; // Panel UI pour la victoire

    [Header("Game Over Actions")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    private bool gameIsOver = false;
    private bool hasWon = false;
    private int finalTimeInCentiseconds = 0; // Stocke le temps pour l'enregistrement

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

        // Cacher l'image RETOUR au départ
        if (retourImage != null)
        {
            retourImage.gameObject.SetActive(false);
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
        // Si le jeu est terminé, écouter les touches
        if (gameIsOver)
        {
            // Si c'est une victoire
            if (hasWon)
            {
                // P1_B3 = Relancer une partie
                if (Input.GetButtonDown("P1_B3") || Input.GetKeyDown(restartKey))
                {
                    RestartGame();
                }
                // P1_B6 = Enregistrer son temps
                else if (Input.GetButtonDown("P1_B6"))
                {
                    SaveScore();
                }
            }
            // Si c'est un game over (défaite)
            else
            {
                if (Input.GetButtonDown("P1_B3") || Input.GetKeyDown(restartKey))
                {
                    RestartGame();
                }
                else if (Input.GetKeyDown(quitKey))
                {
                    QuitGame();
                }
            }
        }
    }

    /// <summary>
    /// Affiche l'écran de saisie du pseudo pour enregistrer le score
    /// </summary>
    private void SaveScore()
    {
        // Cacher le panel de victoire
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Afficher l'écran de saisie du pseudo
        Anatidae.HighscoreManager.ShowHighscoreInput(finalTimeInCentiseconds);
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

        // Afficher "vous avez été repéré"
        if (gameOverText != null)
        {
            gameOverText.text = "vous avez été repéré";
        }

        // Afficher l'image RETOUR.PNG
        if (retourImage != null)
        {
            retourImage.preserveAspect = true;
            RectTransform rectTransform = retourImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = retourImageSize;
            }
            retourImage.gameObject.SetActive(true);
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
        
         Application.Quit();

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
    /// Déclenche la victoire et affiche l'écran de victoire
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

        // Stocker le temps pour l'enregistrement ultérieur
        finalTimeInCentiseconds = Mathf.RoundToInt(finalTime * 100f);

        // Afficher le panel de victoire
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Mettre le jeu en pause
        Time.timeScale = 0f;
    }

}
