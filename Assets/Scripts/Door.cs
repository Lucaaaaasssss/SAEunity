using UnityEngine;

/// <summary>
/// Porte qui peut s'ouvrir et se fermer
/// Change de sprite et active/désactive le collider
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Door Sprites")]
    [SerializeField] private Sprite closedSprite; // Porte fermée
    [SerializeField] private Sprite openSprite;   // Porte ouverte

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D doorCollider;

    [Header("State")]
    [SerializeField] private bool isOpen = false;

    void Start()
    {
        // Auto-find components si non assignés
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (doorCollider == null)
            doorCollider = GetComponent<BoxCollider2D>();

        // Appliquer l'état initial
        UpdateDoorState();
    }

    /// <summary>
    /// Ouvre la porte
    /// </summary>
    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        UpdateDoorState();
        Debug.Log($"🚪 {gameObject.name} ouverte");
    }

    /// <summary>
    /// Ferme la porte
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        UpdateDoorState();
        Debug.Log($"🚪 {gameObject.name} fermée");
    }

    /// <summary>
    /// Met à jour le sprite et le collider selon l'état
    /// </summary>
    void UpdateDoorState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = !isOpen; // Collider actif quand fermée
        }
    }

    // Permet de changer l'état dans l'éditeur
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateDoorState();
        }
    }
}
