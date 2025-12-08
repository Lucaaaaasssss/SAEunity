using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script qui "snap" automatiquement un waypoint au centre des carreaux de la grille
/// Attache ce script à tes waypoints pour faciliter leur placement
/// </summary>
public class WaypointGridSnap : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Taille d'un carreau en unités Unity (32px = 0.5 si PPU=64)")]
    [SerializeField] private float gridSize = 0.5f;

    [Header("Snap Settings")]
    [Tooltip("Snap automatiquement en temps réel dans l'éditeur")]
    [SerializeField] private bool autoSnapInEditor = true;

    [Header("Visualisation")]
    [SerializeField] private bool showGridGizmo = true;
    [SerializeField] private Color waypointColor = Color.cyan;
    [SerializeField] private float gizmoSize = 0.3f;

    private Vector3 lastPosition;

    void Start()
    {
        // Snap à la grille au démarrage
        SnapToGrid();
    }

    void OnValidate()
    {
        // Snap quand on change les valeurs dans l'Inspector
        if (autoSnapInEditor)
        {
            SnapToGrid();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showGridGizmo)
            return;

        // Dessiner le waypoint
        Gizmos.color = waypointColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.DrawSphere(transform.position, gizmoSize * 0.5f);

        // Dessiner le carreau de la grille
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        DrawGridTile(transform.position, gridSize);

        // Afficher les coordonnées
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        style.fontSize = 12;
        Vector3 screenPos = transform.position + Vector3.up * (gizmoSize + 0.3f);
        Handles.Label(screenPos, $"({transform.position.x:F1}, {transform.position.y:F1})", style);
    }

    void DrawGridTile(Vector3 center, float size)
    {
        float halfSize = size / 2f;
        Vector3 topLeft = new Vector3(center.x - halfSize, center.y + halfSize, 0);
        Vector3 topRight = new Vector3(center.x + halfSize, center.y + halfSize, 0);
        Vector3 bottomLeft = new Vector3(center.x - halfSize, center.y - halfSize, 0);
        Vector3 bottomRight = new Vector3(center.x + halfSize, center.y - halfSize, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
#endif

    /// <summary>
    /// Snap la position du waypoint au centre du carreau le plus proche
    /// </summary>
    [ContextMenu("Snap to Grid")]
    public void SnapToGrid()
    {
        Vector3 pos = transform.position;

        // Arrondir au centre du carreau le plus proche
        // Le centre d'un carreau est à 0.5, 1.5, 2.5, etc. si gridSize = 1
        float snappedX = Mathf.Round(pos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(pos.y / gridSize) * gridSize;

        transform.position = new Vector3(snappedX, snappedY, pos.z);
    }

    /// <summary>
    /// Snap au coin de la grille (0, 1, 2, 3, etc.) au lieu du centre
    /// </summary>
    [ContextMenu("Snap to Grid Corner")]
    public void SnapToGridCorner()
    {
        Vector3 pos = transform.position;

        float snappedX = Mathf.Round(pos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(pos.y / gridSize) * gridSize;

        // Enlever 0.5 pour aller au coin
        snappedX = Mathf.Floor(snappedX / gridSize) * gridSize;
        snappedY = Mathf.Floor(snappedY / gridSize) * gridSize;

        transform.position = new Vector3(snappedX, snappedY, pos.z);
    }
}

#if UNITY_EDITOR
/// <summary>
/// Éditeur custom pour faciliter le snap en temps réel
/// </summary>
[CustomEditor(typeof(WaypointGridSnap))]
public class WaypointGridSnapEditor : Editor
{
    void OnSceneGUI()
    {
        WaypointGridSnap waypoint = (WaypointGridSnap)target;

        EditorGUI.BeginChangeCheck();
        Vector3 newPos = Handles.PositionHandle(waypoint.transform.position, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(waypoint.transform, "Move Waypoint");
            waypoint.transform.position = newPos;

            // Snap automatiquement si activé
            waypoint.SnapToGrid();
        }
    }
}
#endif
