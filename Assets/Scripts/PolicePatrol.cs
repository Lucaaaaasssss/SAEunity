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
    [SerializeField] private GridBasedDetectionZone detectionZone; // Zone de détection enfant

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

        // Arrêter les animations pendant l'attente
        if (animator != null)
        {
            animator.SetBool("isMovingDown", false);
            animator.SetBool("isMovingUp", false);
            animator.SetBool("isMovingRight", false);
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
}
