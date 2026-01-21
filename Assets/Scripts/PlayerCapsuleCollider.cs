using UnityEngine;

/// <summary>
/// Configure le système de colliders du joueur:
/// 1. CapsuleCollider2D principal arrondi pour les collisions physiques (évite les accrochages aux coins)
/// 2. BoxCollider2D trigger enfant pour la détection par les caméras et gardes
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCapsuleCollider : MonoBehaviour
{
    [Header("Capsule Collider - Collisions Physiques")]
    [Tooltip("Taille du collider physique principal (arrondi)")]
    [SerializeField] private Vector2 capsuleSize = new Vector2(0.9f, 1f);

    [Tooltip("Décalage vertical/horizontal du collider physique")]
    [SerializeField] private Vector2 capsuleOffset = new Vector2(0f, 0f);

    [Tooltip("Direction de la capsule (Vertical = personnage debout)")]
    [SerializeField] private CapsuleDirection2D direction = CapsuleDirection2D.Vertical;

    [Header("Detection Trigger - Pour Caméras/Gardes")]
    [Tooltip("Taille du trigger de détection (reste un rectangle)")]
    [SerializeField] private Vector2 detectionColliderSize = new Vector2(0.1f, 1f);

    [Tooltip("Décalage du trigger de détection")]
    [SerializeField] private Vector2 detectionColliderOffset = Vector2.zero;

    [Tooltip("Tag du trigger de détection")]
    [SerializeField] private string detectionTag = "Player";

    private CapsuleCollider2D mainCapsule;
    private GameObject detectionTriggerObject;
    private BoxCollider2D detectionCollider;

    void Awake()
    {
        SetupColliders();
    }

    void SetupColliders()
    {
        // 1. Configurer le CapsuleCollider2D principal
        ConvertToCapsule();

        // 2. Créer le trigger de détection
        SetupDetectionTrigger();
    }

    void ConvertToCapsule()
    {
        // Chercher et supprimer l'ancien BoxCollider2D si présent
        BoxCollider2D oldBoxCollider = GetComponent<BoxCollider2D>();
        if (oldBoxCollider != null)
        {
            DestroyImmediate(oldBoxCollider);
            Debug.Log($"✅ BoxCollider2D supprimé et remplacé par CapsuleCollider2D sur {gameObject.name}");
        }

        // Créer ou récupérer le CapsuleCollider2D
        mainCapsule = GetComponent<CapsuleCollider2D>();
        if (mainCapsule == null)
        {
            mainCapsule = gameObject.AddComponent<CapsuleCollider2D>();
        }

        // Configurer la capsule principale (collisions physiques)
        mainCapsule.size = capsuleSize;
        mainCapsule.offset = capsuleOffset;
        mainCapsule.direction = direction;
        mainCapsule.isTrigger = false; // Collider physique, pas un trigger

        Debug.Log($"✅ CapsuleCollider2D configuré: size={capsuleSize}, offset={capsuleOffset}, direction={direction}");
    }

    void SetupDetectionTrigger()
    {
        // Chercher si le trigger existe déjà
        Transform existingTrigger = transform.Find("DetectionTrigger");
        if (existingTrigger != null)
        {
            detectionTriggerObject = existingTrigger.gameObject;
            detectionCollider = detectionTriggerObject.GetComponent<BoxCollider2D>();
        }
        else
        {
            // Créer un GameObject enfant pour le trigger de détection
            detectionTriggerObject = new GameObject("DetectionTrigger");
            detectionTriggerObject.transform.SetParent(transform);
            detectionTriggerObject.transform.localRotation = Quaternion.identity;
            detectionTriggerObject.transform.localScale = Vector3.one;

            // Ajouter le BoxCollider2D trigger
            detectionCollider = detectionTriggerObject.AddComponent<BoxCollider2D>();
        }

        // Configurer le trigger
        detectionTriggerObject.transform.localPosition = detectionColliderOffset;
        detectionTriggerObject.tag = detectionTag;
        detectionCollider.isTrigger = true;
        detectionCollider.size = detectionColliderSize;

        Debug.Log($"✅ Trigger de détection créé: size={detectionColliderSize}, offset={detectionColliderOffset}");
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        // Dessiner le CapsuleCollider2D principal (jaune)
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Vector3 pos = transform.position + (Vector3)capsuleOffset;

        if (direction == CapsuleDirection2D.Vertical)
        {
            float radius = capsuleSize.x / 2f;
            float height = capsuleSize.y;
            float cylinderHeight = Mathf.Max(0, height - capsuleSize.x);

            // Cercles haut et bas
            Gizmos.DrawWireSphere(pos + Vector3.up * (cylinderHeight / 2f), radius);
            Gizmos.DrawWireSphere(pos + Vector3.down * (cylinderHeight / 2f), radius);

            // Lignes de côté
            Gizmos.DrawLine(pos + Vector3.up * (cylinderHeight / 2f) + Vector3.right * radius,
                           pos + Vector3.down * (cylinderHeight / 2f) + Vector3.right * radius);
            Gizmos.DrawLine(pos + Vector3.up * (cylinderHeight / 2f) + Vector3.left * radius,
                           pos + Vector3.down * (cylinderHeight / 2f) + Vector3.left * radius);
        }
        else
        {
            float radius = capsuleSize.y / 2f;
            float width = capsuleSize.x;
            float cylinderWidth = Mathf.Max(0, width - capsuleSize.y);

            Gizmos.DrawWireSphere(pos + Vector3.right * (cylinderWidth / 2f), radius);
            Gizmos.DrawWireSphere(pos + Vector3.left * (cylinderWidth / 2f), radius);

            Gizmos.DrawLine(pos + Vector3.right * (cylinderWidth / 2f) + Vector3.up * radius,
                           pos + Vector3.left * (cylinderWidth / 2f) + Vector3.up * radius);
            Gizmos.DrawLine(pos + Vector3.right * (cylinderWidth / 2f) + Vector3.down * radius,
                           pos + Vector3.left * (cylinderWidth / 2f) + Vector3.down * radius);
        }

        // Dessiner le trigger de détection (vert)
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 triggerPos = transform.position + (Vector3)detectionColliderOffset;
        Gizmos.DrawCube(triggerPos, new Vector3(detectionColliderSize.x, detectionColliderSize.y, 0.1f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(triggerPos, new Vector3(detectionColliderSize.x, detectionColliderSize.y, 0.1f));
    }
}
