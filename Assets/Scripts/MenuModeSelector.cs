using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script pour le menu principal
/// Permet de s�lectionner le mode de jeu (1 joueur ou 2 joueurs) avant de lancer une partie
/// </summary>
public class MenuModeSelector : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "MainScene"; // Nom de la sc�ne de jeu

    /// <summary>
    /// Lance le jeu en mode 1 joueur (Solo)
    /// � attacher � un bouton "1 Joueur" dans le menu
    /// </summary>
    public void StartSinglePlayer()
    {
        Debug.Log("<� D�marrage du mode 1 joueur");

        // Cr�er le GameModeManager s'il n'existe pas
        CreateGameModeManager();

        // D�finir le mode Solo
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetMode(GameMode.Solo);
            GameModeManager.Instance.ShouldShowControls = true; // Afficher l'�cran des commandes
        }

        // Charger la sc�ne de jeu
        LoadGameScene();
    }

    /// <summary>
    /// Lance le jeu en mode 2 joueurs (Duo)
    /// � attacher � un bouton "2 Joueurs" dans le menu
    /// </summary>
    public void StartTwoPlayers()
    {
        Debug.Log("<� D�marrage du mode 2 joueurs");

        // Cr�er le GameModeManager s'il n'existe pas
        CreateGameModeManager();

        // D�finir le mode Duo
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetMode(GameMode.Duo);
            GameModeManager.Instance.ShouldShowControls = true; // Afficher l'�cran des commandes
        }

        // Charger la sc�ne de jeu
        LoadGameScene();
    }

    /// <summary>
    /// Cr�e le GameModeManager s'il n'existe pas d�j�
    /// </summary>
    void CreateGameModeManager()
    {
        if (GameModeManager.Instance == null)
        {
            GameObject gmm = new GameObject("GameModeManager");
            gmm.AddComponent<GameModeManager>();
            Debug.Log(" GameModeManager cr��");
        }
    }

    /// <summary>
    /// Charge la sc�ne de jeu
    /// </summary>
    void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("L Nom de sc�ne de jeu non d�fini dans MenuModeSelector!");
        }
    }

    /// <summary>
    /// Quitte le jeu
    /// � attacher � un bouton "Quitter"
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
