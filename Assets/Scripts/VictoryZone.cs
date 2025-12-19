using UnityEngine;

/// <summary>
/// Zone de victoire - quand le joueur entre dans cette zone, il a gagné!
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class VictoryZone : MonoBehaviour
{
    [Header("Victory Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField] private bool showDuringPlay = true;
    [SerializeField] private bool showDebugZone = true;
    [SerializeField] private Color victoryColor = new Color(0f, 1f, 0f, 0.3f); // Vert transparent

    private bool victoryTriggered = false;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private GameObject visualObject;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        // Créer un objet enfant pour la visualisation
        visualObject = new GameObject("VictoryZone_Visual");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localPosition = boxCollider.offset;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = new Vector3(boxCollider.size.x, boxCollider.size.y, 1f);

        // Ajouter le SpriteRenderer à l'objet enfant
        spriteRenderer = visualObject.AddComponent<SpriteRenderer>();

        // Créer un sprite carré simple
        CreateSquareSprite();

        // Configurer le rendu
        spriteRenderer.color = victoryColor;
        spriteRenderer.sortingOrder = -1; // Derrière les personnages
    }

    void Update()
    {
        if (!showDuringPlay)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    void CreateSquareSprite()
    {
        // Créer une texture carrée simple
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        // Créer un sprite à partir de la texture
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        spriteRenderer.sprite = sprite;
    }

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
