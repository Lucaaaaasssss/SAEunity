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
        sr.sortingOrder = sortingOrder;

        // Collider
        if (addColliders)
        {
            wall.AddComponent<BoxCollider2D>();
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
        // Afficher le mur sélectionné à la position de la souris
        if (wallSprites != null && selectedWallIndex < wallSprites.Length && wallSprites[selectedWallIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 mousePos = GetMouseWorldPosition();
            if (snapToGrid)
            {
                mousePos = SnapToGrid(mousePos);
            }
            Gizmos.DrawWireCube(mousePos, Vector3.one * gridSize);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
        {
            Vector3 mousePos = Event.current != null ? Event.current.mousePosition : Vector2.zero;
            mousePos.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePos.y;
            Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePos);
            return ray.origin;
        }
        return Vector3.zero;
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

        // Snap le WallBuilder lui-même à la grille (comme les points de patrouille)
        SerializedProperty snapToGrid = serializedObject.FindProperty("snapToGrid");
        if (snapToGrid != null && snapToGrid.boolValue)
        {
            Vector3 currentPos = builder.transform.position;

            // Utiliser la méthode SnapToGrid via réflexion
            System.Reflection.MethodInfo snapMethod = builder.GetType().GetMethod("SnapToGrid",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (snapMethod != null)
            {
                Vector3 snappedPos = (Vector3)snapMethod.Invoke(builder, new object[] { currentPos });

                if (currentPos != snappedPos)
                {
                    Undo.RecordObject(builder.transform, "Snap WallBuilder to Grid");
                    builder.transform.position = snappedPos;
                }
            }
        }

        // Appuyer sur P pour placer
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
        {
            Vector3 mousePos = GetMouseWorldPos();
            builder.PlaceWall(mousePos);
            e.Use();
        }

        // Afficher où le mur sera placé
        Handles.color = Color.yellow;
        Vector3 pos = GetMouseWorldPos();
        if (builder.GetType().GetField("snapToGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(builder) is bool snap && snap)
        {
            float grid = (float)builder.GetType().GetField("gridSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(builder);
            pos.x = Mathf.Round(pos.x / grid) * grid;
            pos.y = Mathf.Round(pos.y / grid) * grid;
        }
        Handles.DrawWireCube(pos, Vector3.one * 0.5f);

        SceneView.RepaintAll();
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        return ray.origin;
    }
}
#endif
