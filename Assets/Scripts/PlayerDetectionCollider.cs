using UnityEngine;

/// <summary>
/// Ajoute un collider trigger plus petit pour la détection par les gardes
/// Le collider principal reste pour les collisions physiques avec les murs
/// </summary>
public class PlayerDetectionCollider : MonoBehaviour
{
    [Header("Detection Collider (pour les gardes)")]
    [SerializeField] private Vector2 detectionColliderSize = new Vector2(0.1f, 1f);
    [SerializeField] private Vector2 detectionColliderOffset = Vector2.zero;
    [SerializeField] private string detectionTag = "Player"; // Tag pour la détection (peut être différent)

    private BoxCollider2D detectionCollider;

    void Start()
    {
        SetupDetectionCollider();
    }

    void SetupDetectionCollider()
    {
        // Vérifier qu'il y a déjà un collider principal (pour les murs)
        BoxCollider2D mainCollider = GetComponent<BoxCollider2D>();
        if (mainCollider == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} n'a pas de BoxCollider2D principal !");
            return;
        }

        // Créer un GameObject enfant pour le collider de détection
        GameObject detectionObj = new GameObject("DetectionTrigger");
        detectionObj.transform.SetParent(transform);
        detectionObj.transform.localPosition = detectionColliderOffset;
        detectionObj.transform.localRotation = Quaternion.identity;
        detectionObj.transform.localScale = Vector3.one;

        // Mettre le tag de détection sur le trigger
        detectionObj.tag = detectionTag;

        // Ajouter un BoxCollider2D en trigger
        detectionCollider = detectionObj.AddComponent<BoxCollider2D>();
        detectionCollider.isTrigger = true;
        detectionCollider.size = detectionColliderSize;

        Debug.Log($"✅ Collider de détection créé pour {gameObject.name} : {detectionColliderSize}");
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(detectionColliderOffset, new Vector3(detectionColliderSize.x, detectionColliderSize.y, 0.1f));

        // Contour
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(detectionColliderOffset, new Vector3(detectionColliderSize.x, detectionColliderSize.y, 0.1f));
    }
}
