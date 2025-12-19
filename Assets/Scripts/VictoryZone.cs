using UnityEngine;

/// <summary>
/// Zone de victoire - quand le joueur entre dans cette zone, il a gagné!
/// </summary>
public class VictoryZone : MonoBehaviour
{
    [Header("Victory Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugZone = true;
    [SerializeField] private Color victoryColor = new Color(0f, 1f, 0f, 0.5f); // Vert

    private bool victoryTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifier si c'est le joueur et que la victoire n'a pas déjà été déclenchée
        if (!victoryTriggered && other.CompareTag(playerTag))
        {
            victoryTriggered = true;
            OnPlayerVictory(other.gameObject);
        }
    }

    void OnPlayerVictory(GameObject player)
    {
        Debug.Log($"🎉 VICTOIRE! Joueur: {player.name}");

        // Déclencher la victoire via le GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.TriggerVictory();
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot trigger victory.");
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
            Gizmos.color = victoryColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
        }
    }
}
