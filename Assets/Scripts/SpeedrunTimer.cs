using UnityEngine;

/// <summary>
/// Gestionnaire du timer de speedrun (singleton)
/// Suit le temps écoulé depuis le début de la partie
/// </summary>
public class SpeedrunTimer : MonoBehaviour
{
    public static SpeedrunTimer Instance { get; private set; }

    [Header("Timer State")]
    [SerializeField] private bool autoStartOnPlay = false;
    [SerializeField] private bool isRunning = false;

    private float elapsedTime = 0f;

    public delegate void TimerUpdateDelegate(float time);
    public event TimerUpdateDelegate OnTimerUpdate;

    public float ElapsedTime => elapsedTime;
    public bool IsRunning => isRunning;

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
        if (autoStartOnPlay)
        {
            StartTimer();
        }
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            OnTimerUpdate?.Invoke(elapsedTime);
        }
    }

    /// <summary>
    /// Démarre ou reprend le timer
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// Met le timer en pause
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Réinitialise le timer à zéro
    /// </summary>
    public void ResetTimer()
    {
        elapsedTime = 0f;
        OnTimerUpdate?.Invoke(elapsedTime);
    }

    /// <summary>
    /// Arrête et réinitialise complètement le timer
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        ResetTimer();
    }

    /// <summary>
    /// Formate le temps en MM:SS.CC
    /// </summary>
    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds = Mathf.FloorToInt((time * 100f) % 100f);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }

    /// <summary>
    /// Obtient le temps formaté actuel
    /// </summary>
    public string GetFormattedTime()
    {
        return FormatTime(elapsedTime);
    }
}
