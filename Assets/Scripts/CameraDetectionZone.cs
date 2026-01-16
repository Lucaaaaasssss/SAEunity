using UnityEngine;

/// <summary>
/// Zone de détection carrée 3x3 pour les caméras de surveillance
/// Similaire à GridBasedDetectionZone mais forme carrée simple
/// </summary>
public class CameraDetectionZone : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float tileSize = 0.5f; // Taille d'un carreau (32px = 0.5 unit si PPU=64)
    [SerializeField] private float detectionRadius = 0.75f; // Rayon du cercle (1.5 tiles de diamètre = 3x3)
    [SerializeField] private float offsetDistance = 0.5f; // Distance de la caméra (1 tile d'écart)
    [SerializeField] private string playerTag = "Player";

    [Header("Line of Sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private string wallTag = "Wall";

    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugZone = true;
    [SerializeField] private Color detectionColor = new Color(1f, 0f, 0f, 0.3f); // Rouge quand détecté
    [SerializeField] private Color normalColor = new Color(0f, 0.5f, 1f, 0.3f); // Bleu clair normalement

    private CircleCollider2D circleCollider;
    private bool playerDetected = false;
    private GameObject lastDetectedPlayer;
    private bool hasLineOfSight = false;

    // Visualisation
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    void Start()
    {
        SetupCollider();
        SetupVisualization();
    }

    void SetupCollider()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        circleCollider.isTrigger = true;

        // Rayon du cercle
        circleCollider.radius = detectionRadius;

        // Pas d'offset - le cercle est centré sur le GameObject
        // Vous déplacez le GameObject DetectionZone pour positionner la zone
        circleCollider.offset = Vector2.zero;

        Debug.Log($"✅ Zone de détection caméra créée : rayon={detectionRadius}");
    }

    void SetupVisualization()
    {
        // Créer un MeshFilter et MeshRenderer pour afficher la zone
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

        // Matériau transparent
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        Material mat = new Material(shader);
        mat.color = normalColor;
        meshRenderer.material = mat;
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = -10;

        UpdateVisualization();
    }

    void UpdateVisualization()
    {
        // Créer un cercle avec des triangles
        int segments = 32; // Nombre de segments pour le cercle
        Vector3 center = Vector3.zero; // Centré sur le GameObject

        Vector3[] vertices = new Vector3[segments + 1];
        vertices[0] = center; // Centre du cercle

        // Créer les points du cercle
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 360f / segments * Mathf.Deg2Rad;
            vertices[i + 1] = center + new Vector3(
                Mathf.Cos(angle) * detectionRadius,
                Mathf.Sin(angle) * detectionRadius,
                0
            );
        }

        // Créer les triangles
        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // Centre
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void Update()
    {
        // Mettre à jour la couleur selon la détection
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = playerDetected ? detectionColor : normalColor;
        }
    }

    /// <summary>
    /// Active ou désactive la caméra (appelé par PressurePlate)
    /// </summary>
    void OnEnable()
    {
        // Activer le mesh et le collider quand le script est activé
        if (meshRenderer != null) meshRenderer.enabled = true;
        if (circleCollider != null) circleCollider.enabled = true;
    }

    void OnDisable()
    {
        // Cacher le mesh et désactiver le collider quand le script est désactivé
        if (meshRenderer != null) meshRenderer.enabled = false;
        if (circleCollider != null) circleCollider.enabled = false;
        playerDetected = false;
        lastDetectedPlayer = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && other.isTrigger)
        {
            GameObject targetPlayer = other.transform.parent != null ?
                other.transform.parent.gameObject : other.gameObject;

            lastDetectedPlayer = targetPlayer;
            CheckDetection();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && other.isTrigger)
        {
            GameObject targetPlayer = other.transform.parent != null ?
                other.transform.parent.gameObject : other.gameObject;

            lastDetectedPlayer = targetPlayer;
            CheckDetection();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && other.isTrigger)
        {
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

    void CheckDetection()
    {
        if (lastDetectedPlayer == null)
            return;

        if (requireLineOfSight)
        {
            hasLineOfSight = HasLineOfSight(lastDetectedPlayer);

            if (hasLineOfSight && !playerDetected)
            {
                playerDetected = true;
                OnPlayerDetected(lastDetectedPlayer);
            }
            else if (!hasLineOfSight)
            {
                playerDetected = false;
            }
        }
        else
        {
            if (!playerDetected)
            {
                playerDetected = true;
                OnPlayerDetected(lastDetectedPlayer);
            }
        }
    }

    bool HasLineOfSight(GameObject player)
    {
        if (player == null)
            return false;

        Vector2 cameraPos = transform.position;
        Vector2 playerPos = player.transform.position;

        Vector2 direction = (playerPos - cameraPos).normalized;
        float distance = Vector2.Distance(cameraPos, playerPos);

        RaycastHit2D[] hits = Physics2D.RaycastAll(cameraPos, direction, distance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject == player)
                continue;

            if (!string.IsNullOrEmpty(wallTag) && hit.collider.CompareTag(wallTag))
            {
                return false;
            }
        }

        return true;
    }

    void OnPlayerDetected(GameObject player)
    {
        Debug.Log($"🎥 Caméra a détecté : {player.name}");

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.TriggerGameOver($"Repéré par une caméra !");
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugZone)
            return;

        DrawDetectionZone();
    }

    void OnDrawGizmosSelected()
    {
        // Toujours afficher quand sélectionné
        DrawDetectionZone();
    }

    void DrawDetectionZone()
    {
        // Couleur selon l'état
        Color zoneColor = Application.isPlaying && playerDetected ? detectionColor : normalColor;
        Gizmos.color = zoneColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Centre du cercle sur l'origine du GameObject
        Vector3 center = Vector3.zero;

        // Dessiner le cercle avec des segments
        int segments = 32;
        float angleStep = 360f / segments;

        // Contour du cercle
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = center + new Vector3(
                Mathf.Cos(angle1) * detectionRadius,
                Mathf.Sin(angle1) * detectionRadius,
                0
            );

            Vector3 point2 = center + new Vector3(
                Mathf.Cos(angle2) * detectionRadius,
                Mathf.Sin(angle2) * detectionRadius,
                0
            );

            Gizmos.DrawLine(point1, point2);
        }

        // Remplissage semi-transparent avec des lignes horizontales
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
        for (float y = -detectionRadius; y <= detectionRadius; y += 0.1f)
        {
            float halfWidth = Mathf.Sqrt(detectionRadius * detectionRadius - y * y);
            if (!float.IsNaN(halfWidth))
            {
                Gizmos.DrawLine(
                    center + new Vector3(-halfWidth, y, 0),
                    center + new Vector3(halfWidth, y, 0)
                );
            }
        }
    }
}
