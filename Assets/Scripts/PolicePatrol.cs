using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PolicePatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints; // Points de passage du pattern
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waypointReachDistance = 0.1f;
    [SerializeField] private float waitTimeAtWaypoint = 1f; // Temps d'attente à chaque point

    [Header("Visual")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Detection")]
    [SerializeField] private GridBasedDetectionZone detectionZone; // Zone de détection enfant

    [Header("Waypoint Creation (Editor Only)")]
    [SerializeField] private int waypointsToCreate = 4;
    [SerializeField] private float waypointSpacing = 1.5f;
    [SerializeField] private bool createInCircle = true;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Vector3 lastPosition;
    private GridBasedDetectionZone.DetectionDirection lastDirection = GridBasedDetectionZone.DetectionDirection.Right; // Direction actuelle

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        lastPosition = transform.position;

        // Vérifier qu'il y a des waypoints
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("PolicePatrol: No waypoints assigned!");
        }

        // Initialiser la direction de la zone de détection
        if (detectionZone != null && waypoints.Length > 0)
        {
            Vector3 initialDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
            UpdateDetectionDirection(initialDirection);
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (isWaiting)
        {
            HandleWaiting();
        }
        else
        {
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        // Récupérer le waypoint cible
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        if (targetWaypoint == null)
        {
            Debug.LogWarning($"Waypoint {currentWaypointIndex} is null!");
            return;
        }

        // Calculer la direction vers le waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;

        // Déplacer le policier
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Mettre à jour les animations selon la direction
        UpdateAnimation(direction);

        // Vérifier si on a atteint le waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint <= waypointReachDistance)
        {
            // On a atteint le waypoint, commencer l'attente
            isWaiting = true;
            waitTimer = 0f;

            // Passer au prochain waypoint (boucle)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void HandleWaiting()
    {
        waitTimer += Time.deltaTime;

        // Arrêter les animations pendant l'attente (direction = 0)
        if (animator != null)
        {
            animator.SetFloat("DirectionX", 0);
            animator.SetFloat("DirectionY", 0);
        }

        // La zone de détection garde automatiquement sa dernière direction

        if (waitTimer >= waitTimeAtWaypoint)
        {
            isWaiting = false;
        }
    }

    void UpdateAnimation(Vector3 direction)
    {
        if (animator == null)
            return;

        // Envoyer la direction normalisée au blend tree
        animator.SetFloat("DirectionX", direction.x);
        animator.SetFloat("DirectionY", direction.y);

        // Flip le sprite UNIQUEMENT pour les mouvements principalement horizontaux
        if (spriteRenderer != null)
        {
            bool isMovingHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
            if (isMovingHorizontal)
            {
                // Flipper uniquement si le mouvement est principalement horizontal
                spriteRenderer.flipX = direction.x < 0;
            }
            else
            {
                // Ne pas flipper pendant les mouvements verticaux (front/back)
                spriteRenderer.flipX = false;
            }
        }

        // Mettre à jour la direction de détection
        UpdateDetectionDirection(direction);
    }

    void UpdateDetectionDirection(Vector3 direction)
    {
        if (detectionZone == null)
            return;

        GridBasedDetectionZone.DetectionDirection newDirection;

        // Déterminer la direction principale
        bool isMovingHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);

        if (isMovingHorizontal)
        {
            newDirection = direction.x > 0 ?
                GridBasedDetectionZone.DetectionDirection.Right :
                GridBasedDetectionZone.DetectionDirection.Left;
        }
        else
        {
            newDirection = direction.y > 0 ?
                GridBasedDetectionZone.DetectionDirection.Up :
                GridBasedDetectionZone.DetectionDirection.Down;
        }

        // Ne mettre à jour que si la direction a changé
        if (newDirection != lastDirection)
        {
            lastDirection = newDirection;
            detectionZone.UpdateDetectionShape(newDirection);
        }
    }

    // Méthode pour dessiner les waypoints dans l'éditeur
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        // Dessiner les waypoints
        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                // Dessiner une sphère au waypoint
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

                // Dessiner une ligne vers le prochain waypoint
                int nextIndex = (i + 1) % waypoints.Length;
                if (waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                }
            }
        }
    }

