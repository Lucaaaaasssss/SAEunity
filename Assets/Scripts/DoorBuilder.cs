using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Builder pour créer des portes facilement dans l'éditeur
/// </summary>
public class DoorBuilder : MonoBehaviour
{
    [Header("Door Sprites")]
    [SerializeField] private Sprite porte_f_g_h; // Fermée Gauche Haut
    [SerializeField] private Sprite porte_f_g_b; // Fermée Gauche Bas
    [SerializeField] private Sprite porte_f_d_h; // Fermée Droite Haut
    [SerializeField] private Sprite porte_f_d_b; // Fermée Droite Bas
    [SerializeField] private Sprite porte_o_g_h; // Ouverte Gauche Haut
    [SerializeField] private Sprite porte_o_g_b; // Ouverte Gauche Bas
    [SerializeField] private Sprite porte_o_d_h; // Ouverte Droite Haut
    [SerializeField] private Sprite porte_o_d_b; // Ouverte Droite Bas

    [Header("Pressure Plate Sprites")]
    [SerializeField] private Sprite plaque_lev; // Levée
    [SerializeField] private Sprite plaque_ap;  // Appuyée

    [Header("Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private int sortingOrder = 0;
    [SerializeField] private Transform groundReference; // Référence au sol pour alignement
    [SerializeField] private bool autoFindGround = true;

    public enum DoorType
    {
        GaucheHaut,
        GaucheBas,
        DroiteHaut,
        DroiteBas
    }

    [Header("Door Placement")]
    [SerializeField] private DoorType selectedDoorType = DoorType.GaucheHaut;

    private Transform doorsContainer;
    private Transform platesContainer;

    void Start()
    {
        // Trouver automatiquement le Ground
        if (autoFindGround && groundReference == null)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                groundReference = ground.transform;
                Debug.Log($"✅ DoorBuilder aligné sur Ground à {ground.transform.position}");
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Place une porte à la position actuelle
    /// </summary>
    public void PlaceDoor(Vector3 position)
    {
        if (snapToGrid)
        {
            position = SnapToGrid(position);
        }

        // Créer le GameObject porte
        GameObject doorObj = new GameObject($"Door_{selectedDoorType}");
        doorObj.transform.position = position;

        // Parent
        Transform parent = GetOrCreateDoorsContainer();
        doorObj.transform.SetParent(parent);

        // SpriteRenderer
        SpriteRenderer sr = doorObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;

        // Door script
        Door door = doorObj.AddComponent<Door>();

        // Assigner les sprites selon le type
        Sprite closedSprite = null;
        Sprite openSprite = null;

        switch (selectedDoorType)
        {
            case DoorType.GaucheHaut:
                closedSprite = porte_f_g_h;
                openSprite = porte_o_g_h;
                break;
            case DoorType.GaucheBas:
                closedSprite = porte_f_g_b;
                openSprite = porte_o_g_b;
                break;
            case DoorType.DroiteHaut:
                closedSprite = porte_f_d_h;
                openSprite = porte_o_d_h;
                break;
            case DoorType.DroiteBas:
                closedSprite = porte_f_d_b;
                openSprite = porte_o_d_b;
                break;
        }

        // Utiliser reflection pour set les private fields du Door
        var closedField = typeof(Door).GetField("closedSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var openField = typeof(Door).GetField("openSprite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var spriteRendererField = typeof(Door).GetField("spriteRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (closedField != null) closedField.SetValue(door, closedSprite);
        if (openField != null) openField.SetValue(door, openSprite);
        if (spriteRendererField != null) spriteRendererField.SetValue(door, sr);

        sr.sprite = closedSprite;

        // BoxCollider2D (actif par défaut pour porte fermée)
        BoxCollider2D collider = doorObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;

        Undo.RegisterCreatedObjectUndo(doorObj, "Place Door");
        Debug.Log($"✅ Porte {selectedDoorType} placée à {position}");
    }

    /// <summary>
    /// Place une plaque de pression à la position actuelle
    /// </summary>
    public void PlacePressurePlate(Vector3 position)
    {
        if (snapToGrid)
        {
            position = SnapToGrid(position);
        }

        // Créer le GameObject plaque
        GameObject plateObj = new GameObject("PressurePlate");
        plateObj.transform.position = position;

        // Parent
        Transform parent = GetOrCreatePlatesContainer();
        plateObj.transform.SetParent(parent);

        // SpriteRenderer
        SpriteRenderer sr = plateObj.AddComponent<SpriteRenderer>();
        sr.sprite = plaque_lev;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -20; // Sous la lumière des gardes (-10) et tout le reste

        // BoxCollider2D (trigger)
        BoxCollider2D collider = plateObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f); // Taille de la plaque

        // PressurePlate script
        PressurePlate plate = plateObj.AddComponent<PressurePlate>();

        // Utiliser reflection pour set les private fields
        var plateUpField = typeof(PressurePlate).GetField("plateUp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var plateDownField = typeof(PressurePlate).GetField("plateDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var spriteRendererField = typeof(PressurePlate).GetField("spriteRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (plateUpField != null) plateUpField.SetValue(plate, plaque_lev);
        if (plateDownField != null) plateDownField.SetValue(plate, plaque_ap);
        if (spriteRendererField != null) spriteRendererField.SetValue(plate, sr);

        Undo.RegisterCreatedObjectUndo(plateObj, "Place Pressure Plate");
        Debug.Log($"✅ Plaque de pression placée à {position}");
    }

    Transform GetOrCreateDoorsContainer()
    {
        if (doorsContainer != null) return doorsContainer;

        GameObject container = GameObject.Find("Doors");
        if (container == null)
        {
            container = new GameObject("Doors");
        }
        doorsContainer = container.transform;
        return doorsContainer;
    }

    Transform GetOrCreatePlatesContainer()
    {
        if (platesContainer != null) return platesContainer;

        GameObject container = GameObject.Find("PressurePlates");
        if (container == null)
        {
            container = new GameObject("PressurePlates");
        }
        platesContainer = container.transform;
        return platesContainer;
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        // Snap au centre des carreaux (décalage de 0.5)
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize + (gridSize * 0.5f);
        float snappedY = Mathf.Round(position.y / gridSize) * gridSize + (gridSize * 0.5f);
        return new Vector3(snappedX, snappedY, 0);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(DoorBuilder))]
public class DoorBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DoorBuilder builder = (DoorBuilder)target;

        GUILayout.Space(15);
        EditorGUILayout.LabelField("═══ PLACEMENT ═══", EditorStyles.boldLabel);

        // Placement de porte
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🚪 PLACER PORTE ICI", GUILayout.Height(50)))
        {
            builder.PlaceDoor(builder.transform.position);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        // Placement de plaque
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("⬜ PLACER PLAQUE ICI", GUILayout.Height(50)))
        {
            builder.PlacePressurePlate(builder.transform.position);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "💡 UTILISATION :\n" +
            "1. Assignez tous les sprites de portes et plaques\n" +
            "2. Choisissez le type de porte (Gauche/Droite, Haut/Bas)\n" +
            "3. Déplacez le DoorBuilder dans la scène\n" +
            "4. Cliquez 'PLACER PORTE ICI' ou 'PLACER PLAQUE ICI'\n" +
            "5. Pour connecter une plaque à des portes :\n" +
            "   - Sélectionnez la plaque\n" +
            "   - Glissez les portes dans 'Connected Doors'",
            MessageType.Info);
    }

    void OnSceneGUI()
    {
        DoorBuilder builder = (DoorBuilder)target;
        Event e = Event.current;

        // Récupérer les propriétés
        SerializedProperty snapToGrid = serializedObject.FindProperty("snapToGrid");
        SerializedProperty gridSize = serializedObject.FindProperty("gridSize");

        // Snap le DoorBuilder à la grille
        if (snapToGrid.boolValue)
        {
            Vector3 currentPos = builder.transform.position;
            float grid = gridSize.floatValue;
            Vector3 snappedPos = new Vector3(
                Mathf.Round(currentPos.x / grid) * grid,
                Mathf.Round(currentPos.y / grid) * grid,
                0
            );

            if (currentPos != snappedPos)
            {
                Undo.RecordObject(builder.transform, "Snap DoorBuilder to Grid");
                builder.transform.position = snappedPos;
            }
        }

        // Raccourcis clavier
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.D)
        {
            builder.PlaceDoor(builder.transform.position);
            e.Use();
        }

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
        {
            builder.PlacePressurePlate(builder.transform.position);
            e.Use();
        }

        SceneView.RepaintAll();
    }
}
#endif
