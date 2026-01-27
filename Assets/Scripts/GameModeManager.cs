using UnityEngine;

public enum GameMode
{
    Solo,
    Duo
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public GameMode CurrentMode { get; private set; } = GameMode.Solo;

    // Indique si on doit afficher l'écran des commandes (true = première partie depuis le menu)
    public bool ShouldShowControls { get; set; } = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
        Debug.Log($"Game mode set to: {mode}");
    }

    public bool IsSolo()
    {
        return CurrentMode == GameMode.Solo;
    }

    public bool IsDuo()
    {
        return CurrentMode == GameMode.Duo;
    }
}
