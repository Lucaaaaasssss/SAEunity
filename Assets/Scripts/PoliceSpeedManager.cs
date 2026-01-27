using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public static class PoliceSpeedManager
{
    public const float DefaultPoliceSpeed = 2f;

    // Configuration API - même que le site web
    private const string LOCAL_PROXY_URL = "http://localhost:3000/proxy";
    private const string VPS_BASE_URL = "https://lucaslebecq.fr/api";

    [System.Serializable]
    private class ParametrageResponse
    {
        public Parametrage parametrage;
    }

    [System.Serializable]
    private class Parametrage
    {
        public float police_speed;
    }

    public static IEnumerator FetchAndApplyPoliceSpeed()
    {
        // 1. Construire l'URL pour récupérer les paramètres
        string apiUrl = VPS_BASE_URL + "/?parametre=parametrage";
        string requestUrl = LOCAL_PROXY_URL + "?url=" + UnityWebRequest.EscapeURL(apiUrl);

        Debug.Log("PoliceSpeedManager: Fetching police speed from API...");

        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        yield return request.SendWebRequest();

        float speed = DefaultPoliceSpeed;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string json = request.downloadHandler.text;
                ParametrageResponse response = JsonUtility.FromJson<ParametrageResponse>(json);

                if (response != null && response.parametrage != null)
                {
                    speed = Mathf.Clamp(response.parametrage.police_speed, 0.5f, 10f);
                    Debug.Log($"PoliceSpeedManager: Police speed loaded from API: {speed}");
                }
                else
                {
                    Debug.LogWarning($"PoliceSpeedManager: Invalid response, using default: {DefaultPoliceSpeed}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"PoliceSpeedManager: Error parsing response: {e.Message}, using default: {DefaultPoliceSpeed}");
            }
        }
        else
        {
            Debug.LogWarning($"PoliceSpeedManager: API error: {request.error}, using default: {DefaultPoliceSpeed}");
        }

        // 2. Appliquer à toutes les instances de PolicePatrol
        PolicePatrol[] allPolice = Object.FindObjectsOfType<PolicePatrol>();
        foreach (var police in allPolice)
        {
            police.SetMoveSpeed(speed);
        }

        Debug.Log($"PoliceSpeedManager: Applied speed {speed} to {allPolice.Length} police patrol(s)");
    }
}
