using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Plaque de pression qui détecte les joueurs et ouvre des portes
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Header("Plaque Sprites")]
    [SerializeField] private Sprite plateUp;   // plaque_lev (levée)
    [SerializeField] private Sprite plateDown; // plaque_ap (appuyée)

    [Header("Connected Doors")]
    [SerializeField] private Door[] connectedDoors; // Portes à ouvrir/fermer

    [Header("Detection Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D plateCollider;

    private int playersOnPlate = 0; // Nombre de joueurs sur la plaque
    private bool isPressed = false;

    void Start()
    {
        // Auto-find components si non assignés
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (plateCollider == null)
        {
            plateCollider = GetComponent<BoxCollider2D>();
            if (plateCollider != null)
            {
                plateCollider.isTrigger = true; // La plaque doit être un trigger
            }
        }

        // Appliquer l'état initial
        UpdatePlateState();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Détecter les joueurs (peut être le trigger enfant du PlayerDetectionCollider)
        if (other.CompareTag(playerTag))
        {
            playersOnPlate++;

            if (!isPressed && playersOnPlate > 0)
            {
                Press();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playersOnPlate--;

            if (isPressed && playersOnPlate <= 0)
            {
                Release();
            }
        }
    }

    /// <summary>
    /// Appuyer sur la plaque (ouvrir les portes)
    /// </summary>
    void Press()
    {
        isPressed = true;
        UpdatePlateState();
        OpenDoors();
        Debug.Log($"⬇️ Plaque {gameObject.name} appuyée");
    }

    /// <summary>
    /// Relâcher la plaque (fermer les portes)
    /// </summary>
    void Release()
    {
        isPressed = false;
        UpdatePlateState();
        CloseDoors();
        Debug.Log($"⬆️ Plaque {gameObject.name} relâchée");
    }

    /// <summary>
    /// Met à jour le sprite de la plaque
    /// </summary>
    void UpdatePlateState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isPressed ? plateDown : plateUp;
        }
    }

    /// <summary>
    /// Ouvre toutes les portes connectées
    /// </summary>
    void OpenDoors()
    {
        foreach (Door door in connectedDoors)
        {
            if (door != null)
            {
                door.Open();
            }
        }
    }

    /// <summary>
    /// Ferme toutes les portes connectées
    /// </summary>
    void CloseDoors()
    {
        foreach (Door door in connectedDoors)
        {
            if (door != null)
            {
                door.Close();
            }
        }
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        if (connectedDoors == null || connectedDoors.Length == 0)
            return;

        // Dessiner des lignes vers les portes connectées
        Gizmos.color = isPressed ? Color.green : Color.yellow;
        foreach (Door door in connectedDoors)
        {
            if (door != null)
            {
                Gizmos.DrawLine(transform.position, door.transform.position);
            }
        }
    }
}
