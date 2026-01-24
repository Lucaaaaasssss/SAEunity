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

    [Header("Collider")]
    [SerializeField] private Collider2D doorCollider;

    [Header("State")]
    [SerializeField] private bool isOpen = false;

    private GameObject indicatorInstance;
    private SpriteRenderer indicatorRenderer;
    private bool isPlayerNear = false;
    private bool hasKey = false;

    [Serializable]
    public class DoorPart
    {
        public SpriteRenderer spriteRenderer;
        public Sprite closedSprite;
        public Sprite openSprite;
    }

    void Start()
    {
        // Auto-find collider si non assigné
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

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

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= interactionRange)
            {
                isPlayerNear = true;

                if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(requiredItem))
                {
                    hasKey = true;
                }
                break;
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

        if (consumeKey && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.RemoveItem(requiredItem);
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

        // Désactiver le collider quand ouvert
        if (doorCollider != null)
        {
            doorCollider.enabled = !isOpen;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
