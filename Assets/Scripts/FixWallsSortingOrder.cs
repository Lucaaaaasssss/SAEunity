using UnityEngine;

/// <summary>
/// Met à jour le sorting order de tous les murs pour qu'ils soient devant la lumière
/// </summary>
public class FixWallsSortingOrder : MonoBehaviour
{
    [ContextMenu("Fix All Walls Sorting Order")]
    void FixAllWalls()
    {
        // Trouver tous les objets avec le tag "Wall"
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

        int updated = 0;
        foreach (GameObject wall in walls)
        {
            SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 0; // Au-dessus de la lumière
                updated++;
            }
        }

        Debug.Log($"✅ {updated} murs ont été mis à jour avec sorting order = 0");
    }

    void Start()
    {
        FixAllWalls();
        Debug.Log("✅ Sorting order des murs corrigé automatiquement !");
    }
}
