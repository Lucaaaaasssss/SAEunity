using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Configure automatiquement le timer en haut à gauche au démarrage de la scène
/// Crée le Canvas et le TextMesh si nécessaire
/// </summary>
public class AutoTimerSetup : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Vector2 soloPosition = new Vector2(30f, -30f); // Position en mode Solo
    [SerializeField] private Vector2 duoPosition = new Vector2(280f, -30f); // Position en mode Duo (décalé à droite du cadre rouge)
    [SerializeField] private float fontSize = 40f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private string prefix = "TIME: ";

    private Vector2 currentPosition => (GameModeManager.Instance != null && GameModeManager.Instance.IsDuo()) ? duoPosition : soloPosition;

    void Awake()
    {
        SetupTimer();
    }

    void Start()
    {
        // Re-forcer la configuration au Start au cas où
        TimerDisplay timer = FindObjectOfType<TimerDisplay>();
        if (timer != null && timer.TryGetComponent<TextMeshProUGUI>(out var text))
        {
            ConfigureTimer(text);
        }
    }

    void SetupTimer()
    {
        // 1. Trouver ou créer le Canvas
        Canvas canvas = FindCanvasOrCreate();

        // 2. Trouver ou créer le Timer TextMesh
        TextMeshProUGUI timerText = FindTimerOrCreate(canvas);

        // 3. Configurer le Timer
        ConfigureTimer(timerText);

        // 4. Ajouter le TimerDisplay si pas présent
        if (timerText.GetComponent<TimerDisplay>() == null)
        {
            TimerDisplay display = timerText.gameObject.AddComponent<TimerDisplay>();

            // Configurer le TimerDisplay via reflection pour set les private fields
            var prefixField = typeof(TimerDisplay).GetField("prefix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prefixField != null)
            {
                prefixField.SetValue(display, prefix);
            }
        }

        Debug.Log("✅ Timer configuré automatiquement en haut à gauche !");
    }

    Canvas FindCanvasOrCreate()
    {
        // Chercher un Canvas existant
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.Log("Aucun Canvas trouvé, création d'un nouveau...");

            // Créer un nouveau Canvas
            GameObject canvasObj = new GameObject("UI_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Au-dessus de tout

            // Ajouter le CanvasScaler pour l'adaptation d'écran
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Ajouter le GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("✅ Canvas créé !");
        }
        else
        {
            // S'assurer qu'il est en Screen Space Overlay
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            Debug.Log("✅ Canvas existant trouvé et configuré !");
        }

        return canvas;
    }

    TextMeshProUGUI FindTimerOrCreate(Canvas canvas)
    {
        // Chercher un Timer existant
        TimerDisplay existingTimer = FindObjectOfType<TimerDisplay>();

        if (existingTimer != null && existingTimer.TryGetComponent<TextMeshProUGUI>(out var existingText))
        {
            Debug.Log("✅ Timer existant trouvé !");
            return existingText;
        }

        // Créer un nouveau Timer
        Debug.Log("Création d'un nouveau Timer...");

        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();

        Debug.Log("✅ Timer créé !");
        return timerText;
    }

    void ConfigureTimer(TextMeshProUGUI timerText)
    {
        RectTransform rectTransform = timerText.GetComponent<RectTransform>();

        // Configuration du RectTransform - HAUT GAUCHE
        rectTransform.anchorMin = new Vector2(0, 1); // Coin haut-gauche
        rectTransform.anchorMax = new Vector2(0, 1); // Coin haut-gauche
        rectTransform.pivot = new Vector2(0, 1); // Pivot haut-gauche
        rectTransform.anchoredPosition = currentPosition;
        rectTransform.sizeDelta = new Vector2(400, 80);

        // Configuration du texte
        timerText.fontSize = fontSize;
        timerText.color = textColor;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.text = prefix + "00:00.00";

        // Outline pour la lisibilité
        if (timerText.GetComponent<Outline>() == null)
        {
            Outline outline = timerText.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
        }

        Debug.Log($"✅ Timer configuré : Position={currentPosition}, FontSize={fontSize}");
    }

    void LateUpdate()
    {
        // Forcer la position à chaque frame (ajustée selon le mode Solo/Duo)
        TimerDisplay timer = FindObjectOfType<TimerDisplay>();
        if (timer != null && timer.TryGetComponent<TextMeshProUGUI>(out var text))
        {
            RectTransform rect = text.GetComponent<RectTransform>();

            // Forcer les anchors et la position
            if (rect.anchorMin != new Vector2(0, 1) || rect.anchorMax != new Vector2(0, 1))
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
            }

            if (rect.anchoredPosition != currentPosition)
            {
                rect.anchoredPosition = currentPosition;
            }
        }
    }

    void OnValidate()
    {
        // Si on change les valeurs dans l'Inspector pendant le jeu, mettre à jour
        if (Application.isPlaying)
        {
            TimerDisplay timer = FindObjectOfType<TimerDisplay>();
            if (timer != null && timer.TryGetComponent<TextMeshProUGUI>(out var text))
            {
                text.fontSize = fontSize;
                text.color = textColor;
                text.GetComponent<RectTransform>().anchoredPosition = currentPosition;
            }
        }
    }
}
