using UnityEngine;

/// <summary>
/// Système de deux plaques qui nécessite que les deux personnages soient chacun sur une plaque différente
/// Quand les deux personnages sont sur des plaques différentes, les portes s'ouvrent définitivement
/// </summary>
public class DualPressurePlateSystem : MonoBehaviour
{
    [Header("Pressure Plates")]
    [SerializeField] private PressurePlate plate1;
    [SerializeField] private PressurePlate plate2;

    [Header("Connected Doors")]
    [SerializeField] private Door[] connectedDoors; // Portes à ouvrir définitivement

    [Header("Sound")]
    [SerializeField] private AudioClip successSound; // Son à jouer quand les deux plaques sont activées
    private AudioSource audioSource;

    private bool puzzleSolved = false; // Le puzzle a-t-il été résolu ?

    void Start()
    {
        // Initialiser l'AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Si le puzzle est déjà résolu, ne rien faire
        if (puzzleSolved)
            return;

        // Vérifier si les deux plaques ont des joueurs différents dessus
        if (CheckBothPlatesActivated())
        {
            SolvePuzzle();
        }
    }

    /// <summary>
    /// Vérifie si les deux plaques ont chacune un joueur différent dessus
    /// </summary>
    bool CheckBothPlatesActivated()
    {
        // Vérifier que les deux plaques existent
        if (plate1 == null || plate2 == null)
            return false;

        // Récupérer les joueurs sur chaque plaque
        GameObject player1 = plate1.GetCurrentPlayer();
        GameObject player2 = plate2.GetCurrentPlayer();

        // Vérifier que les deux plaques ont un joueur ET que ce sont des joueurs différents
        if (player1 != null && player2 != null && player1 != player2)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Résout le puzzle et ouvre les portes définitivement
    /// </summary>
    void SolvePuzzle()
    {
        puzzleSolved = true;

        // Jouer le son de succès
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        // Ouvrir toutes les portes définitivement
        OpenDoorsPermantently();

        Debug.Log($"✅ Puzzle résolu ! Les deux personnages sont sur des plaques différentes. Portes ouvertes définitivement.");
    }

    /// <summary>
    /// Ouvre toutes les portes connectées de manière permanente
    /// </summary>
    void OpenDoorsPermantently()
    {
        foreach (Door door in connectedDoors)
        {
            if (door != null)
            {
                door.Open();
                Debug.Log($"🚪 Porte {door.gameObject.name} ouverte définitivement");
            }
        }
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmos()
    {
        // Dessiner des lignes entre les plaques et ce GameObject
        Gizmos.color = puzzleSolved ? Color.green : Color.magenta;

        if (plate1 != null)
        {
            Gizmos.DrawLine(transform.position, plate1.transform.position);
        }

        if (plate2 != null)
        {
            Gizmos.DrawLine(transform.position, plate2.transform.position);
        }

        // Dessiner des lignes vers les portes
        if (connectedDoors != null && connectedDoors.Length > 0)
        {
            Gizmos.color = puzzleSolved ? Color.green : Color.yellow;
            foreach (Door door in connectedDoors)
            {
                if (door != null)
                {
                    Gizmos.DrawLine(transform.position, door.transform.position);
                }
            }
        }
    }
}
