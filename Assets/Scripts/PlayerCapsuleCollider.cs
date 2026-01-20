using UnityEngine;

/// <summary>
/// Convertit le BoxCollider2D du joueur en CapsuleCollider2D pour éviter les accrochages aux coins des murs
/// Les capsules ont des bords arrondis qui glissent beaucoup mieux
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCapsuleCollider : MonoBehaviour
{
    [Header("Capsule Collider Settings")]
    [SerializeField] private Vector2 capsuleSize = new Vector2(0.9f, 1f);
    [SerializeField] private Vector2 capsuleOffset = new Vector2(0f, 0f);
    [SerializeField] private CapsuleDirection2D direction = CapsuleDirection2D.Vertical;

    void Awake()
    {
        ConvertToCapsule();
    }

    void ConvertToCapsule()
    {
        // Chercher le BoxCollider2D existant
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            // Sauvegarder les paramètres
            Vector2 oldSize = boxCollider.size;
            Vector2 oldOffset = boxCollider.offset;
            bool wasTrigger = boxCollider.isTrigger;

            // Détruire le BoxCollider2D
            DestroyImmediate(boxCollider);

            Debug.Log($"✅ BoxCollider2D converti en CapsuleCollider2D pour {gameObject.name}");
        }

        // Vérifier si on a déjà un CapsuleCollider2D
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider2D>();
        }

        // Configurer la capsule
        capsule.size = capsuleSize;
        capsule.offset = capsuleOffset;
        capsule.direction = direction;
        capsule.isTrigger = false; // Le collider principal n'est jamais un trigger

        Debug.Log($"✅ CapsuleCollider2D configuré pour {gameObject.name} : size={capsuleSize}, offset={capsuleOffset}");
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        // Dessiner la forme de la capsule
        Vector3 pos = transform.position;
        if (direction == CapsuleDirection2D.Vertical)
        {
            float radius = capsuleSize.x / 2f;
            float height = capsuleSize.y;

            // Dessiner les cercles haut et bas
            Gizmos.DrawWireSphere(pos + Vector3.up * (height / 2f - radius), radius);
            Gizmos.DrawWireSphere(pos + Vector3.down * (height / 2f - radius), radius);
        }
        else
        {
            float radius = capsuleSize.y / 2f;
            float width = capsuleSize.x;

            Gizmos.DrawWireSphere(pos + Vector3.right * (width / 2f - radius), radius);
            Gizmos.DrawWireSphere(pos + Vector3.left * (width / 2f - radius), radius);
        }
    }
}