#if UNITY_EDITOR
    // ===== MÉTHODES D'ÉDITION (EDITOR ONLY) =====

    [ContextMenu("Create Waypoints")]
    public void CreateWaypoints()
    {
        // Trouver ou créer le parent
        string parentName = $"{gameObject.name}_Waypoints";
        Transform parent = transform.parent?.Find(parentName);

        if (parent == null)
        {
            GameObject parentObj = new GameObject(parentName);
            if (transform.parent != null)
                parentObj.transform.SetParent(transform.parent);
            parentObj.transform.position = transform.position;
            parent = parentObj.transform;
            Undo.RegisterCreatedObjectUndo(parentObj, "Create Waypoints Parent");
        }

        // Créer les waypoints
        Transform[] newWaypoints = new Transform[waypointsToCreate];

        for (int i = 0; i < waypointsToCreate; i++)
        {
            GameObject waypoint = new GameObject($"Waypoint_{i + 1}");
            waypoint.transform.SetParent(parent);

            // Position
            if (createInCircle)
            {
                float angle = (360f / waypointsToCreate) * i * Mathf.Deg2Rad;
                waypoint.transform.position = transform.position + new Vector3(
                    Mathf.Cos(angle) * waypointSpacing,
                    Mathf.Sin(angle) * waypointSpacing,
                    0
                );
            }
            else
            {
                // En ligne
                waypoint.transform.position = transform.position + Vector3.right * waypointSpacing * i;
            }

            // Ajouter WaypointGridSnap
            waypoint.AddComponent<WaypointGridSnap>();
            Undo.RegisterCreatedObjectUndo(waypoint, "Create Waypoint");

            newWaypoints[i] = waypoint.transform;
        }

        // Assigner au PolicePatrol
        Undo.RecordObject(this, "Assign Waypoints");
        waypoints = newWaypoints;
        EditorUtility.SetDirty(this);

        Debug.Log($"✅ {waypointsToCreate} waypoints créés pour {gameObject.name}!");
        Selection.activeGameObject = parent.gameObject;
    }

    [ContextMenu("Clear Waypoints")]
    public void ClearWaypoints()
    {
        Undo.RecordObject(this, "Clear Waypoints");
        waypoints = new Transform[0];
        EditorUtility.SetDirty(this);
        Debug.Log($"✅ Waypoints effacés de {gameObject.name}");
    }

    [ContextMenu("Show Patrol Info")]
    public void ShowPatrolInfo()
    {
        Debug.Log($"=== PATROL INFO: {gameObject.name} ===");
        Debug.Log($"Nombre de waypoints: {(waypoints != null ? waypoints.Length : 0)}");
        Debug.Log($"Move Speed: {moveSpeed}");
        Debug.Log($"Wait Time: {waitTimeAtWaypoint}s");

        if (waypoints != null)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Debug.Log($"  [{i}] {waypoints[i].name} at {waypoints[i].position}");
                }
                else
                {
                    Debug.LogWarning($"  [{i}] NULL WAYPOINT!");
                }
            }
        }
    }
#endif
}

#if UNITY_EDITOR
// Éditeur custom pour ajouter des boutons dans l'Inspector
[CustomEditor(typeof(PolicePatrol))]
public class PolicePatrolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PolicePatrol patrol = (PolicePatrol)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Actions Rapides", EditorStyles.boldLabel);

        if (GUILayout.Button("🎯 Créer les Waypoints", GUILayout.Height(40)))
        {
            patrol.CreateWaypoints();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("🗑️ Effacer les Waypoints", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirmer", "Effacer tous les waypoints ?", "Oui", "Non"))
            {
                patrol.ClearWaypoints();
            }
        }

        GUILayout.Space(5);

        if (GUILayout.Button("ℹ️ Afficher Info", GUILayout.Height(30)))
        {
            patrol.ShowPatrolInfo();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "1. Configure 'Waypoints To Create' et 'Waypoint Spacing'\n" +
            "2. Clique '🎯 Créer les Waypoints'\n" +
            "3. Déplace-les dans la Scene (snap auto à la grille)\n" +
            "4. Le policier patrouille en boucle infinie ! 🔄",
            MessageType.Info);
    }
}
#endif
