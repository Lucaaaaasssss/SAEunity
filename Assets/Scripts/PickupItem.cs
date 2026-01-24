using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Key";

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private KeyCode pickupKey = KeyCode.R;
    [SerializeField] private string arcadeButton = "P1_B2";

    [Header("Visual Indicator")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Sprite defaultIndicatorSprite;

    private GameObject indicatorInstance;
    private Transform playerInRange;
    private bool isPlayerNear = false;

    void Start()
    {
        // Créer l'indicateur s'il n'y a pas de prefab
        if (indicatorPrefab == null)
        {
            CreateDefaultIndicator();
        }
        else
        {
            indicatorInstance = Instantiate(indicatorPrefab, transform.position + indicatorOffset, Quaternion.identity, transform);
        }

        // Cacher l'indicateur au départ
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(false);
        }
    }

    void CreateDefaultIndicator()
    {
        indicatorInstance = new GameObject("PickupIndicator");
        indicatorInstance.transform.SetParent(transform);
        indicatorInstance.transform.localPosition = indicatorOffset;

        // Ajouter un SpriteRenderer avec un sprite par défaut ou texte
        SpriteRenderer sr = indicatorInstance.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 100;

        if (defaultIndicatorSprite != null)
        {
            sr.sprite = defaultIndicatorSprite;
        }
        else
        {
            // Créer un simple carré comme indicateur par défaut
            Texture2D tex = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.yellow;
            tex.SetPixels(colors);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
            sr.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
    }

    void Update()
    {
        CheckForPlayers();

        if (isPlayerNear)
        {
            // Vérifier si le joueur appuie sur la touche de ramassage
            if (Input.GetKeyDown(pickupKey) || Input.GetButtonDown(arcadeButton))
            {
                PickUp();
            }
        }
    }

    void CheckForPlayers()
    {
        // Chercher tous les joueurs avec le tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        isPlayerNear = false;
        playerInRange = null;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= interactionRange)
            {
                isPlayerNear = true;
                playerInRange = player.transform;
                break;
            }
        }

        // Afficher/cacher l'indicateur
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(isPlayerNear);
        }
    }

    void PickUp()
    {
        // Ajouter à l'inventaire
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(itemName);
        }
        else
        {
            Debug.LogWarning("PlayerInventory non trouvé! Crée un GameObject avec le script PlayerInventory.");
        }

        Debug.Log($"Ramassé: {itemName}");

        // Détruire l'objet
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Afficher la zone d'interaction dans l'éditeur
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
