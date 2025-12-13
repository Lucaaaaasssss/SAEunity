using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WallBuilder : MonoBehaviour
{
    [Header("Wall Sprites (mur1 à mur16)")]
    [SerializeField] private Sprite[] wallSprites = new Sprite[16];

    [Header("Paramètres")]
    [SerializeField] private float gridSize = 1f; // 32px avec PPU=32
    [SerializeField] private Transform groundReference; // Référence au sol pour alignement
    [SerializeField] private bool autoFindGround = true;
    [SerializeField] private int sortingOrder = 10; // Au-dessus du sol
    [SerializeField] private bool addColliders = true;

    [Header("Collider Settings")]
    [SerializeField] private Vector2 colliderSizeMultiplier = new Vector2(1.1f, 1.0f); // Largeur +10% pour gauche/droite

    [Header("Placement Rapide")]
    [SerializeField] private int selectedWallIndex = 0; // 0-15
    [SerializeField] private bool snapToGrid = true;

    private Transform wallsContainer;

    void Start()
    {
        // Créer le conteneur pour organiser les murs
        if (wallsContainer == null)
        {
            GameObject container = GameObject.Find("Walls");
            if (container == null)
            {
                container = new GameObject("Walls");
            }
            wallsContainer = container.transform;
        }

        // Trouver automatiquement le Ground
        if (autoFindGround && groundReference == null)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                groundReference = ground.transform;
                Debug.Log($"✅ WallBuilder aligné sur Ground à {ground.transform.position}");
            }
        }
    }

