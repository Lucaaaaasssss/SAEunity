using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Fait tourner LA ZONE DE DÉTECTION d'une caméra de surveillance
/// Le sprite de la caméra reste fixe, seule la zone de détection tourne
/// ATTENTION : Ce script doit être sur l'objet DetectionZone (enfant), pas sur la caméra parent !
/// </summary>
public class CameraRotation : MonoBehaviour
{
    [Header("Waypoints (Directions)")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private int currentWaypointIndex = 0;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f; // Degrés par seconde
    [SerializeField] private float waitTimeAtWaypoint = 2f; // Temps d'attente à chaque direction

    [Header("Debug")]
    [SerializeField] private bool showDebugLines = true;

    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Quaternion targetRotation;

    void Start()
    {
        if (waypoints.Length > 0)
        {
            UpdateTargetRotation();
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} : Aucun waypoint assigné !");
        }
    }

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        if (isWaiting)
        {
            // Attendre avant de passer au waypoint suivant
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
        }
        else
        {
            // Tourner progressivement vers la direction cible
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Vérifier si on a atteint la rotation cible
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isWaiting = true;
            }
        }
    }

    void MoveToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        UpdateTargetRotation();
    }

    void UpdateTargetRotation()
    {
        if (waypoints[currentWaypointIndex] == null)
        {
            Debug.LogWarning($"⚠️ Waypoint {currentWaypointIndex} est null !");
            return;
        }

        // Calculer la direction vers le waypoint
        Vector3 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;
        directionToWaypoint.z = 0; // 2D

        // Calculer l'angle en degrés
        float angle = Mathf.Atan2(directionToWaypoint.y, directionToWaypoint.x) * Mathf.Rad2Deg;

        // Créer la rotation cible (autour de l'axe Z pour 2D)
        targetRotation = Quaternion.Euler(0, 0, angle);
    }

    void OnDrawGizmos()
    {
        if (!showDebugLines || waypoints == null || waypoints.Length == 0)
            return;

        // Dessiner des lignes vers tous les waypoints
        Gizmos.color = Color.cyan;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawLine(transform.position, waypoint.position);
                Gizmos.DrawWireSphere(waypoint.position, 0.2f);
            }
        }

        // Dessiner la direction actuelle en jaune
        if (Application.isPlaying && waypoints.Length > 0)
        {
            Transform currentWaypoint = waypoints[currentWaypointIndex];
            if (currentWaypoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, currentWaypoint.position);
            }
        }

        // Dessiner la direction vers laquelle regarde la caméra
        Gizmos.color = Color.red;
        Vector3 forward = transform.right; // En 2D, "forward" est à droite
        Gizmos.DrawRay(transform.position, forward * 2f);
    }

#if UNITY_EDITOR
    [ContextMenu("Create Waypoints Circle")]
    public void CreateWaypointsCircle()
    {
        // Créer 4 waypoints en cercle autour de la caméra
        int count = 4;
        float radius = 3f;

        List<Transform> newWaypoints = new List<Transform>();

        // Trouver ou créer le conteneur
        Transform container = transform.Find("CameraWaypoints");
        if (container == null)
        {
            GameObject containerObj = new GameObject("CameraWaypoints");
            containerObj.transform.SetParent(transform.parent);
            containerObj.transform.position = transform.position;
            container = containerObj.transform;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            GameObject waypoint = new GameObject($"CameraWaypoint_{i + 1}");
            waypoint.transform.SetParent(container);
            waypoint.transform.position = transform.position + new Vector3(x, y, 0);

            // Ajouter le script de snap
            waypoint.AddComponent<WaypointGridSnap>();

            newWaypoints.Add(waypoint.transform);

            Undo.RegisterCreatedObjectUndo(waypoint, "Create Camera Waypoint");
        }

        waypoints = newWaypoints.ToArray();
        Debug.Log($"✅ {count} waypoints créés en cercle pour {gameObject.name}");
    }

    [ContextMenu("Clear Waypoints")]
    public void ClearWaypoints()
    {
        waypoints = new Transform[0];
        Debug.Log("✅ Waypoints effacés");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraRotation))]
public class CameraRotationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraRotation camera = (CameraRotation)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("═══ OUTILS ═══", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🎯 Créer 4 Waypoints en Cercle", GUILayout.Height(40)))
        {
            camera.CreateWaypointsCircle();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🗑️ Effacer les Waypoints", GUILayout.Height(30)))
        {
            camera.ClearWaypoints();
        }

        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "💡 UTILISATION :\n" +
            "1. Clique 'Créer 4 Waypoints en Cercle'\n" +
            "2. Déplace les waypoints où tu veux que la caméra regarde\n" +
            "3. La caméra tournera pour regarder chaque waypoint\n" +
            "4. Ajuste Rotation Speed et Wait Time",
            MessageType.Info);
    }
}
#endif
