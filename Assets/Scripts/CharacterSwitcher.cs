using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private GameObject character1;
    [SerializeField] private GameObject character2;

    [Header("Switch Settings")]
    [SerializeField] private KeyCode switchKey = KeyCode.Tab;

    [Header("Visual Feedback")]
    [SerializeField] private bool highlightActiveCharacter = true;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Starting Positions")]
    [SerializeField] private bool setStartingPositions = false;
    [SerializeField] private Vector3 character1StartPosition = new Vector3(-2, 0, 0);
    [SerializeField] private Vector3 character2StartPosition = new Vector3(2, 0, 0);

    private GameObject currentCharacter;
    private PlayerMovement character1Movement;
    private PlayerMovement character2Movement;

    void Start()
    {
        // Récupérer les composants PlayerMovement
        if (character1 != null)
            character1Movement = character1.GetComponent<PlayerMovement>();

        if (character2 != null)
            character2Movement = character2.GetComponent<PlayerMovement>();

        // Vérifier que les deux personnages sont définis
        if (character1 == null || character2 == null)
        {
            Debug.LogError("CharacterSwitcher: Both characters must be assigned!");
            return;
        }

        // Définir les positions de départ si activé
        if (setStartingPositions)
        {
            if (character1 != null)
                character1.transform.position = character1StartPosition;

            if (character2 != null)
                character2.transform.position = character2StartPosition;
        }

        // Commencer avec le premier personnage actif
        currentCharacter = character1;
        UpdateCharacterStates();
    }

    void Update()
    {
        // Détecter l'appui sur la touche de switch (P1_B1 pour la borne d'arcade)
        if (Input.GetButtonDown("P1_B1") || Input.GetKeyDown(switchKey))
        {
            SwitchCharacter();
        }
    }

    void SwitchCharacter()
    {
        // Alterner entre les deux personnages
        if (currentCharacter == character1)
        {
            currentCharacter = character2;
        }
        else
        {
            currentCharacter = character1;
        }

        UpdateCharacterStates();
        Debug.Log($"Switched to: {currentCharacter.name}");
    }

    void UpdateCharacterStates()
    {
        if (character1 == null || character2 == null) return;

        // Activer/désactiver le contrôle des personnages
        bool char1Active = (currentCharacter == character1);
        bool char2Active = (currentCharacter == character2);

        if (character1Movement != null)
            character1Movement.SetControlEnabled(char1Active);

        if (character2Movement != null)
            character2Movement.SetControlEnabled(char2Active);

        // Appliquer le feedback visuel
        if (highlightActiveCharacter)
        {
            UpdateVisualFeedback(character1, char1Active);
            UpdateVisualFeedback(character2, char2Active);
        }
    }

    void UpdateVisualFeedback(GameObject character, bool isActive)
    {
        SpriteRenderer renderer = character.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = isActive ? activeColor : inactiveColor;
        }
    }

    // Méthode publique pour obtenir le personnage actif
    public GameObject GetActiveCharacter()
    {
        return currentCharacter;
    }

    // Méthode publique pour forcer le switch vers un personnage spécifique
    public void SwitchToCharacter(int characterIndex)
    {
        if (characterIndex == 1 && character1 != null)
        {
            currentCharacter = character1;
            UpdateCharacterStates();
        }
        else if (characterIndex == 2 && character2 != null)
        {
            currentCharacter = character2;
            UpdateCharacterStates();
        }
    }
}
