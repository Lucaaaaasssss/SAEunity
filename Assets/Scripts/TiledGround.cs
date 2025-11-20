using UnityEngine;

public class TiledGround : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private float heightMultiplier = 5f;
    [SerializeField] private float tileSize = 1f; // Taille de chaque tuile en unités Unity

    [Header("Tile Sprites")]
    [SerializeField] private Sprite[] groundTiles = new Sprite[4]; // Les 4 sprites de sol

    private Camera mainCamera;
    private GameObject tilesContainer;

    // Dimensions de la zone de sol pour les limites
    private float groundWidth;
    private float groundHeight;

    public Vector2 GetGroundDimensions()
    {
        return new Vector2(groundWidth, groundHeight);
    }

    void Start()
    {
        mainCamera = Camera.main;
        SetupTiledGround();
    }

    void SetupTiledGround()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        if (groundTiles == null || groundTiles.Length < 4)
        {
            Debug.LogError("Please assign 4 ground tile sprites in the inspector!");
            return;
        }

        // Nettoyer les anciennes tuiles si elles existent
        if (tilesContainer != null)
        {
            Destroy(tilesContainer);
        }

        // Créer un conteneur pour toutes les tuiles
        tilesContainer = new GameObject("Tiles");
        tilesContainer.transform.parent = transform;
        tilesContainer.transform.localPosition = Vector3.zero;

        // Calculer les dimensions de la zone à couvrir
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        groundHeight = cameraHeight * heightMultiplier;
        groundWidth = cameraWidth;

        // Positionner le Ground au centre de la caméra
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0);

        // Calculer le nombre de tuiles nécessaires
        int tilesX = Mathf.CeilToInt(groundWidth / tileSize) + 1;
        int tilesY = Mathf.CeilToInt(groundHeight / tileSize) + 1;

        // Calculer la position de départ (coin inférieur gauche)
        float startX = -(tilesX * tileSize) / 2f;
        float startY = -(tilesY * tileSize) / 2f;

        // Créer les tuiles en grille
        int tileIndex = 0;
        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                // Créer une nouvelle tuile
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.parent = tilesContainer.transform;

                // Positionner la tuile
                float posX = startX + (x * tileSize);
                float posY = startY + (y * tileSize);
                tile.transform.localPosition = new Vector3(posX, posY, 0);

                // Ajouter et configurer le SpriteRenderer
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                // Utiliser les sprites en alternance pour créer un motif (pattern 2x2)
                int patternIndex = ((x % 2) + (y % 2) * 2) % 4;
                sr.sprite = groundTiles[patternIndex];

                // Ajuster la taille de la tuile
                if (sr.sprite != null)
                {
                    float spriteWidth = sr.sprite.bounds.size.x;
                    float spriteHeight = sr.sprite.bounds.size.y;
                    tile.transform.localScale = new Vector3(
                        tileSize / spriteWidth,
                        tileSize / spriteHeight,
                        1
                    );
                }

                // Définir le sorting order pour que le sol soit derrière tout
                sr.sortingOrder = -100;

                tileIndex++;
            }
        }

        Debug.Log($"Created {tilesX}x{tilesY} = {tilesX * tilesY} ground tiles");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            SetupTiledGround();
        }
    }
#endif
}
