using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DetectionZoneVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private bool showDuringPlay = true;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.3f); // Jaune transparent
    [SerializeField] private Color detectionColor = new Color(1f, 0f, 0f, 0.5f); // Rouge transparent

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool playerDetected = false;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        // Créer ou récupérer le SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Créer un sprite carré simple
        CreateSquareSprite();

        // Configurer le rendu
        spriteRenderer.color = normalColor;
        spriteRenderer.sortingOrder = -1; // Derrière les personnages
    }

    void Update()
    {
        if (!showDuringPlay)
        {
            spriteRenderer.enabled = false;
            return;
        }

        // Mettre à jour la couleur selon la détection
        spriteRenderer.color = playerDetected ? detectionColor : normalColor;

        // Synchroniser la taille et position avec le BoxCollider2D
        transform.localScale = new Vector3(boxCollider.size.x, boxCollider.size.y, 1f);
        transform.localPosition = new Vector3(boxCollider.offset.x, boxCollider.offset.y, 0f);
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
        if (other.CompareTag("Player"))
        {
            playerDetected = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = false;
        }
    }
}
