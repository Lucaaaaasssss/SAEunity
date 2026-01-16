using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Ajoute automatiquement WaypointGridSnap à tous les waypoints au démarrage
/// </summary>
public class AutoAddWaypointSnap : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string waypointNamePattern = "Waypoint";

    void Awake()
    {
        AddSnapToAllWaypoints();
    }

    [ContextMenu("Add Snap to All Waypoints")]
    public void AddSnapToAllWaypoints()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int added = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains(waypointNamePattern.ToLower()))
            {
                if (obj.GetComponent<WaypointGridSnap>() == null)
                {
                    WaypointGridSnap snap = obj.AddComponent<WaypointGridSnap>();
                    snap.SnapToGrid();
                    added++;
                    Debug.Log($"✅ WaypointGridSnap ajouté à {obj.name}");

#if UNITY_EDITOR
                    EditorUtility.SetDirty(obj);
#endif
                }
            }
        }

        if (added > 0)
        {
            Debug.Log($"✅ {added} waypoint(s) ont reçu le script WaypointGridSnap !");
        }
        else
        {
            Debug.Log("ℹ️ Tous les waypoints ont déjà le script WaypointGridSnap");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AutoAddWaypointSnap))]
public class AutoAddWaypointSnapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutoAddWaypointSnap script = (AutoAddWaypointSnap)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("═══ OUTILS ═══", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✅ AJOUTER SNAP À TOUS LES WAYPOINTS", GUILayout.Height(50)))
        {
            script.AddSnapToAllWaypoints();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "💡 Clique le bouton pour ajouter WaypointGridSnap à tous les objets qui contiennent 'Waypoint' dans leur nom.",
            MessageType.Info);
    }
}
#endif
