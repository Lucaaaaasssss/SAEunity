using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float horizontalSpeed = 3f;

    [Header("Arcade Controls")]
    [SerializeField] private bool useArcadeControls = true;
    [SerializeField] private string horizontalAxis = "P1_Horizontal";
    [SerializeField] private string verticalAxis = "P1_Vertical";

    [Header("Boundaries")]
    [SerializeField] private bool constrainToBounds = true;
    [SerializeField] private float boundaryPadding = 0.5f;
    [SerializeField] private Transform groundTransform; // Référence au Ground
    [SerializeField] private bool autoFindGround = true; // Trouver automatiquement le Ground

    private Camera mainCamera;
    private float minX, maxX, minY, maxY;
    private bool boundariesCalculated = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool controlEnabled = true; // Pour le système de switch de personnages
    private bool timerStarted = false; // Pour démarrer le timer au premier mouvement

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Trouver automatiquement le Ground si nécessaire
        if (autoFindGround && groundTransform == null)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                groundTransform = ground.transform;
                Debug.Log("PlayerMovement: Ground found automatically");
            }
            else
            {
                Debug.LogWarning("PlayerMovement: Ground not found! Boundaries will use camera view.");
            }
        }

        // Ne pas calculer les limites immédiatement, attendre que Ground soit initialisé
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Ne pas traiter les inputs si le contrôle est désactivé
        if (!controlEnabled) return;

        // Calculer les limites si pas encore fait et que le Ground a une taille valide
        if (!boundariesCalculated && constrainToBounds)
        {
            if (groundTransform != null)
            {
                bool isGroundReady = false;

                // Vérifier si c'est un TiledGround
                TiledGround tiledGround = groundTransform.GetComponent<TiledGround>();
                if (tiledGround != null)
                {
                    // Vérifier si les dimensions sont valides
                    Vector2 dimensions = tiledGround.GetGroundDimensions();
                    isGroundReady = dimensions.magnitude > 1f;
                }
                else
                {
                    // Vérifier le localScale pour les autres types de Ground
                    isGroundReady = groundTransform.localScale.magnitude > 3f;
                }

                if (isGroundReady)
                {
                    CalculateBoundaries();
                    boundariesCalculated = true;
                }
            }
        }

        // Récupérer les inputs (arcade ou standard)
        float horizontalInput = useArcadeControls ? Input.GetAxis(horizontalAxis) : Input.GetAxis("Horizontal");
        float verticalInput = useArcadeControls ? Input.GetAxis(verticalAxis) : Input.GetAxis("Vertical");

        // Démarrer le timer au premier mouvement
        if (!timerStarted && (horizontalInput != 0 || verticalInput != 0))
        {
            if (SpeedrunTimer.Instance != null)
            {
                SpeedrunTimer.Instance.StartTimer();
                timerStarted = true;
                Debug.Log("Speedrun timer started!");
            }
        }

        // Calculer le mouvement
        // Le jeu se déplace principalement vers le haut
        Vector3 movement = new Vector3(
            horizontalInput * horizontalSpeed,
            verticalInput * moveSpeed,
            0
        );

        // Appliquer le mouvement
        transform.position += movement * Time.deltaTime;

        // Contrôler l'animation selon la direction
        if (animator != null)
        {
            bool isMovingDown = verticalInput < 0; // Vers le bas
            bool isMovingUp = verticalInput > 0;   // Vers le haut
            bool isMovingHorizontal = horizontalInput != 0 && verticalInput == 0; // Horizontal seulement si pas de vertical

            animator.SetBool("isMovingDown", isMovingDown);
            animator.SetBool("isMovingUp", isMovingUp);
            animator.SetBool("isMovingRight", isMovingHorizontal);

            // Gérer le flip du sprite pour gauche/droite
            if (spriteRenderer != null && horizontalInput != 0)
            {
                // Si on va à gauche, flipper le sprite
                spriteRenderer.flipX = horizontalInput < 0;
            }
        }

        // Contraindre aux limites de l'écran si activé
        if (constrainToBounds && boundariesCalculated)
        {
            ClampPositionToBounds();
        }
    }

    void CalculateBoundaries()
    {
        if (groundTransform != null)
        {
            Vector3 groundPos = groundTransform.position;
            float halfWidth, halfHeight;

            // Vérifier si c'est un TiledGround
            TiledGround tiledGround = groundTransform.GetComponent<TiledGround>();
            if (tiledGround != null)
            {
                // Utiliser les dimensions du TiledGround
                Vector2 dimensions = tiledGround.GetGroundDimensions();
                halfWidth = dimensions.x / 2f;
                halfHeight = dimensions.y / 2f;
            }
            else
            {
                // Fallback: utiliser le localScale (pour ArcadeGround ou autres)
                Vector3 groundScale = groundTransform.localScale;
                halfWidth = groundScale.x / 2f;
                halfHeight = groundScale.y / 2f;
            }

            minX = groundPos.x - halfWidth + boundaryPadding;
            maxX = groundPos.x + halfWidth - boundaryPadding;
            minY = groundPos.y - halfHeight + boundaryPadding;
            maxY = groundPos.y + halfHeight - boundaryPadding;

            Debug.Log($"Boundaries set from Ground: X({minX} to {maxX}), Y({minY} to {maxY})");
        }
        else if (mainCamera != null)
        {
            // Fallback: utiliser la zone visible de la caméra
            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;

            minX = mainCamera.transform.position.x - camWidth + boundaryPadding;
            maxX = mainCamera.transform.position.x + camWidth - boundaryPadding;
            minY = mainCamera.transform.position.y - camHeight + boundaryPadding;
            maxY = mainCamera.transform.position.y + camHeight - boundaryPadding;

            Debug.Log("Boundaries set from camera view (Ground not found)");
        }
    }

    void ClampPositionToBounds()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    // Optionnel: pour un scrolling automatique vers le haut
    [Header("Auto Scroll")]
    [SerializeField] private bool autoScrollUp = false;
    [SerializeField] private float autoScrollSpeed = 2f;

    void LateUpdate()
    {
        if (autoScrollUp)
        {
            transform.position += Vector3.up * autoScrollSpeed * Time.deltaTime;
        }
    }

    // Méthode publique pour activer/désactiver le contrôle (utilisée par CharacterSwitcher)
    public void SetControlEnabled(bool enabled)
    {
        controlEnabled = enabled;
    }

    void OnDrawGizmos()
    {
        if (!constrainToBounds) return;

        // Si on a les valeurs calculées, les utiliser
        if (minX != 0 || maxX != 0 || minY != 0 || maxY != 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0),
                new Vector3(maxX - minX, maxY - minY, 0)
            );
        }
        // Sinon afficher les limites du Ground en mode éditeur
        else if (groundTransform != null)
        {
            Vector3 groundScale = groundTransform.localScale;
            Vector3 groundPos = groundTransform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundPos, new Vector3(groundScale.x, groundScale.y, 0));
        }
    }
}