#if UNITY_EDITOR
    // Placer un mur à une position donnée
    public void PlaceWall(Vector3 position)
    {
        if (wallSprites == null || selectedWallIndex >= wallSprites.Length)
        {
            Debug.LogError("Sprite de mur invalide !");
            return;
        }

        Sprite sprite = wallSprites[selectedWallIndex];
        if (sprite == null)
        {
            Debug.LogError($"Mur {selectedWallIndex + 1} n'est pas assigné !");
            return;
        }

        // Snap à la grille
        if (snapToGrid)
        {
            position = SnapToGrid(position);
        }

        // Créer le GameObject mur
        GameObject wall = new GameObject($"Wall_{sprite.name}");
        wall.tag = "Wall"; // Tag pour la détection par les policiers

        // Parent
        Transform parent = GameObject.Find("Walls")?.transform;
        if (parent == null)
        {
            GameObject container = new GameObject("Walls");
            parent = container.transform;
            Undo.RegisterCreatedObjectUndo(container, "Create Walls Container");
        }
        wall.transform.SetParent(parent);
        wall.transform.position = position;

        // SpriteRenderer
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0; // Au-dessus de la lumière (-10) mais sous les personnages

        // Collider avec taille ajustée
        if (addColliders)
        {
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();

            // Calculer la taille du sprite
            Vector2 spriteSize = sprite.bounds.size;

            // Appliquer le multiplicateur pour agrandir sur les côtés
            collider.size = new Vector2(
                spriteSize.x * colliderSizeMultiplier.x,
                spriteSize.y * colliderSizeMultiplier.y
            );

            Debug.Log($"🔲 Collider: {collider.size} (sprite: {spriteSize})");
        }

        Undo.RegisterCreatedObjectUndo(wall, "Place Wall");
        Debug.Log($"✅ Mur {selectedWallIndex + 1} placé à {position}");
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 offset = Vector3.zero;

        // S'aligner sur le Ground si disponible
        if (groundReference != null)
        {
            offset = groundReference.position;
            // Ajuster pour le pivot central des sprites (correction de 0.5)
            offset.x -= gridSize * 0.5f;
            offset.y -= gridSize * 0.5f;
        }

        // Snap relatif au Ground
        float x = Mathf.Round((position.x - offset.x) / gridSize) * gridSize + offset.x;
        float y = Mathf.Round((position.y - offset.y) / gridSize) * gridSize + offset.y;
        return new Vector3(x, y, 0);
    }

    // Dessiner la grille alignée sur le Ground
    void OnDrawGizmos()
    {
        // Trouver le Ground automatiquement
        if (groundReference == null && autoFindGround)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                groundReference = ground.transform;
            }
        }

        Vector3 gridCenter = Vector3.zero;
        if (groundReference != null)
        {
            gridCenter = groundReference.position;
            // Ajuster pour le pivot central des sprites (correction de 0.5)
            gridCenter.x -= gridSize * 0.5f;
            gridCenter.y -= gridSize * 0.5f;
        }

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f); // Grille grise

        // Grille alignée sur le Ground
        for (int x = -15; x <= 15; x++)
        {
            for (int y = -15; y <= 15; y++)
            {
                Vector3 pos = gridCenter + new Vector3(x * gridSize, y * gridSize, 0);
                Gizmos.DrawWireCube(pos, Vector3.one * gridSize * 0.95f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Note: La prévisualisation est maintenant gérée dans OnSceneGUI du custom editor
        // pour éviter les problèmes de décalage
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(WallBuilder))]
public class WallBuilderEditor : Editor
{
    private const int PREVIEW_SIZE = 64;
    private const int PER_ROW = 4;
    private Vector2 scrollPos;

    public override void OnInspectorGUI()
    {
        WallBuilder builder = (WallBuilder)target;
        serializedObject.Update();

        DrawDefaultInspector();

        GUILayout.Space(15);
        EditorGUILayout.LabelField("═══ SÉLECTION DE MUR ═══", EditorStyles.boldLabel);

        SerializedProperty wallSprites = serializedObject.FindProperty("wallSprites");
        SerializedProperty selectedIndex = serializedObject.FindProperty("selectedWallIndex");

        // Vérifier 16 sprites
        if (wallSprites.arraySize != 16)
        {
            EditorGUILayout.HelpBox("Ajoute 16 sprites (mur1 à mur16)", MessageType.Warning);
            if (GUILayout.Button("Initialiser 16 emplacements"))
            {
                wallSprites.arraySize = 16;
                serializedObject.ApplyModifiedProperties();
            }
            return;
        }

        // Grille de sélection 4x4
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

        for (int row = 0; row < 4; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < PER_ROW; col++)
            {
                int index = row * PER_ROW + col;
                DrawWallButton(wallSprites, selectedIndex, index);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();

        // Mur sélectionné
        GUILayout.Space(10);
        EditorGUILayout.LabelField($"✅ Sélectionné : Mur {selectedIndex.intValue + 1}", EditorStyles.boldLabel);

        GUILayout.Space(10);

        // Position de placement
        EditorGUILayout.LabelField("Position de placement :", EditorStyles.boldLabel);
        Vector3 builderPos = builder.transform.position;
        EditorGUILayout.LabelField($"X: {builderPos.x:F2}, Y: {builderPos.y:F2}");

        // BOUTON DE PLACEMENT SIMPLE
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🧱 PLACER LE MUR ICI", GUILayout.Height(50)))
        {
            builder.PlaceWall(builder.transform.position);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("💡 Déplace le WallBuilder dans la Scene et clique 'PLACER LE MUR ICI' !", MessageType.Info);

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "🎨 RACCOURCI CLAVIER :\n" +
            "Appuie sur P dans la Scene pour placer à la souris",
            MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWallButton(SerializedProperty wallSprites, SerializedProperty selectedIndex, int index)
    {
        SerializedProperty sprite = wallSprites.GetArrayElementAtIndex(index);
        Sprite s = sprite.objectReferenceValue as Sprite;

        bool isSelected = selectedIndex.intValue == index;
        GUIStyle style = new GUIStyle(GUI.skin.button);

        if (isSelected)
        {
            style.normal.background = MakeTex(2, 2, new Color(0.2f, 0.8f, 0.2f, 0.8f));
        }

        GUILayout.BeginVertical(GUILayout.Width(PREVIEW_SIZE + 10));

        // Preview
        Rect rect = GUILayoutUtility.GetRect(PREVIEW_SIZE, PREVIEW_SIZE);
        if (s != null)
        {
            Texture2D preview = AssetPreview.GetAssetPreview(s);
            if (preview != null)
            {
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
            }
        }
        else
        {
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        }

        // Bouton
        if (GUILayout.Button($"Mur {index + 1}", style))
        {
            selectedIndex.intValue = index;
        }

        GUILayout.EndVertical();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    // Permettre le placement au clic dans la Scene
    private void OnSceneGUI()
    {
        WallBuilder builder = (WallBuilder)target;
        Event e = Event.current;

        // Récupérer les propriétés
        SerializedProperty snapToGrid = serializedObject.FindProperty("snapToGrid");
        SerializedProperty gridSize = serializedObject.FindProperty("gridSize");
        SerializedProperty wallSprites = serializedObject.FindProperty("wallSprites");
        SerializedProperty selectedIndex = serializedObject.FindProperty("selectedWallIndex");

        // Obtenir la position de la souris dans le monde (corrigée pour 2D)
        Vector3 mouseWorldPos = GetMouseWorldPos();

        // Appliquer le snap si nécessaire
        if (snapToGrid.boolValue)
        {
            System.Reflection.MethodInfo snapMethod = builder.GetType().GetMethod("SnapToGrid",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (snapMethod != null)
            {
                mouseWorldPos = (Vector3)snapMethod.Invoke(builder, new object[] { mouseWorldPos });
            }
        }

        // ===== GESTION DES CLICS =====
        // Clic gauche pour placer un mur
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            builder.PlaceWall(mouseWorldPos);
            e.Use(); // Empêche la désélection du WallBuilder
        }

        // Touche P pour placer (raccourci alternatif)
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
        {
            builder.PlaceWall(mouseWorldPos);
            e.Use();
        }

        // ===== PRÉVISUALISATION =====
        if (selectedIndex.intValue < wallSprites.arraySize)
        {
            // Carré jaune de prévisualisation
            Handles.color = new Color(1f, 1f, 0f, 0.8f); // Jaune vif
            Handles.DrawWireCube(mouseWorldPos, Vector3.one * gridSize.floatValue);

            // Remplissage semi-transparent
            Handles.color = new Color(1f, 1f, 0f, 0.2f);
            Handles.DrawSolidDisc(mouseWorldPos, Vector3.forward, gridSize.floatValue * 0.4f);

            // Croix au centre pour marquer le point précis
            Handles.color = Color.yellow;
            float crossSize = gridSize.floatValue * 0.3f;
            Handles.DrawLine(mouseWorldPos + Vector3.left * crossSize, mouseWorldPos + Vector3.right * crossSize);
            Handles.DrawLine(mouseWorldPos + Vector3.down * crossSize, mouseWorldPos + Vector3.up * crossSize);

            // Label avec le numéro du mur sélectionné
            Handles.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            Handles.Label(mouseWorldPos + Vector3.up * (gridSize.floatValue * 0.6f),
                         $"Mur {selectedIndex.intValue + 1}", style);
        }

        // Forcer le repaint pour que la prévisualisation suive la souris
        if (e.type == EventType.MouseMove)
        {
            SceneView.RepaintAll();
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private Vector3 GetMouseWorldPos()
    {
        // Obtenir la position 2D de la souris dans le monde
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.z = 0; // Forcer Z à 0 pour 2D
            return worldPos;
        }

        return Vector3.zero;
    }
}
#endif
