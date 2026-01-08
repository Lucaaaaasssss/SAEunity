using UnityEngine;
using System.Collections;
using Anatidae;

public static class PoliceSpeedManager
{
    public const float DefaultPoliceSpeed = 2f;

    public static IEnumerator FetchAndApplyPoliceSpeed()
    {
        // 1. Vérifier si ExtradataManager a déjà fetch les données
        if (!ExtradataManager.HasFetchedExtraData)
        {
            yield return ExtradataManager.FetchExtraData();
        }

        // 2. Récupérer la valeur de police_speed
        string speedStr = ExtradataManager.GetDataWithKey("police_speed");
        float speed = DefaultPoliceSpeed;

        if (speedStr != null && float.TryParse(speedStr, out float parsedSpeed))
        {
            speed = Mathf.Clamp(parsedSpeed, 0.1f, 20f);
            Debug.Log($"Police speed loaded from API: {speed}");
        }
        else
        {
            Debug.LogWarning($"Police speed not found, using default: {DefaultPoliceSpeed}");
        }

        // 3. Appliquer à toutes les instances de PolicePatrol
        PolicePatrol[] allPolice = Object.FindObjectsOfType<PolicePatrol>();
        foreach (var police in allPolice)
        {
            police.SetMoveSpeed(speed);
        }

        Debug.Log($"Applied speed {speed} to {allPolice.Length} police patrol(s)");
    }
}
