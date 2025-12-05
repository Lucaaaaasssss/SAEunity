using UnityEngine;

public class GridBasedDetectionZone : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f; // Taille d'un carreau en unités Unity (32px = 1 unit si PPU=32)

    [Header("Detection Pattern")]
    [Tooltip("Nombre de carreaux par rangée (ex: 1, 3, 5 pour un cône)")]
    [SerializeField] private int[] tilesPerRow = new int[] { 1, 3, 5 };

    [Header("Detection Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugZone = true;
    [SerializeField] private Color detectionColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.3f);

    private PolygonCollider2D polygonCollider;
    private bool playerDetected = false;
    private Vector2[] currentDirection = null; // Points pour la direction actuelle

    // Visualisation runtime
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    // Directions possibles (Up, Down, Left, Right)
    public enum DetectionDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private DetectionDirection currentDetectionDirection = DetectionDirection.Right;

    void Start()
    {
        // Créer ou récupérer le PolygonCollider2D
        polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

        polygonCollider.isTrigger = true;

        // Créer le système de visualisation runtime
        SetupRuntimeVisualization();

        // Générer la forme initiale
        UpdateDetectionShape(currentDetectionDirection);
    }

    void SetupRuntimeVisualization()
    {
        // Créer un MeshFilter et MeshRenderer pour afficher la zone remplie
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Créer un matériau transparent
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        Material mat = new Material(shader);
        mat.color = normalColor;
        meshRenderer.material = mat;
        meshRenderer.sortingOrder = -1; // Derrière les personnages
    }

    /// <summary>
    /// Met à jour la forme de détection selon la direction
    /// </summary>
    public void UpdateDetectionShape(DetectionDirection direction)
    {
        currentDetectionDirection = direction;
        Vector2[] points = GenerateConePoints(direction);

        if (polygonCollider != null)
        {
            polygonCollider.points = points;
            currentDirection = points;
        }

        // Mettre à jour la visualisation
        UpdateVisualization(points);
    }

    void UpdateVisualization(Vector2[] points)
    {
        if (!showDebugZone)
        {
            if (meshRenderer != null) meshRenderer.enabled = false;
            return;
        }

        // Mettre à jour le mesh
        if (meshFilter != null && meshRenderer != null)
        {
            Mesh mesh = CreateMeshFromPoints(points);
            meshFilter.mesh = mesh;
            meshRenderer.enabled = true;
        }
    }

    Mesh CreateMeshFromPoints(Vector2[] points)
    {
        Mesh mesh = new Mesh();

        // Convertir les points 2D en vertices 3D
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0);
        }

        // Créer les triangles (pour un quad/trapèze, on a 2 triangles)
        int[] triangles = new int[(points.Length - 2) * 3];
        for (int i = 0; i < points.Length - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// Génère les points du cône selon la direction
    /// Pattern: 1 carreau, puis 3, puis 5
    /// </summary>
    private Vector2[] GenerateConePoints(DetectionDirection direction)
    {
        // Créer un trapèze qui englobe le pattern 1-3-5
        // Le cône fait 3 rangées de profondeur
        float depth = tilesPerRow.Length * tileSize; // 3 carreaux de profondeur
        float baseWidth = tileSize; // Largeur à la base (1 carreau)
        float topWidth = tilesPerRow[tilesPerRow.Length - 1] * tileSize; // Largeur au sommet (5 carreaux)

        Vector2[] points = new Vector2[4];

        switch (direction)
        {
            case DetectionDirection.Up:
                // Cône vers le haut
                points[0] = new Vector2(-baseWidth / 2, 0); // Bas gauche
                points[1] = new Vector2(baseWidth / 2, 0);  // Bas droit
                points[2] = new Vector2(topWidth / 2, depth); // Haut droit
                points[3] = new Vector2(-topWidth / 2, depth); // Haut gauche
                break;

            case DetectionDirection.Down:
                // Cône vers le bas
                points[0] = new Vector2(-baseWidth / 2, 0); // Haut gauche
                points[1] = new Vector2(baseWidth / 2, 0);  // Haut droit
                points[2] = new Vector2(topWidth / 2, -depth); // Bas droit
                points[3] = new Vector2(-topWidth / 2, -depth); // Bas gauche
                break;

            case DetectionDirection.Right:
                // Cône vers la droite
                points[0] = new Vector2(0, -baseWidth / 2); // Bas gauche
                points[1] = new Vector2(0, baseWidth / 2);  // Haut gauche
                points[2] = new Vector2(depth, topWidth / 2); // Haut droit
                points[3] = new Vector2(depth, -topWidth / 2); // Bas droit
                break;

            case DetectionDirection.Left:
                // Cône vers la gauche
                points[0] = new Vector2(0, -baseWidth / 2); // Bas droit
                points[1] = new Vector2(0, baseWidth / 2);  // Haut droit
                points[2] = new Vector2(-depth, topWidth / 2); // Haut gauche
                points[3] = new Vector2(-depth, -topWidth / 2); // Bas gauche
                break;
        }

        return points;
    }

    void Update()
    {
        // Mettre à jour la couleur selon la détection
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = playerDetected ? detectionColor : normalColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerDetected = true;
            OnPlayerDetected(other.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerDetected = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerDetected = false;
        }
    }

    void OnPlayerDetected(GameObject player)
    {
        Debug.Log($"Player detected: {player.name}");

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

        Gizmos.color = playerDetected ? detectionColor : normalColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Dessiner le polygone
        if (currentDirection != null && currentDirection.Length > 0)
        {
            for (int i = 0; i < currentDirection.Length; i++)
            {
                Vector2 current = currentDirection[i];
                Vector2 next = currentDirection[(i + 1) % currentDirection.Length];
                Gizmos.DrawLine(current, next);
            }
        }
        else if (Application.isPlaying && polygonCollider != null)
        {
            Vector2[] points = polygonCollider.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % points.Length];
                Gizmos.DrawLine(current, next);
            }
        }
        else
        {
            // Afficher la forme par défaut dans l'éditeur
            Vector2[] previewPoints = GenerateConePoints(currentDetectionDirection);
            for (int i = 0; i < previewPoints.Length; i++)
            {
                Vector2 current = previewPoints[i];
                Vector2 next = previewPoints[(i + 1) % previewPoints.Length];
                Gizmos.DrawLine(current, next);
            }
        }

        // Dessiner aussi la grille des carreaux pour visualiser
        DrawTileGrid();
    }

    void DrawTileGrid()
    {
        if (!showDebugZone)
            return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);

        // Dessiner les carreaux selon le pattern (1-3-5)
        switch (currentDetectionDirection)
        {
            case DetectionDirection.Up:
            case DetectionDirection.Down:
                float dirY = currentDetectionDirection == DetectionDirection.Up ? 1 : -1;
                for (int row = 0; row < tilesPerRow.Length; row++)
                {
                    int tiles = tilesPerRow[row];
                    float yPos = (row + 0.5f) * tileSize * dirY;

                    for (int i = 0; i < tiles; i++)
                    {
                        float xOffset = (i - (tiles - 1) / 2f) * tileSize;
                        DrawTile(new Vector2(xOffset, yPos), tileSize);
                    }
                }
                break;

            case DetectionDirection.Right:
            case DetectionDirection.Left:
                float dirX = currentDetectionDirection == DetectionDirection.Right ? 1 : -1;
                for (int row = 0; row < tilesPerRow.Length; row++)
                {
                    int tiles = tilesPerRow[row];
                    float xPos = (row + 0.5f) * tileSize * dirX;

                    for (int i = 0; i < tiles; i++)
                    {
                        float yOffset = (i - (tiles - 1) / 2f) * tileSize;
                        DrawTile(new Vector2(xPos, yOffset), tileSize);
                    }
                }
                break;
        }
    }

    void DrawTile(Vector2 center, float size)
    {
        float halfSize = size / 2f;
        Vector2 topLeft = center + new Vector2(-halfSize, halfSize);
        Vector2 topRight = center + new Vector2(halfSize, halfSize);
        Vector2 bottomLeft = center + new Vector2(-halfSize, -halfSize);
        Vector2 bottomRight = center + new Vector2(halfSize, -halfSize);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
