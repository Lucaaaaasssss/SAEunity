using UnityEngine;
using TMPro;

/// <summary>
/// Affiche le timer de speedrun à l'écran
/// Nécessite un composant TextMeshProUGUI
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private string prefix = "TIME: ";
    [SerializeField] private bool showPrefix = true;

    [Header("Color Settings")]
    [SerializeField] private bool useColorGradient = false;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 60f; // 1 minute
    [SerializeField] private float dangerThreshold = 120f; // 2 minutes

    private TextMeshProUGUI timerText;
    private bool isSubscribed = false;

    void Awake()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        Debug.Log("TimerDisplay: Awake - TextMeshProUGUI trouvé: " + (timerText != null));
    }

    void OnEnable()
    {
        TrySubscribeToTimer();
    }

    void OnDisable()
    {
        // Se désabonner pour éviter les erreurs
        if (SpeedrunTimer.Instance != null && isSubscribed)
        {
            SpeedrunTimer.Instance.OnTimerUpdate -= UpdateDisplay;
            isSubscribed = false;
        }
    }

    void Start()
    {
        TrySubscribeToTimer();
    }

    void Update()
    {
        // Vérifier constamment si on peut s'abonner au timer
        if (!isSubscribed)
        {
            TrySubscribeToTimer();
        }

        // Mise à jour manuelle si nécessaire (fallback)
        if (SpeedrunTimer.Instance != null && timerText != null)
        {
            UpdateDisplay(SpeedrunTimer.Instance.ElapsedTime);
        }
    }

    private void TrySubscribeToTimer()
    {
        if (SpeedrunTimer.Instance != null && !isSubscribed)
        {
            SpeedrunTimer.Instance.OnTimerUpdate += UpdateDisplay;
            UpdateDisplay(SpeedrunTimer.Instance.ElapsedTime);
            isSubscribed = true;
            Debug.Log("TimerDisplay: Abonné au SpeedrunTimer!");
        }
        else if (SpeedrunTimer.Instance == null)
        {
            // Afficher un message temporaire
            if (timerText != null)
            {
                timerText.text = "00:00.00";
            }
        }
    }

    /// <summary>
    /// Met à jour l'affichage du temps
    /// </summary>
    private void UpdateDisplay(float time)
    {
        if (timerText == null) return;

        string formattedTime = SpeedrunTimer.FormatTime(time);
        timerText.text = showPrefix ? prefix + formattedTime : formattedTime;

        // Appliquer les couleurs si activé
        if (useColorGradient)
        {
            if (time >= dangerThreshold)
            {
                timerText.color = dangerColor;
            }
            else if (time >= warningThreshold)
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
    }

    /// <summary>
    /// Change le préfixe du timer
    /// </summary>
    public void SetPrefix(string newPrefix)
    {
        prefix = newPrefix;
        if (SpeedrunTimer.Instance != null)
        {
            UpdateDisplay(SpeedrunTimer.Instance.ElapsedTime);
        }
    }

    /// <summary>
    /// Active/désactive le préfixe
    /// </summary>
    public void SetShowPrefix(bool show)
    {
        showPrefix = show;
        if (SpeedrunTimer.Instance != null)
        {
            UpdateDisplay(SpeedrunTimer.Instance.ElapsedTime);
        }
    }
}
