using UnityEngine;

public class GridBasedDetectionZone : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 0.5f; // Taille d'un carreau en unités Unity (32px = 0.5 unit si PPU=64)

    [Header("Detection Pattern")]
    [Tooltip("Nombre de carreaux par rangée (ex: 1, 3, 5 pour un cône)")]
    [SerializeField] private int[] tilesPerRow = new int[] { 1, 3, 5 };

    [Header("Detection Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string playerTag = "Player";

    [Header("Line of Sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask wallLayer; // Layer des murs qui bloquent la vue
    [Tooltip("Si vide, détecte tout objet avec le tag 'Wall'")]
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private bool clipVisionToWalls = true; // Découpe la vision aux murs
    [SerializeField] private int raycastResolution = 10; // Nombre de raycasts pour détecter les murs

    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugZone = true;
    [SerializeField] private bool showLineOfSight = true;
    [SerializeField] private Color detectionColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.3f);

    private PolygonCollider2D polygonCollider;
    private bool playerDetected = false;
    private Vector2[] currentDirection = null; // Points pour la direction actuelle

    // Visualisation runtime
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    // Line of Sight tracking
    private GameObject lastDetectedPlayer;
    private bool hasLineOfSight = false;

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

        // Mettre à jour le mesh pour découper selon les murs
        if (clipVisionToWalls && Application.isPlaying)
        {
            Vector2[] clippedPoints = CalculateFieldOfView();
            UpdateVisualization(clippedPoints);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ne détecter QUE les triggers (ignore les colliders normaux du joueur)
        if (other.CompareTag(playerTag) && other.isTrigger)
        {
            // Remonter au parent si c'est un trigger enfant (DetectionTrigger)
            GameObject targetPlayer = other.transform.parent != null ?
                other.transform.parent.gameObject : other.gameObject;

            lastDetectedPlayer = targetPlayer;
            CheckDetection();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Ne détecter QUE les triggers (ignore les colliders normaux du joueur)
        if (other.CompareTag(playerTag) && other.isTrigger)
        {
            // Remonter au parent si c'est un trigger enfant (DetectionTrigger)
            GameObject targetPlayer = other.transform.parent != null ?
                other.transform.parent.gameObject : other.gameObject;

            lastDetectedPlayer = targetPlayer;
            CheckDetection();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Ne détecter QUE les triggers (ignore les colliders normaux du joueur)
        if (other.CompareTag(playerTag) && other.isTrigger)
        {
            // Remonter au parent si c'est un trigger enfant (DetectionTrigger)
            GameObject targetPlayer = other.transform.parent != null ?
                other.transform.parent.gameObject : other.gameObject;

            if (targetPlayer == lastDetectedPlayer)
            {
                playerDetected = false;
                hasLineOfSight = false;
                lastDetectedPlayer = null;
            }
        }
    }

    /// <summary>
    /// Vérifie si le joueur est détectable (avec ligne de vue si activée)
    /// </summary>
    void CheckDetection()
    {
        if (lastDetectedPlayer == null)
            return;

        // Vérifier la ligne de vue si activée
        if (requireLineOfSight)
        {
            hasLineOfSight = HasLineOfSight(lastDetectedPlayer);

            if (hasLineOfSight && !playerDetected)
            {
                // Joueur détecté pour la première fois avec ligne de vue
                playerDetected = true;
                OnPlayerDetected(lastDetectedPlayer);
            }
            else if (!hasLineOfSight)
            {
                // Pas de ligne de vue = pas de détection
                playerDetected = false;
            }
        }
        else
        {
            // Sans ligne de vue, détection immédiate
            if (!playerDetected)
            {
                playerDetected = true;
                OnPlayerDetected(lastDetectedPlayer);
            }
        }
    }

    /// <summary>
    /// Calcule le champ de vision en tenant compte des murs
    /// </summary>
    Vector2[] CalculateFieldOfView()
    {
        if (!clipVisionToWalls)
        {
            // Retourner le cône normal sans découpe
            return GenerateConePoints(currentDetectionDirection);
        }

        // Obtenir le trapèze original
        Vector2[] originalCone = GenerateConePoints(currentDetectionDirection);

        // Pour chaque point du trapèze, vérifier s'il y a un mur
        Vector2[] clippedCone = new Vector2[originalCone.Length];
        Vector2 worldPos = transform.position;

        for (int i = 0; i < originalCone.Length; i++)
        {
            Vector2 localPoint = originalCone[i];
            Vector2 worldPoint = worldPos + localPoint;

            // Direction du policier vers ce point
            Vector2 direction = localPoint.normalized;
            float distance = localPoint.magnitude;

            // Raycast pour voir s'il y a un mur
            RaycastHit2D hit = RaycastForWall(worldPos, direction, distance);

            if (hit.collider != null)
            {
                // Mur trouvé, raccourcir le point jusqu'au mur
                clippedCone[i] = hit.point - worldPos;
            }
            else
            {
                // Pas de mur, garder le point original
                clippedCone[i] = localPoint;
            }
        }

        return clippedCone;
    }

    /// <summary>
    /// Fait un raycast qui détecte uniquement les murs
    /// </summary>
    RaycastHit2D RaycastForWall(Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance);

        foreach (RaycastHit2D hit in hits)
        {
            // Ignorer le trigger de détection lui-même
            if (hit.collider.gameObject == gameObject)
                continue;

            // Ignorer les joueurs
            if (hit.collider.CompareTag(playerTag))
                continue;

            // Vérifier si c'est un mur (par layer ou tag)
            if (wallLayer.value != 0)
            {
                int hitLayer = 1 << hit.collider.gameObject.layer;
                if ((wallLayer.value & hitLayer) != 0)
                {
                    return hit; // Mur trouvé
                }
            }

            if (!string.IsNullOrEmpty(wallTag) && hit.collider.CompareTag(wallTag))
            {
                return hit; // Mur trouvé
            }
        }

        return default(RaycastHit2D); // Pas de mur
    }

    /// <summary>
    /// Vérifie s'il y a une ligne de vue dégagée jusqu'au joueur (pas de mur entre)
    /// </summary>
    bool HasLineOfSight(GameObject player)
    {
        if (player == null)
            return false;

        // Position du policier (parent de la zone de détection)
        Vector2 policePos = transform.position;
        Vector2 playerPos = player.transform.position;

        // Direction et distance
        Vector2 direction = (playerPos - policePos).normalized;
        float distance = Vector2.Distance(policePos, playerPos);

        // Faire un raycast qui détecte tout
        RaycastHit2D[] hits = Physics2D.RaycastAll(policePos, direction, distance);

        // Vérifier chaque objet touché
        foreach (RaycastHit2D hit in hits)
        {
            // Ignorer le trigger de détection lui-même et le joueur
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject == player)
                continue;

            // Si on a configuré un layer spécifique, vérifier d'abord ça
            if (wallLayer.value != 0)
            {
                int hitLayer = 1 << hit.collider.gameObject.layer;
                if ((wallLayer.value & hitLayer) != 0)
                {
                    Debug.Log($"🚫 Mur détecté par layer: {hit.collider.name}");
                    return false; // Un mur bloque la vue
                }
            }

            // Vérifier le tag "Wall"
            if (!string.IsNullOrEmpty(wallTag) && hit.collider.CompareTag(wallTag))
            {
                Debug.Log($"🚫 Mur détecté par tag: {hit.collider.name}");
                return false; // Un mur bloque la vue
            }
        }

        // Pas d'obstacle, ligne de vue dégagée
        Debug.Log($"✅ Ligne de vue dégagée vers {player.name}");
        return true;
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

        // Dessiner la ligne de vue en mode jeu
        if (showLineOfSight && Application.isPlaying && lastDetectedPlayer != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Vector3 policePos = transform.position;
            Vector3 playerPos = lastDetectedPlayer.transform.position;

            // Couleur selon si la ligne de vue est bloquée ou non
            Gizmos.color = hasLineOfSight ? Color.red : Color.green;
            Gizmos.DrawLine(policePos, playerPos);

            // Dessiner une sphère au point d'impact
            if (!hasLineOfSight)
            {
                // Trouver où le raycast a été bloqué
                Vector2 direction = (playerPos - policePos).normalized;
                float distance = Vector2.Distance(policePos, playerPos);
                RaycastHit2D hit = Physics2D.Raycast(policePos, direction, distance, wallLayer);

                if (hit.collider != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(hit.point, 0.2f);
                }
            }
        }
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
