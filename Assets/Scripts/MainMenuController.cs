using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Anatidae;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject highscoresPanel;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "MainScene";

    [Header("Sound")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioSource audioSource;

    private enum MenuState
    {
        MainMenu,
        ModeSelect,
        Highscores
    }

    private MenuState currentState = MenuState.MainMenu;

    void Start()
    {
        // S'assurer que GameModeManager existe
        if (GameModeManager.Instance == null)
        {
            GameObject gmm = new GameObject("GameModeManager");
            gmm.AddComponent<GameModeManager>();
        }

        ShowMainMenu();
    }

    void Update()
    {
        switch (currentState)
        {
            case MenuState.MainMenu:
                HandleMainMenuInput();
                break;
            case MenuState.ModeSelect:
                HandleModeSelectInput();
                break;
            case MenuState.Highscores:
                HandleHighscoresInput();
                break;
        }
    }

    void HandleMainMenuInput()
    {
        // B1 ou F = Jouer (va à l'écran de sélection Solo/Duo)
        if (Input.GetButtonDown("P1_B1") || Input.GetKeyDown(KeyCode.F))
        {
            PlayClickSound();
            ShowModeSelect();
        }
        // B4 ou R = Highscores
        else if (Input.GetButtonDown("P1_B4") || Input.GetKeyDown(KeyCode.R))
        {
            PlayClickSound();
            ShowHighscores();
        }
        // Échap = Quitter
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayClickSound();
            QuitGame();
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    void QuitGame()
    {
        Debug.Log("Quitting game...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void HandleModeSelectInput()
    {
        // B1 ou F = Solo
        if (Input.GetButtonDown("P1_B1") || Input.GetKeyDown(KeyCode.F))
        {
            PlayClickSound();
            StartGame(GameMode.Solo);
        }
        // B4 ou R = Duo
        else if (Input.GetButtonDown("P1_B4") || Input.GetKeyDown(KeyCode.R))
        {
            PlayClickSound();
            StartGame(GameMode.Duo);
        }
        // B3 ou Echap = Retour
        else if (Input.GetButtonDown("P1_B3") || Input.GetKeyDown(KeyCode.Escape))
        {
            PlayClickSound();
            ShowMainMenu();
        }
    }

    void HandleHighscoresInput()
    {
        // N'importe quel bouton pour revenir
        if (Input.GetButtonDown("P1_B1") || Input.GetButtonDown("P1_B2") || Input.GetButtonDown("P1_B3") ||
            Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.R))
        {
            PlayClickSound();
            ShowMainMenu();
        }
    }

    public void ShowMainMenu()
    {
        currentState = MenuState.MainMenu;
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(highscoresPanel, false);
        HighscoreManager.HideHighscores();
        Debug.Log("Menu: Main Menu");
    }

    public void ShowModeSelect()
    {
        currentState = MenuState.ModeSelect;
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, true);
        SetPanelActive(highscoresPanel, false);
        Debug.Log("Menu: Mode Select");
    }

    public void ShowHighscores()
    {
        currentState = MenuState.Highscores;
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(modeSelectPanel, false);
        SetPanelActive(highscoresPanel, true);
        HighscoreManager.ShowHighscores();
        Debug.Log("Menu: Highscores");
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    void StartGame(GameMode mode)
    {
        GameModeManager.Instance.SetMode(mode);
        GameModeManager.Instance.ShouldShowControls = true; // Afficher l'écran des commandes
        Debug.Log($"Starting game in {mode} mode");
        SceneManager.LoadScene(gameSceneName);
    }

    // Méthodes publiques pour les boutons UI
    public void OnPlayButtonClick()
    {
        ShowModeSelect();
    }

    public void OnSoloButtonClick()
    {
        StartGame(GameMode.Solo);
    }

    public void OnDuoButtonClick()
    {
        StartGame(GameMode.Duo);
    }

    public void OnHighscoresButtonClick()
    {
        ShowHighscores();
    }

    public void OnBackButtonClick()
    {
        ShowMainMenu();
    }

    public void OnQuitButtonClick()
    {
        QuitGame();
    }
}
