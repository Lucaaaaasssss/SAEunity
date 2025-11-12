using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ArcadeGround : MonoBehaviour
{
    [Header("Resolution Settings")]
    [SerializeField] private float targetWidth = 1920f;
    [SerializeField] private float targetHeight = 1080f;

    [Header("Ground Settings")]
    [SerializeField] private Color groundColor = new Color(0.2f, 0.3f, 0.2f, 1f);
    [SerializeField] private float heightMultiplier = 5f; // Multiplier pour la hauteur du sol

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        SetupGround();
    }

    void SetupGround()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        // Créer un sprite blanc simple pour le sol
        CreateGroundSprite();

        // Ajuster la taille pour couvrir tout l'écran
        AdjustGroundSize();

        // Définir la couleur
        spriteRenderer.color = groundColor;
    }

    void CreateGroundSprite()
    {
        // Créer une texture 1x1 blanche
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        // Créer un sprite à partir de la texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        spriteRenderer.sprite = sprite;
    }

    void AdjustGroundSize()
    {
        // Calculer la hauteur visible de la caméra en unités Unity
        float cameraHeight = mainCamera.orthographicSize * 2f;

        // Calculer la largeur visible basée sur l'aspect ratio
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Appliquer le multiplicateur de hauteur pour un sol plus long
        float groundHeight = cameraHeight * heightMultiplier;

        // Appliquer la taille au transform
        transform.localScale = new Vector3(cameraWidth, groundHeight, 1);

        // Positionner le sol au centre de la caméra (le pivot est au milieu)
        // Le sol s'étendra vers le haut et vers le bas
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0);

        Debug.Log($"Ground size set to: {cameraWidth} x {groundHeight} units (height x{heightMultiplier})");
    }

    // Méthode pour ajuster dynamiquement si nécessaire
    void Update()
    {
        // Optionnel: mettre à jour la taille si la caméra change
        // Décommentez si vous voulez un ajustement dynamique
        // AdjustGroundSize();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            SetupGround();
        }
    }
#endif
}
