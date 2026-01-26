using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Plaque de pression qui détecte les joueurs et ouvre des portes
/// </summary>
public class PressurePlate : MonoBehaviour
{
    public enum PlateAction
    {
        OpenDoors,          // Ouvre des portes
        DisableCameras      // Désactive des caméras
    }

    [Header("Plaque Sprites")]
    [SerializeField] private Sprite plateUp;   // plaque_lev (levée)
    [SerializeField] private Sprite plateDown; // plaque_ap (appuyée)

    [Header("Plate Action Type")]
    [SerializeField] private PlateAction actionType = PlateAction.OpenDoors;

    [Header("Connected Objects")]
    [SerializeField] private Door[] connectedDoors; // Portes à ouvrir/fermer
    [SerializeField] private CameraDetectionZone[] connectedCameras; // Caméras à désactiver

    [Header("Detection Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D plateCollider;

    [Header("Sound")]
    [SerializeField] private AudioClip plateSound; // Son à jouer quand on monte sur la dalle
    private AudioSource audioSource;

    private int playersOnPlate = 0; // Nombre de joueurs sur la plaque
    private bool isPressed = false;
    private GameObject currentPlayer = null; // Le joueur actuellement sur la plaque

    /// <summary>
    /// Retourne le joueur actuellement sur la plaque (null si aucun)
    /// </summary>
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

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

        // Initialiser l'AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Appliquer l'état initial
        UpdatePlateState();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Détecter uniquement le BoxCollider2D principal (pas le trigger enfant PlayerDetectionCollider)
        if (other.CompareTag(playerTag) && !other.isTrigger)
        {
            playersOnPlate++;

            // Stocker le premier joueur qui monte sur la plaque
            if (currentPlayer == null)
            {
                currentPlayer = other.gameObject;
            }

            if (!isPressed && playersOnPlate > 0)
            {
                Press();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Détecter uniquement le BoxCollider2D principal (pas le trigger enfant PlayerDetectionCollider)
        if (other.CompareTag(playerTag) && !other.isTrigger)
        {
            playersOnPlate--;

            // Réinitialiser le joueur actuel si c'est lui qui part
            if (currentPlayer == other.gameObject)
            {
                currentPlayer = null;
            }

            if (isPressed && playersOnPlate <= 0)
            {
                Release();
            }
        }
    }

    /// <summary>
    /// Appuyer sur la plaque
    /// </summary>
    void Press()
    {
        isPressed = true;
        UpdatePlateState();

        // Jouer le son de la dalle
        if (audioSource != null && plateSound != null)
        {
            audioSource.PlayOneShot(plateSound);
        }

        // Exécuter l'action selon le type
        switch (actionType)
        {
            case PlateAction.OpenDoors:
                OpenDoors();
                Debug.Log($"⬇️ Plaque {gameObject.name} appuyée - Portes ouvertes");
                break;

            case PlateAction.DisableCameras:
                DisableCameras();
                Debug.Log($"⬇️ Plaque {gameObject.name} appuyée - Caméras désactivées");
                break;
        }
    }

    /// <summary>
    /// Relâcher la plaque
    /// </summary>
    void Release()
    {
        isPressed = false;
        UpdatePlateState();

        // Inverser l'action selon le type
        switch (actionType)
        {
            case PlateAction.OpenDoors:
                CloseDoors();
                Debug.Log($"⬆️ Plaque {gameObject.name} relâchée - Portes fermées");
                break;

            case PlateAction.DisableCameras:
                EnableCameras();
                Debug.Log($"⬆️ Plaque {gameObject.name} relâchée - Caméras réactivées");
                break;
        }
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

    /// <summary>
    /// Désactive toutes les caméras connectées
    /// </summary>
    void DisableCameras()
    {
        foreach (CameraDetectionZone camera in connectedCameras)
        {
            if (camera != null)
            {
                camera.enabled = false; // Désactive le script de détection
                Debug.Log($"📷 Caméra {camera.gameObject.name} désactivée");
            }
        }
    }

    /// <summary>
    /// Réactive toutes les caméras connectées
    /// </summary>
    void EnableCameras()
    {
        foreach (CameraDetectionZone camera in connectedCameras)
        {
            if (camera != null)
            {
                camera.enabled = true; // Réactive le script de détection
                Debug.Log($"📷 Caméra {camera.gameObject.name} réactivée");
            }
        }
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        // Dessiner des lignes selon le type d'action
        switch (actionType)
        {
            case PlateAction.OpenDoors:
                if (connectedDoors != null && connectedDoors.Length > 0)
                {
                    Gizmos.color = isPressed ? Color.green : Color.yellow;
                    foreach (Door door in connectedDoors)
                    {
                        if (door != null)
                        {
                            Gizmos.DrawLine(transform.position, door.transform.position);
                        }
                    }
                }
                break;

            case PlateAction.DisableCameras:
                if (connectedCameras != null && connectedCameras.Length > 0)
                {
                    Gizmos.color = isPressed ? Color.green : Color.cyan;
                    foreach (CameraDetectionZone camera in connectedCameras)
                    {
                        if (camera != null)
                        {
                            Gizmos.DrawLine(transform.position, camera.transform.position);
                        }
                    }
                }
                break;
        }
    }
}
