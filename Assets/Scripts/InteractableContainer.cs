using UnityEngine;

/// <summary>
/// Conteneur interactif (armoire, coffre, tiroir...)
/// S'ouvre quand le joueur interagit et révèle son contenu
/// </summary>
public class InteractableContainer : MonoBehaviour
{
    [Header("Container Sprites")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Content")]
    [Tooltip("L'objet caché à l'intérieur (ex: clé). Sera activé quand le conteneur s'ouvre.")]
    [SerializeField] private GameObject hiddenContent;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.R;
    [SerializeField] private string arcadeButton = "P1_B2";

    [Header("Visual Indicator")]
    [SerializeField] private Sprite indicatorSprite;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 1f, 0);

    private SpriteRenderer spriteRenderer;
    private GameObject indicatorInstance;
    private bool isPlayerNear = false;
    private bool isOpen = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Cacher le contenu au départ
        if (hiddenContent != null)
        {
            hiddenContent.SetActive(false);
        }

        CreateIndicator();

        // Mettre le sprite fermé
        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }

    void CreateIndicator()
    {
        indicatorInstance = new GameObject("ContainerIndicator");
        indicatorInstance.transform.SetParent(transform);
        indicatorInstance.transform.localPosition = indicatorOffset;

        SpriteRenderer indicatorRenderer = indicatorInstance.AddComponent<SpriteRenderer>();
        indicatorRenderer.sortingOrder = 100;

        if (indicatorSprite != null)
        {
            indicatorRenderer.sprite = indicatorSprite;
        }
        else
        {
            // Créer un carré jaune par défaut
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.yellow;
            tex.SetPixels(colors);
            tex.Apply();
            indicatorRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            indicatorInstance.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }

        indicatorInstance.SetActive(false);
    }

    void Update()
    {
        if (isOpen) return;

        CheckForPlayers();

        if (isPlayerNear)
        {
            if (Input.GetKeyDown(interactKey) || Input.GetButtonDown(arcadeButton))
            {
                OpenContainer();
            }
        }
    }

    void CheckForPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        isPlayerNear = false;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= interactionRange)
            {
                isPlayerNear = true;
                break;
            }
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(isPlayerNear && !isOpen);
        }
    }

    void OpenContainer()
    {
        isOpen = true;
        Debug.Log($"{gameObject.name} ouvert!");

        // Changer le sprite
        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        // Révéler le contenu caché
        if (hiddenContent != null)
        {
            hiddenContent.SetActive(true);
        }

        // Cacher l'indicateur
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
