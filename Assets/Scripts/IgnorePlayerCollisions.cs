using UnityEngine;

/// <summary>
/// Script qui empêche les joueurs de se collisionner entre eux
/// Attache ce script aux deux joueurs
/// </summary>
public class IgnorePlayerCollisions : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool freezeRotation = true; // Empêcher la rotation
    [SerializeField] private string[] playerTags = new string[] { "Player" }; // Tags des joueurs

    private Rigidbody2D rb;
    private Collider2D playerCollider;

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Figer la rotation si un Rigidbody2D existe
        if (rb != null && freezeRotation)
        {
            rb.freezeRotation = true;
            Debug.Log($"{gameObject.name}: Rotation figée");
        }

        // Ignorer les collisions avec les autres joueurs
        IgnoreCollisionsWithOtherPlayers();
    }

    void IgnoreCollisionsWithOtherPlayers()
    {
        if (playerCollider == null)
        {
            Debug.LogWarning($"{gameObject.name}: Aucun Collider2D trouvé!");
            return;
        }

        // Trouver tous les GameObjects avec les tags de joueurs
        foreach (string playerTag in playerTags)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

            foreach (GameObject otherPlayer in players)
            {
                // Ne pas ignorer ses propres collisions
                if (otherPlayer == gameObject)
                    continue;

                Collider2D otherCollider = otherPlayer.GetComponent<Collider2D>();
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, otherCollider, true);
                    Debug.Log($"Collisions ignorées entre {gameObject.name} et {otherPlayer.name}");
                }
            }
        }
    }

    // Méthode pour réactiver les collisions si nécessaire
    [ContextMenu("Enable Player Collisions")]
    public void EnablePlayerCollisions()
    {
        if (playerCollider == null)
            return;

        foreach (string playerTag in playerTags)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

            foreach (GameObject otherPlayer in players)
            {
                if (otherPlayer == gameObject)
                    continue;

                Collider2D otherCollider = otherPlayer.GetComponent<Collider2D>();
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, otherCollider, false);
                }
            }
        }
    }
}
