using UnityEngine;

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
    [SerializeField] private Transform detectionZone; // Zone de détection enfant

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Vector3 lastPosition;
    private float lastDetectionOffset = 0.7f; // Mémoriser la dernière position de la zone
    private bool detectionZoneInitialized = false; // Pour s'assurer que la zone est bien initialisée

    void OnEnable()
    {
        // Réinitialiser le flag à chaque fois que l'objet est activé (y compris après un reload)
        detectionZoneInitialized = false;
    }

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

        // Marquer comme non initialisé pour forcer l'initialisation dans Update
        detectionZoneInitialized = false;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        // S'assurer que la zone de détection est bien initialisée
        if (!detectionZoneInitialized && detectionZone != null && waypoints.Length > 0)
        {
            Vector3 initialDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
            if (initialDirection.magnitude > 0.01f) // S'assurer qu'on a une direction valide
            {
                lastDetectionOffset = initialDirection.x < 0 ? -0.7f : 0.7f;
                detectionZone.localPosition = new Vector3(lastDetectionOffset, 0, 0);
                detectionZoneInitialized = true;
                Debug.Log($"DetectionZone initialisée à : {detectionZone.localPosition}");
            }
        }

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

        // Arrêter les animations pendant l'attente
        if (animator != null)
        {
            animator.SetBool("isMovingDown", false);
            animator.SetBool("isMovingUp", false);
            animator.SetBool("isMovingRight", false);
        }

        // Garder la zone de détection dans la dernière direction
        if (detectionZone != null)
        {
            detectionZone.localPosition = new Vector3(lastDetectionOffset, 0, 0);
            // Debug pour voir si la position est bien maintenue
            if (Time.frameCount % 30 == 0) // Log toutes les 30 frames pour ne pas spammer
            {
                Debug.Log($"HandleWaiting: DetectionZone position = {detectionZone.localPosition}, lastOffset = {lastDetectionOffset}");
            }
        }

        if (waitTimer >= waitTimeAtWaypoint)
        {
            isWaiting = false;
        }
    }

    void UpdateAnimation(Vector3 direction)
    {
        if (animator == null)
            return;

        // Déterminer la direction principale
        bool isMovingHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);

        if (isMovingHorizontal)
        {
            // Mouvement horizontal
            animator.SetBool("isMovingRight", true);
            animator.SetBool("isMovingDown", false);
            animator.SetBool("isMovingUp", false);

            // Flip le sprite selon la direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }

            // Déplacer la zone de détection devant le policier
            if (detectionZone != null)
            {
                // Si le sprite est flippé (va à gauche), mettre la zone à gauche
                // Sinon, mettre la zone à droite
                lastDetectionOffset = direction.x < 0 ? -0.7f : 0.7f;
                detectionZone.localPosition = new Vector3(lastDetectionOffset, 0, 0);

                // Debug pour voir si la position change pendant le mouvement
                if (Time.frameCount % 30 == 0)
                {
                    Debug.Log($"HandleMovement: DetectionZone position = {detectionZone.localPosition}, direction.x = {direction.x}");
                }
            }
        }
        else
        {
            // Mouvement vertical
            animator.SetBool("isMovingRight", false);

            if (direction.y > 0)
            {
                // Vers le haut
                animator.SetBool("isMovingUp", true);
                animator.SetBool("isMovingDown", false);
            }
            else
            {
                // Vers le bas
                animator.SetBool("isMovingDown", true);
                animator.SetBool("isMovingUp", false);
            }
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
}
