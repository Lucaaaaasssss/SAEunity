using UnityEngine;
using UnityEditor;

/// <summary>
/// S'exécute automatiquement dans l'éditeur pour ajouter WaypointGridSnap à tous les waypoints
/// Aucune intervention manuelle nécessaire
/// </summary>
[InitializeOnLoad]
public static class WaypointAutoSetup
{
    static WaypointAutoSetup()
    {
        // S'exécute au chargement de l'éditeur
        EditorApplication.hierarchyChanged += OnHierarchyChanged;

        // Exécuter une première fois
        SetupAllWaypoints();
    }

    static void OnHierarchyChanged()
    {
        // S'exécute quand la hiérarchie change (nouvel objet créé, etc.)
        SetupAllWaypoints();
    }

    static void SetupAllWaypoints()
    {
        // Trouver tous les waypoints
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Si le nom contient "Waypoint" ou "waypoint"
            if (obj.name.ToLower().Contains("waypoint"))
            {
                // Vérifier s'il n'a pas déjà le script
                if (obj.GetComponent<WaypointGridSnap>() == null)
                {
                    WaypointGridSnap snap = obj.AddComponent<WaypointGridSnap>();
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}
