using UnityEngine;
using System;

/// <summary>
/// Porte qui nécessite une clé pour s'ouvrir
/// Supporte les portes composées de plusieurs morceaux
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Door Parts (si plusieurs morceaux)")]
    [SerializeField] private DoorPart[] doorParts;

    [Header("Single Door (si un seul sprite)")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Key Settings")]
    [SerializeField] private string requiredItem = "Key";
    [SerializeField] private bool consumeKey = true;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode openKey = KeyCode.R;
    [SerializeField] private string arcadeButton = "P1_B2";

    [Header("Visual Indicator")]
    [SerializeField] private Sprite indicatorSprite;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 1f, 0);

    [Header("Colliders")]
    [SerializeField] private Collider2D[] doorColliders;

    [Header("State")]
    [SerializeField] private bool isOpen = false;

    private GameObject indicatorInstance;
    private SpriteRenderer indicatorRenderer;
    private bool isPlayerNear = false;
    private bool hasKey = false;
    private PlayerInventory playerWithKey;

    [Serializable]
    public class DoorPart
    {
        public SpriteRenderer spriteRenderer;
        public Sprite closedSprite;
        public Sprite openSprite;
    }

    void Start()
    {
        // Auto-find colliders si non assignés
        if (doorColliders == null || doorColliders.Length == 0)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                doorColliders = new Collider2D[] { col };
            }
        }

        // Si pas de parts définies, chercher dans les enfants
        if ((doorParts == null || doorParts.Length == 0) && spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        CreateIndicator();
        UpdateDoorState();
    }

    void CreateIndicator()
    {
        indicatorInstance = new GameObject("DoorIndicator");
        indicatorInstance.transform.SetParent(transform);
        indicatorInstance.transform.localPosition = indicatorOffset;

        indicatorRenderer = indicatorInstance.AddComponent<SpriteRenderer>();
        indicatorRenderer.sortingOrder = 100;

        if (indicatorSprite != null)
        {
            indicatorRenderer.sprite = indicatorSprite;
        }
        else
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.green;
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

        if (isPlayerNear && hasKey)
        {
            if (Input.GetKeyDown(openKey) || Input.GetButtonDown(arcadeButton))
            {
                Open();
            }
        }
    }

    void CheckForPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        isPlayerNear = false;
        hasKey = false;
        playerWithKey = null;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= interactionRange)
            {
                isPlayerNear = true;

                // Vérifier si CE joueur a la clé
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.HasItem(requiredItem))
                {
                    hasKey = true;
                    playerWithKey = inventory;
                    break; // On a trouvé un joueur avec la clé, on peut arrêter
                }
                // Sinon on continue à chercher parmi les autres joueurs
            }
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(isPlayerNear && hasKey);
        }
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;

        // Retirer la clé de l'inventaire du joueur qui l'a
        if (consumeKey && playerWithKey != null)
        {
            playerWithKey.RemoveItem(requiredItem);
        }

        UpdateDoorState();
        Debug.Log($"Porte ouverte avec {requiredItem}!");

        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        UpdateDoorState();
    }

    void UpdateDoorState()
    {
        // Mettre à jour les parties multiples
        if (doorParts != null && doorParts.Length > 0)
        {
            foreach (DoorPart part in doorParts)
            {
                if (part.spriteRenderer != null)
                {
                    part.spriteRenderer.sprite = isOpen ? part.openSprite : part.closedSprite;
                }
            }
        }
        // Ou le sprite unique
        else if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
        }

        // Désactiver les colliders quand ouvert
        if (doorColliders != null)
        {
            foreach (Collider2D col in doorColliders)
            {
                if (col != null)
                {
                    col.enabled = !isOpen;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
