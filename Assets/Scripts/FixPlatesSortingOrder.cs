using UnityEngine;

/// <summary>
/// Met à jour le sorting order de toutes les plaques pour qu'elles soient sous la lumière
/// </summary>
public class FixPlatesSortingOrder : MonoBehaviour
{
    [ContextMenu("Fix All Plates Sorting Order")]
    void FixAllPlates()
    {
        // Trouver toutes les plaques
        PressurePlate[] plates = FindObjectsOfType<PressurePlate>();

        int updated = 0;
        foreach (PressurePlate plate in plates)
        {
            SpriteRenderer sr = plate.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = -20; // Sous la lumière (-10)
                updated++;
            }
        }

        Debug.Log($"✅ {updated} plaques ont été mises à jour avec sorting order = -20");
    }

    void Start()
    {
        FixAllPlates();
        Debug.Log("✅ Sorting order des plaques corrigé automatiquement !");
    }
}
