using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask playerLayer; // Layer des joueurs (cobayes)
    [SerializeField] private string playerTag = "Player"; // Tag des cobayes

    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugZone = true;
    [SerializeField] private Color detectionColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.3f);

    private bool playerDetected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifier si c'est un des cobayes
        if (other.CompareTag(playerTag))
        {
            playerDetected = true;
            OnPlayerDetected(other.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Continue de vérifier pendant que le joueur est dans la zone
        if (other.CompareTag(playerTag))
        {
            playerDetected = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Le joueur a quitté la zone
        if (other.CompareTag(playerTag))
        {
            playerDetected = false;
        }
    }

    void OnPlayerDetected(GameObject player)
    {
        Debug.Log($"Player detected: {player.name}");

        // Déclencher le game over
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.TriggerGameOver($"Repéré par un policier!");
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot trigger game over.");
        }
    }

    // Visualisation dans l'éditeur Unity
    void OnDrawGizmos()
    {
        if (!showDebugZone)
            return;

        // Utiliser BoxCollider2D pour afficher la zone
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = playerDetected ? detectionColor : normalColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
        }
    }
}
