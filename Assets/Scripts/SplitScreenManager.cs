using UnityEngine;

/// <summary>
/// G�re le split screen en mode 2 joueurs
/// Cr�e deux cam�ras: une pour chaque joueur
/// </summary>
public class SplitScreenManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private Transform player1; // Cobaye 1
    [SerializeField] private Transform player2; // Cobaye 2

    [Header("Cameras")]
    [SerializeField] private Camera mainCamera; // Cam�ra principale (pour le mode solo)
    private Camera camera1; // Cam�ra du joueur 1 (split screen gauche)
    private Camera camera2; // Cam�ra du joueur 2 (split screen droit)

    [Header("Camera Settings")]
    [SerializeField] private float cameraSize = 8f; // Taille orthographique des cam�ras (augment� pour bien voir)
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -10f); // Offset de la cam�ra
    [SerializeField] private bool followY = true; // Suivre les joueurs verticalement
    [SerializeField] private bool followX = false; // Suivre les joueurs horizontalement

    [Header("Visual Settings")]
    [SerializeField] private bool showBorders = true; // Afficher les cadres rouges
    [SerializeField] private int borderThickness = 5; // �paisseur des cadres

    private bool splitScreenActive = false;
    private Texture2D redTexture; // Texture pour les cadres rouges

    void Start()
    {
        // Auto-trouver la cam�ra principale si non assign�e
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Auto-trouver les joueurs si non assign�s
        if (player1 == null || player2 == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length >= 2)
            {
                player1 = players[0].transform;
                player2 = players[1].transform;
                Debug.Log($"Joueurs trouves automatiquement: {player1.name} et {player2.name}");
            }
            else
            {
                Debug.LogWarning("SplitScreenManager: Pas assez de joueurs trouves! Assigne-les manuellement.");
            }
        }

        // Attendre un frame pour que GameModeManager soit initialis�
        Invoke(nameof(InitializeCameras), 0.1f);
    }

    void InitializeCameras()
    {
        // Configurer le mode selon GameModeManager
        if (GameModeManager.Instance != null)
        {
            Debug.Log($"GameModeManager trouve! Mode actuel: {(GameModeManager.Instance.IsDuo() ? "DUO" : "SOLO")}");

            if (GameModeManager.Instance.IsDuo())
            {
                EnableSplitScreen();
            }
            else
            {
                Debug.Log("Mode SOLO detecte - pas de split screen");
            }
        }
        else
        {
            Debug.LogWarning("GameModeManager non trouve! Le split screen ne s'activera pas.");
        }
    }

    void LateUpdate()
    {
        // Mettre � jour les cam�ras en mode split screen (LateUpdate pour suivre apr�s le mouvement des joueurs)
        if (splitScreenActive)
        {
            UpdateCameraPositions();
        }
    }

    /// <summary>
    /// Active le split screen (2 cam�ras c�te � c�te)
    /// </summary>
    public void EnableSplitScreen()
    {
        if (splitScreenActive) return;

        Debug.Log("=== ACTIVATION DU SPLIT SCREEN ===");

        // V�rifier que les joueurs sont assign�s
        if (player1 == null || player2 == null)
        {
            Debug.LogError("ERREUR: Les joueurs ne sont pas assignes! Player1=" + (player1 != null ? player1.name : "NULL") + " Player2=" + (player2 != null ? player2.name : "NULL"));
            return;
        }

        Debug.Log($"Players trouves: {player1.name} et {player2.name}");

        // D�sactiver la cam�ra principale
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
            Debug.Log("Camera principale desactivee");
        }

        // Cr�er la texture rouge pour les cadres
        if (redTexture == null)
        {
            redTexture = new Texture2D(1, 1);
            redTexture.SetPixel(0, 0, Color.red);
            redTexture.Apply();
        }

        // Cr�er la cam�ra du joueur 1 (gauche)
        CreateCamera1();

        // Cr�er la cam�ra du joueur 2 (droite)
        CreateCamera2();

        splitScreenActive = true;
        Debug.Log("Split screen active!");
    }

    /// <summary>
    /// D�sactive le split screen et revient � la cam�ra principale
    /// </summary>
    public void DisableSplitScreen()
    {
        if (!splitScreenActive) return;

        Debug.Log("<� Split screen d�sactiv�");

        // R�activer la cam�ra principale
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        // D�truire les cam�ras de split screen
        if (camera1 != null)
        {
            Destroy(camera1.gameObject);
            camera1 = null;
        }

        if (camera2 != null)
        {
            Destroy(camera2.gameObject);
            camera2 = null;
        }

        splitScreenActive = false;
    }

    /// <summary>
    /// Cr�e la cam�ra du joueur 1 (c�t� gauche)
    /// </summary>
    void CreateCamera1()
    {
        GameObject cam1Object = new GameObject("Camera_Player1");
        camera1 = cam1Object.AddComponent<Camera>();
        camera1.orthographic = true;
        camera1.orthographicSize = cameraSize;

        // Vue gauche de l'�cran (50% de largeur)
        camera1.rect = new Rect(0f, 0f, 0.5f, 1f);

        // Copier les param�tres de la cam�ra principale
        if (mainCamera != null)
        {
            camera1.clearFlags = mainCamera.clearFlags;
            camera1.backgroundColor = mainCamera.backgroundColor;
            camera1.cullingMask = mainCamera.cullingMask;
        }

        // Positionner la cam�ra au d�marrage
        if (player1 != null)
        {
            Vector3 startPos = player1.position + cameraOffset;
            camera1.transform.position = startPos;
            Debug.Log($"Camera Player 1 creee (GAUCHE) - Position: {startPos} - Size: {cameraSize}");
        }
    }

    /// <summary>
    /// Cr�e la cam�ra du joueur 2 (c�t� droit)
    /// </summary>
    void CreateCamera2()
    {
        GameObject cam2Object = new GameObject("Camera_Player2");
        camera2 = cam2Object.AddComponent<Camera>();
        camera2.orthographic = true;
        camera2.orthographicSize = cameraSize;

        // Vue droite de l'�cran (50% de largeur)
        camera2.rect = new Rect(0.5f, 0f, 0.5f, 1f);

        // Copier les param�tres de la cam�ra principale
        if (mainCamera != null)
        {
            camera2.clearFlags = mainCamera.clearFlags;
            camera2.backgroundColor = mainCamera.backgroundColor;
            camera2.cullingMask = mainCamera.cullingMask;
        }

        // Positionner la cam�ra au d�marrage
        if (player2 != null)
        {
            Vector3 startPos = player2.position + cameraOffset;
            camera2.transform.position = startPos;
            Debug.Log($"Camera Player 2 creee (DROITE) - Position: {startPos} - Size: {cameraSize}");
        }
    }

    /// <summary>
    /// Met � jour les positions des cam�ras pour suivre les joueurs
    /// </summary>
    void UpdateCameraPositions()
    {
        // Cam�ra 1 suit le joueur 1
        if (camera1 != null && player1 != null)
        {
            Vector3 currentPos = camera1.transform.position;
            Vector3 targetPos = currentPos;

            if (followX)
                targetPos.x = player1.position.x + cameraOffset.x;

            if (followY)
                targetPos.y = player1.position.y + cameraOffset.y;

            targetPos.z = cameraOffset.z;
            camera1.transform.position = targetPos;
        }

        // Cam�ra 2 suit le joueur 2
        if (camera2 != null && player2 != null)
        {
            Vector3 currentPos = camera2.transform.position;
            Vector3 targetPos = currentPos;

            if (followX)
                targetPos.x = player2.position.x + cameraOffset.x;

            if (followY)
                targetPos.y = player2.position.y + cameraOffset.y;

            targetPos.z = cameraOffset.z;
            camera2.transform.position = targetPos;
        }
    }

    /// <summary>
    /// M�thode publique pour changer de mode en cours de jeu
    /// </summary>
    public void SetSplitScreenMode(bool enabled)
    {
        if (enabled)
        {
            EnableSplitScreen();
        }
        else
        {
            DisableSplitScreen();
        }
    }

    // Dessiner les cadres rouges autour des cam�ras
    void OnGUI()
    {
        if (!splitScreenActive || !showBorders || redTexture == null) return;

        // Cadre autour de la cam�ra gauche (50% de l'�cran)
        float leftWidth = Screen.width * 0.5f;
        float fullHeight = Screen.height;

        // Haut gauche
        GUI.DrawTexture(new Rect(0, 0, leftWidth, borderThickness), redTexture);
        // Bas gauche
        GUI.DrawTexture(new Rect(0, fullHeight - borderThickness, leftWidth, borderThickness), redTexture);
        // Gauche
        GUI.DrawTexture(new Rect(0, 0, borderThickness, fullHeight), redTexture);
        // S�paration centrale
        GUI.DrawTexture(new Rect(leftWidth - borderThickness / 2, 0, borderThickness, fullHeight), redTexture);

        // Cadre autour de la cam�ra droite (50% de l'�cran)
        float rightX = Screen.width * 0.5f;

        // Haut droit
        GUI.DrawTexture(new Rect(rightX, 0, leftWidth, borderThickness), redTexture);
        // Bas droit
        GUI.DrawTexture(new Rect(rightX, fullHeight - borderThickness, leftWidth, borderThickness), redTexture);
        // Droite
        GUI.DrawTexture(new Rect(Screen.width - borderThickness, 0, borderThickness, fullHeight), redTexture);
    }
}
