using UnityEngine;

public class TiledGround : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private float heightMultiplier = 5f;
    [SerializeField] private float tileSize = 1f;

    [Header("Tile Sprites")]
    [SerializeField] private Sprite[] groundTiles = new Sprite[4];

    private Camera mainCamera;
    private GameObject tilesContainer;

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

        if (tilesContainer != null)
        {
            Destroy(tilesContainer);
        }

        tilesContainer = new GameObject("Tiles");
        tilesContainer.transform.parent = transform;
        tilesContainer.transform.localPosition = Vector3.zero;

        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        groundHeight = cameraHeight * heightMultiplier;
        groundWidth = cameraWidth;

        transform.position = new Vector3(
            mainCamera.transform.position.x,
            mainCamera.transform.position.y,
            0
        );

        // ===== CALCUL ORIGINAL =====
        int originalTilesX = Mathf.CeilToInt(groundWidth / tileSize) + 1;
        int tilesY = Mathf.CeilToInt(groundHeight / tileSize) + 1;

        float originalStartX = -(originalTilesX * tileSize) / 2f;
        float rightEdge = originalStartX + (originalTilesX - 1) * tileSize;

        // ===== NOUVEAU CALCUL (1 colonne en moins à GAUCHE) =====
        int tilesX = originalTilesX - 1;
        float startX = rightEdge - (tilesX - 1) * tileSize;
        float startY = -(tilesY * tileSize) / 2f;

        // ===== GÉNÉRATION =====
        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.parent = tilesContainer.transform;

                float posX = startX + x * tileSize;
                float posY = startY + y * tileSize;
                tile.transform.localPosition = new Vector3(posX, posY, 0);

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                int patternIndex = ((x % 2) + (y % 2) * 2) % 4;
                sr.sprite = groundTiles[patternIndex];

                if (sr.sprite != null)
                {
                    Vector2 size = sr.sprite.bounds.size;
                    tile.transform.localScale = new Vector3(
                        tileSize / size.x,
                        tileSize / size.y,
                        1
                    );
                }

                sr.sortingOrder = -100;
            }
        }

        Debug.Log($"Created {tilesX}x{tilesY} ground tiles (1 column removed on LEFT)");
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
