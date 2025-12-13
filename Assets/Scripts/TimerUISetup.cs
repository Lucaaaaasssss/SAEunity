using UnityEngine;
using TMPro;

/// <summary>
/// Configure automatiquement le timer en haut à gauche de l'écran
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerUISetup : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f); // Offset depuis le coin haut-gauche
    [SerializeField] private TextAnchor anchor = TextAnchor.UpperLeft;

    [Header("Style")]
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private bool boldText = true;

    private TextMeshProUGUI textComponent;
    private RectTransform rectTransform;

    void Awake()
    {
        SetupTimer();
    }

    void SetupTimer()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        // Configuration du texte
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.fontStyle = boldText ? FontStyles.Bold : FontStyles.Normal;
        textComponent.alignment = TextAlignmentOptions.TopLeft;

        // Positionner en haut à gauche
        rectTransform.anchorMin = new Vector2(0, 1); // Coin haut-gauche
        rectTransform.anchorMax = new Vector2(0, 1); // Coin haut-gauche
        rectTransform.pivot = new Vector2(0, 1); // Pivot haut-gauche
        rectTransform.anchoredPosition = offset;

        // Taille du texte
        rectTransform.sizeDelta = new Vector2(300, 100);

        Debug.Log("✅ Timer configuré en haut à gauche !");
    }

    // Forcer la position à chaque frame (au cas où)
    void LateUpdate()
    {
        if (rectTransform.anchoredPosition != offset)
        {
            rectTransform.anchoredPosition = offset;
        }
    }
}
