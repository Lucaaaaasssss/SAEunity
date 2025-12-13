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
    [SerializeField] private string waypointNamePattern = "Waypoint"; // Cherche les objets avec ce nom

    void Start()
    {
        AddSnapToAllWaypoints();
    }

    [ContextMenu("Add Snap to All Waypoints")]
    void AddSnapToAllWaypoints()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int added = 0;

        foreach (GameObject obj in allObjects)
        {
            // Si le nom contient "Waypoint" ou "waypoint"
            if (obj.name.ToLower().Contains(waypointNamePattern.ToLower()))
            {
                // Vérifier s'il n'a pas déjà le script
                if (obj.GetComponent<WaypointGridSnap>() == null)
                {
                    obj.AddComponent<WaypointGridSnap>();
                    added++;
                    Debug.Log($"✅ WaypointGridSnap ajouté à {obj.name}");
                }
            }
        }

        Debug.Log($"✅ {added} waypoint(s) ont reçu le script WaypointGridSnap !");
    }
}
