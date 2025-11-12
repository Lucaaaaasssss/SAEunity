using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ArcadeCameraSetup : MonoBehaviour
{
    [Header("Arcade Screen Settings")]
    [SerializeField] private float screenWidth = 1920f;
    [SerializeField] private float screenHeight = 1080f;

    [Header("Camera Settings")]
    [SerializeField] private float pixelsPerUnit = 100f; // Nombre de pixels par unité Unity
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        SetupCamera();
    }

    void SetupCamera()
    {
        // S'assurer que la caméra est orthographique (pour 2D)
        cam.orthographic = true;

        // Calculer la taille orthographique pour que la hauteur de l'écran
        // corresponde exactement à la résolution verticale
        cam.orthographicSize = screenHeight / (2f * pixelsPerUnit);

        // Définir la couleur de fond
        cam.backgroundColor = backgroundColor;

        // Position pour vue du dessus en 2D
        // En 2D Unity, la caméra regarde déjà dans l'axe Z+ vers Z-
        // Donc on garde juste une position standard
        transform.position = new Vector3(0, 0, -10);
        transform.rotation = Quaternion.identity;

        Debug.Log($"Camera orthographic size set to: {cam.orthographicSize} units");
        Debug.Log($"Visible area: {GetVisibleWidth()} x {GetVisibleHeight()} units");
    }

    public float GetVisibleHeight()
    {
        return cam.orthographicSize * 2f;
    }

    public float GetVisibleWidth()
    {
        return GetVisibleHeight() * cam.aspect;
    }

    void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();

        // Dessiner les limites de la caméra dans l'éditeur
        Gizmos.color = Color.yellow;
        float height = GetVisibleHeight();
        float width = GetVisibleWidth();

        Vector3 topLeft = transform.position + new Vector3(-width / 2, height / 2, 10);
        Vector3 topRight = transform.position + new Vector3(width / 2, height / 2, 10);
        Vector3 bottomLeft = transform.position + new Vector3(-width / 2, -height / 2, 10);
        Vector3 bottomRight = transform.position + new Vector3(width / 2, -height / 2, 10);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
