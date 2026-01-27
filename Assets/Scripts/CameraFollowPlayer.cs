using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private bool followActiveCharacter = true; // Suivre le personnage actif du CharacterSwitcher

    private CharacterSwitcher characterSwitcher;

    [Header("Follow Constraints")]
    [SerializeField] private bool followX = false; // Ne suit pas horizontalement
    [SerializeField] private bool followY = true;  // Suit verticalement

    [Header("Smooth Follow")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private float fixedX; // Position X fixe de la caméra

    void Start()
    {
        // Trouver le CharacterSwitcher si on veut suivre le personnage actif
        if (followActiveCharacter)
        {
            characterSwitcher = FindObjectOfType<CharacterSwitcher>();
            if (characterSwitcher != null)
            {
                Debug.Log("CameraFollowPlayer: CharacterSwitcher trouvé - suivra le personnage actif");
            }
            else
            {
                Debug.LogWarning("CameraFollowPlayer: CharacterSwitcher non trouvé! Utilise le mode de suivi normal.");
                followActiveCharacter = false;
            }
        }

        // Trouver automatiquement le joueur si nécessaire (et si pas en mode CharacterSwitcher)
        if (!followActiveCharacter && autoFindPlayer && playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            if (player == null)
            {
                player = GameObject.Find("Capsule");
            }

            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"Camera found player: {player.name}");
            }
            else
            {
                Debug.LogWarning("CameraFollowPlayer: No player found! Please assign manually or tag your player as 'Player'");
            }
        }

        // Sauvegarder la position X initiale de la caméra
        fixedX = transform.position.x;
    }

    void LateUpdate()
    {
        // Ne pas suivre en mode 2 joueurs (le SplitScreenManager s'en charge)
        if (GameModeManager.Instance != null && GameModeManager.Instance.IsDuo())
        {
            return;
        }

        // Mettre à jour la cible si on suit le personnage actif
        if (followActiveCharacter && characterSwitcher != null)
        {
            GameObject activeCharacter = characterSwitcher.GetActiveCharacter();
            if (activeCharacter != null)
            {
                playerTransform = activeCharacter.transform;
            }
        }

        if (playerTransform == null) return;

        // Calculer la position cible
        Vector3 targetPosition = transform.position;

        // Suivre horizontalement (X) si activé
        if (followX)
        {
            targetPosition.x = playerTransform.position.x + offset.x;
        }
        else
        {
            // Garder la position X fixe
            targetPosition.x = fixedX;
        }

        // Suivre verticalement (Y) si activé
        if (followY)
        {
            targetPosition.y = playerTransform.position.y + offset.y;
        }

        // Toujours garder le Z pour la caméra
        targetPosition.z = offset.z;

        // Appliquer le mouvement (smooth ou instantané)
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, playerTransform.position);
        Gizmos.DrawWireSphere(playerTransform.position, 0.5f);
    }
}
