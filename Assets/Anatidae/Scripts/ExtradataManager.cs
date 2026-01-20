using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

namespace Anatidae {

    [Serializable]
    public class ExtraData
    {
        public List<ExtraDataElement> extradata;
    }

    [Serializable]
    public struct ExtraDataElement
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class ProxyPostRequest
    {
        public string url;
        public ExtraDataElement data;
    }

    public class ExtradataManager : MonoBehaviour
    {
        // CONFIGURATION : Changez ces URLs selon votre environnement
        // Pour utiliser le VPS : mettre USE_PROXY = true et renseigner VPS_BASE_URL
        // Pour utiliser en local direct : mettre USE_PROXY = false
        private const bool USE_PROXY = true;
        private const string LOCAL_PROXY_URL = "http://localhost:3000/proxy";
        private const string VPS_BASE_URL = "http://45.147.97.139"; // VPS configuré

        public static List<ExtraDataElement> ExtraData { get; private set; } = new List<ExtraDataElement>();
        public static bool HasFetchedExtraData { get; private set; }

        public static IEnumerator FetchExtraData()
        {
            string apiUrl = VPS_BASE_URL + "/api/extradata?game=" + HighscoreManager.GameName;
            string requestUrl;

            if (USE_PROXY)
            {
                // Utiliser le proxy pour contourner CORS
                requestUrl = LOCAL_PROXY_URL + "?url=" + UnityWebRequest.EscapeURL(apiUrl);
                Debug.Log("ExtradataManager: Using proxy to fetch from " + apiUrl);
            }
            else
            {
                // Appel direct (pour tests locaux)
                requestUrl = apiUrl;
                Debug.Log("ExtradataManager: Direct fetch from " + apiUrl);
            }

            UnityWebRequest request = UnityWebRequest.Get(requestUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("ExtradataManager: " + request.error);
            }
            else
            {
                string data = request.downloadHandler.text;
                try
                {
                    ExtraData extra = JsonUtility.FromJson<ExtraData>(data);
                    ExtraData = extra.extradata ?? new List<ExtraDataElement>();
                    HasFetchedExtraData = true;
                    Debug.Log("ExtradataManager: Extradata fetched successfully");
                }
                catch (Exception e)
                {
                    Debug.LogError("ExtradataManager: Error parsing extra data: " + e.Message);
                }
            }
        }

        public static IEnumerator SetExtraData(string key, string value)
        {
            ExtraDataElement extraDataElement = new ExtraDataElement { key = key, value = value };
            string apiUrl = VPS_BASE_URL + "/api/extradata?game=" + HighscoreManager.GameName;
            UnityWebRequest request;

            if (USE_PROXY)
            {
                // Utiliser le proxy POST
                ProxyPostRequest proxyRequest = new ProxyPostRequest
                {
                    url = apiUrl,
                    data = extraDataElement
                };

                request = new UnityWebRequest(LOCAL_PROXY_URL)
                {
                    method = UnityWebRequest.kHttpVerbPOST,
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(proxyRequest)))
                    {
                        contentType = "application/json"
                    },
                    downloadHandler = new DownloadHandlerBuffer()
                };
                Debug.Log("ExtradataManager: Using proxy to post to " + apiUrl);
            }
            else
            {
                // Appel direct
                request = new UnityWebRequest(apiUrl)
                {
                    method = UnityWebRequest.kHttpVerbPOST,
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(extraDataElement)))
                    {
                        contentType = "application/json"
                    },
                    downloadHandler = new DownloadHandlerBuffer()
                };
                Debug.Log("ExtradataManager: Direct post to " + apiUrl);
            }

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                Debug.LogError("ExtradataManager: " + request.error);
            else {
                int i = ExtraData.FindIndex(e => e.key == key);
                if (i != -1)
                    ExtraData[i] = extraDataElement;
                else
                    ExtraData.Add(extraDataElement);
                Debug.Log("ExtradataManager: Data set successfully");
            }
            // yield return FetchExtraData();
        }

        public static string GetDataWithKey(string key)
        {
            if (!HasFetchedExtraData)
            {
                Debug.LogWarning("Extradata not fetched yet. Please call FetchExtraData() first.");
                return null;
            }

            foreach (var element in ExtraData)
            {
                if (element.key == key)
                {
                    return element.value;
                }
            }

            // Debug.LogWarning($"No extra data found for key: {key}");
            return null;
        }
    }
}

