using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script pour le menu principal
/// Permet de sélectionner le mode de jeu (1 joueur ou 2 joueurs) avant de lancer une partie
/// </summary>
public class MenuModeSelector : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "MainScene"; // Nom de la scène de jeu

    /// <summary>
    /// Lance le jeu en mode 1 joueur (Solo)
    /// À attacher à un bouton "1 Joueur" dans le menu
    /// </summary>
    public void StartSinglePlayer()
    {
        Debug.Log("<® Démarrage du mode 1 joueur");

        // Créer le GameModeManager s'il n'existe pas
        CreateGameModeManager();

        // Définir le mode Solo
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetMode(GameMode.Solo);
        }

        // Charger la scène de jeu
        LoadGameScene();
    }

    /// <summary>
    /// Lance le jeu en mode 2 joueurs (Duo)
    /// À attacher à un bouton "2 Joueurs" dans le menu
    /// </summary>
    public void StartTwoPlayers()
    {
        Debug.Log("<® Démarrage du mode 2 joueurs");

        // Créer le GameModeManager s'il n'existe pas
        CreateGameModeManager();

        // Définir le mode Duo
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetMode(GameMode.Duo);
        }

        // Charger la scène de jeu
        LoadGameScene();
    }

    /// <summary>
    /// Crée le GameModeManager s'il n'existe pas déjà
    /// </summary>
    void CreateGameModeManager()
    {
        if (GameModeManager.Instance == null)
        {
            GameObject gmm = new GameObject("GameModeManager");
            gmm.AddComponent<GameModeManager>();
            Debug.Log(" GameModeManager créé");
        }
    }

    /// <summary>
    /// Charge la scène de jeu
    /// </summary>
    void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("L Nom de scène de jeu non défini dans MenuModeSelector!");
        }
    }

    /// <summary>
    /// Quitte le jeu
    /// À attacher à un bouton "Quitter"
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("=K Quitter le jeu");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
