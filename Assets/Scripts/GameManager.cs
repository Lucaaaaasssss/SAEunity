using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
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
    [SerializeField] private AudioClip detectionSound; // Son quand on se fait repérer

    [Header("Victory Settings")]
    [SerializeField] private GameObject victoryPanel; // Panel UI pour la victoire
    [SerializeField] private GameObject scoreSubmittedPanel; // Panel affiché après envoi du score
    [SerializeField] private AudioClip victorySound; // Son de victoire

    [Header("Controls Screen")]
    [SerializeField] private GameObject controlsPanelSolo; // Panel des commandes pour le mode Solo
    [SerializeField] private GameObject controlsPanelDuo; // Panel des commandes pour le mode Duo
    [SerializeField] private TextMeshProUGUI countdownTextSolo; // Texte du décompte pour le mode Solo
    [SerializeField] private TextMeshProUGUI countdownTextDuo; // Texte du décompte pour le mode Duo
    [SerializeField] private AudioClip countdownBeep; // Son pour chaque seconde (3, 2, 1)
    [SerializeField] private AudioClip countdownGo; // Son pour le départ (optionnel)
    [SerializeField] private AudioSource countdownAudioSource; // AudioSource pour jouer les sons

    [Header("Game Over Actions")]
    [SerializeField] private KeyCode quitKey = KeyCode.Escape;

    private bool gameIsOver = false;
    private bool hasWon = false;
    private int finalTimeInCentiseconds = 0; // Stocke le temps pour l'enregistrement
    private bool wasShowingHighscoreInput = false; // Pour détecter quand l'écran de saisie se ferme
    private bool scoreWasSubmitted = false; // Pour savoir si le score a été envoyé
    private bool isShowingControls = false; // Pour savoir si l'écran des commandes est affiché

    // Configuration API pour compter les parties
    private const string LOCAL_PROXY_URL = "http://localhost:3000/proxy";
    private const string VPS_BASE_URL = "https://lucaslebecq.fr/api";

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
        // Augmenter le sorting order du Canvas pour qu'il passe devant les bordures du split screen
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            // Chercher le Canvas dans les parents des panels
            if (gameOverPanel != null)
            {
                canvas = gameOverPanel.GetComponentInParent<Canvas>();
            }
        }

        if (canvas != null)
        {
            // Mettre un sorting order très élevé pour passer devant OnGUI
            canvas.sortingOrder = 1000;
            Debug.Log($"Canvas sorting order set to {canvas.sortingOrder} pour passer devant les bordures split screen");
        }

        // Cacher les panels au départ
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Cacher le panel de score envoyé au départ
        if (scoreSubmittedPanel != null)
        {
            scoreSubmittedPanel.SetActive(false);
        }

        // Cacher l'image RETOUR au départ
        if (retourImage != null)
        {
            retourImage.gameObject.SetActive(false);
        }

        // Cacher les panels des commandes au départ
        if (controlsPanelSolo != null)
        {
            controlsPanelSolo.SetActive(false);
        }
        if (controlsPanelDuo != null)
        {
            controlsPanelDuo.SetActive(false);
        }

        // Vérifier si on doit afficher l'écran des commandes (première partie depuis le menu)
        if (GameModeManager.Instance != null && GameModeManager.Instance.ShouldShowControls)
        {
            StartCoroutine(ShowControlsScreen());
        }
        else
        {
            // Démarrer normalement
            StartCoroutine(InitializeGameSettings());
            StartCoroutine(EnregistrerNouvellePartie());
        }
    }

    IEnumerator InitializeGameSettings()
    {
        yield return PoliceSpeedManager.FetchAndApplyPoliceSpeed();
    }

    /// <summary>
    /// Affiche l'écran des commandes avec un décompte avant de démarrer le jeu
    /// </summary>
    IEnumerator ShowControlsScreen()
    {
        isShowingControls = true;

        // Marquer qu'on a affiché les commandes (ne plus les afficher au prochain replay)
        GameModeManager.Instance.ShouldShowControls = false;

        // Mettre le jeu en pause pendant l'affichage des commandes
        Time.timeScale = 0f;

        // Déterminer quel panel et texte afficher selon le mode
        bool isSolo = GameModeManager.Instance.IsSolo();
        GameObject panelToShow = isSolo ? controlsPanelSolo : controlsPanelDuo;
        TextMeshProUGUI countdownText = isSolo ? countdownTextSolo : countdownTextDuo;

        // Afficher le panel des commandes approprié
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }

        // Décompte 3, 2, 1
        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }
            // Jouer le son du bip
            if (countdownAudioSource != null && countdownBeep != null)
            {
                countdownAudioSource.clip = countdownBeep;
                countdownAudioSource.Play();
            }
            yield return new WaitForSecondsRealtime(1f);
        }

        // Couper le son du bip
        if (countdownAudioSource != null)
        {
            countdownAudioSource.Stop();
        }

        // Jouer le son de départ (GO)
        if (countdownAudioSource != null && countdownGo != null)
        {
            countdownAudioSource.PlayOneShot(countdownGo);
        }

        // Cacher le panel des commandes
        if (panelToShow != null)
        {
            panelToShow.SetActive(false);
        }

        // Cacher le texte du décompte
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Réinitialiser le timer à 0 avant de démarrer le jeu
        if (SpeedrunTimer.Instance != null)
        {
            SpeedrunTimer.Instance.StopTimer();
        }

        // Remettre le jeu en marche
        Time.timeScale = 1f;
        isShowingControls = false;

        // Initialiser le jeu normalement
        StartCoroutine(InitializeGameSettings());
        StartCoroutine(EnregistrerNouvellePartie());
    }

    void Update()
    {
        // Détecter quand l'écran de saisie du pseudo se ferme (score envoyé)
        if (wasShowingHighscoreInput && !Anatidae.HighscoreManager.IsHighscoreInputScreenShown)
        {
            wasShowingHighscoreInput = false;
            scoreWasSubmitted = true;

            // Afficher le panel "score envoyé"
            if (scoreSubmittedPanel != null)
            {
                scoreSubmittedPanel.SetActive(true);
            }
        }

        // Mettre à jour le tracking de l'écran de saisie
        if (Anatidae.HighscoreManager.IsHighscoreInputScreenShown)
        {
            wasShowingHighscoreInput = true;
        }

        // Si le jeu est terminé, écouter les touches
        if (gameIsOver)
        {
            // Si le score a été envoyé
            if (scoreWasSubmitted)
            {
                // P1_B3 = Relancer une partie
                if (Input.GetButtonDown("P1_B3"))
                {
                    RestartGame();
                }
                // P1_B5 = Retourner au menu pour changer de mode
                else if (Input.GetButtonDown("P1_B5"))
                {
                    GoToMenu();
                }
                return;
            }

            // Si c'est une victoire
            if (hasWon)
            {
                // P1_B3 = Relancer une partie
                if (Input.GetButtonDown("P1_B3"))
                {
                    RestartGame();
                }
                // P1_B6 = Enregistrer son temps
                else if (Input.GetButtonDown("P1_B6"))
                {
                    SaveScore();
                }
                // P1_B5 = Retourner au menu pour changer de mode
                else if (Input.GetButtonDown("P1_B5"))
                {
                    GoToMenu();
                }
            }
            // Si c'est un game over (défaite)
            else
            {
                // P1_B3 = Relancer une partie
                if (Input.GetButtonDown("P1_B3"))
                {
                    RestartGame();
                }
                // P1_B5 = Retourner au menu pour changer de mode
                else if (Input.GetButtonDown("P1_B5"))
                {
                    GoToMenu();
                }
            }
        }
    }

    /// <summary>
    /// Affiche l'écran de saisie du pseudo pour enregistrer le score
    /// </summary>
    private void SaveScore()
    {
        Debug.Log("SaveScore() appelé - Temps: " + finalTimeInCentiseconds);

        // Remettre le temps normal pour que l'UI fonctionne
        Time.timeScale = 1f;

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

        // Jouer le son de détection
        if (countdownAudioSource != null && detectionSound != null)
        {
            countdownAudioSource.PlayOneShot(detectionSound);
        }

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

    public void GoToMenu()
    {
        // Rétablir le temps normal
        Time.timeScale = 1f;

        // Charger la scène du menu
        SceneManager.LoadScene("Menu");
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
    /// Enregistre une nouvelle partie jouée via l'API
    /// </summary>
    IEnumerator EnregistrerNouvellePartie()
    {
        string apiUrl = VPS_BASE_URL + "/?parametre=nouvelle_partie";
        string requestUrl = LOCAL_PROXY_URL + "?url=" + UnityWebRequest.EscapeURL(apiUrl);

        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Nouvelle partie enregistrée!");
        }
        else
        {
            Debug.LogWarning("Erreur lors de l'enregistrement de la partie: " + request.error);
        }
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

        // Jouer le son de victoire
        if (countdownAudioSource != null && victorySound != null)
        {
            countdownAudioSource.PlayOneShot(victorySound);
        }

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
