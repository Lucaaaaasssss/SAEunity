using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

namespace Anatidae {

    [Serializable]
    public class ProxyPostRequestHighscore
    {
        public string url;
        public HighscoreManager.HighscoreEntry data;
    }

    public class HighscoreManager : MonoBehaviour
    {
        // Changez cette variable par le nom de votre jeu
        // Ce nom sera le même que le nom du dossier contenant votre build, il ne doit donc pas contenir de caractères spéciaux ni d'espaces
        // Cette variable est utilisée pour stocker les highscores sur le serveur !
        public static string GameName = "Runaway_v1";
        // Changez cette variable pour définir quand est-ce qu'un score est considéré comme un highscore (top 10 par défaut)
        const int NumHighscores = 10;

        // ==================== CONFIGURATION ====================
        // Pour utiliser le VPS via le proxy (mode production WebGL):
        //   - USE_PROXY = true
        //   - VPS_BASE_URL = l'URL de votre API sur le VPS (ex: "http://45.147.97.139/api")
        //
        // Pour utiliser anatidae-arcade en local (mode test):
        //   - USE_PROXY = false
        //   - VPS_BASE_URL = "http://localhost:3000"
        //
        // Pour utiliser l'API PHP simple du TD:
        //   - USE_PROXY = true
        //   - VPS_BASE_URL = "http://VOTRE_VPS_IP/api"
        //   - USE_PHP_API = true
        // ===========================================================

        // ===== CONFIGURATION PRODUCTION =====
        // API PHP déployée sur le VPS

        private const bool USE_PROXY = true; // true pour WebGL sur la borne
        private const bool USE_PHP_API = true; // Utiliser l'API PHP
        private const string LOCAL_PROXY_URL = "http://localhost:3000/proxy"; // Proxy anatidae-arcade
        private const string VPS_BASE_URL = "https://lucaslebecq.fr/api"; // API PHP sur le VPS
        private const string PHP_SECURITY_KEY = "12345"; // Clé de sécurité pour l'écriture

        [Serializable]
        public struct HighscoreData
        {
            public List<HighscoreEntry> highscores;
        }

        [Serializable]
        public struct HighscoreEntry
        {
            public string name;
            public int score;
        }

        public static HighscoreManager Instance { get; private set; }
        public static List<HighscoreEntry> Highscores { get; private set;}
        public static bool HasFetchedHighscores { get; private set; }
        public static bool IsHighscoreInputScreenShown { get; private set; }
        public static string PlayerName;

        [SerializeField] HighscoreNameInput highscoreNameInput;
        [SerializeField] HighscoreUI highscoreUi;

        void Awake()
        {
            if (Instance == null){
                Instance = this;
            }
            else{
                Destroy(gameObject);
            }

            if (Instance.highscoreNameInput is null)
                Debug.LogError("HighscoreNameInput de HighscoreManager n'est pas défini.");
            else highscoreNameInput.gameObject.SetActive(false);

            if (Instance.highscoreUi is null)
                Debug.LogError("HighscoreUI de HighscoreManager n'est pas défini.");
            else highscoreUi.gameObject.SetActive(false);
        }

        public static void ShowHighscores()
        {
            if (Instance.highscoreUi is null){
                Debug.LogError("HighscoreUI de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreUi.gameObject.SetActive(true);
        }

        public static void HideHighscores()
        {
            if (Instance.highscoreUi is null){
                Debug.LogError("HighscoreUI de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreUi.gameObject.SetActive(false);
        }
        
        public static void ShowHighscoreInput(int highscore)
        {
            if (Instance.highscoreNameInput is null){
                Debug.LogError("HighscoreNameInput de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreNameInput.ShowHighscoreInput(highscore);
            IsHighscoreInputScreenShown = true;
        }

        public static void DisableHighscoreInput()
        {
            if (Instance.highscoreNameInput is null){
                Debug.LogError("HighscoreNameInput de HighscoreManager n'est pas défini.");
                return;
            }

            Instance.highscoreNameInput.gameObject.SetActive(false); 
            IsHighscoreInputScreenShown = false;
        }

        public static IEnumerator FetchHighscores()
        {
            Debug.Log("HighscoreManager: Fetching highscores...");

            string apiUrl;
            if (USE_PHP_API)
            {
                // API PHP du TD: ?parametre=lecture
                apiUrl = VPS_BASE_URL + "/?parametre=lecture";
            }
            else
            {
                // API Node.js/anatidae-arcade: /api/?game=GameName
                apiUrl = VPS_BASE_URL + "/api/?game=" + GameName;
            }

            string requestUrl;
            if (USE_PROXY)
            {
                // Utiliser le proxy pour contourner CORS
                requestUrl = LOCAL_PROXY_URL + "?url=" + UnityWebRequest.EscapeURL(apiUrl);
                Debug.Log("HighscoreManager: Using proxy to fetch from " + apiUrl);
            }
            else
            {
                // Appel direct (pour tests locaux)
                requestUrl = apiUrl;
                Debug.Log("HighscoreManager: Direct fetch from " + apiUrl);
            }

            UnityWebRequest request = UnityWebRequest.Get(requestUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("HighscoreManager: " + request.error);
            }
            else
            {
                string data = request.downloadHandler.text;
                try {
                    HighscoreData highscoreData = JsonUtility.FromJson<HighscoreData>(data);
                    Highscores = highscoreData.highscores;
                    HasFetchedHighscores = true;
                    Debug.Log("HighscoreManager: Highscores fetched successfully!");
                } catch (Exception e) {
                    Debug.LogError("HighscoreManager: " + e);
                }
            }
        }

        public static IEnumerator SetHighscore(string name, int score)
        {
            HighscoreEntry entry = new HighscoreEntry { name = name, score = score };
            Debug.Log("HighscoreManager: Setting highscore: " + JsonUtility.ToJson(entry));

            string apiUrl;
            if (USE_PHP_API)
            {
                // API PHP du TD: utiliser GET avec valeur dans l'URL (compatible avec le proxy GitHub)
                string valeur = "{\"name\":\"" + name + "\",\"score\":" + score + "}";
                apiUrl = VPS_BASE_URL + "/?parametre=ecriture&clefsecu=" + PHP_SECURITY_KEY + "&valeur=" + UnityWebRequest.EscapeURL(valeur);
            }
            else
            {
                // API Node.js/anatidae-arcade: /api/?game=GameName
                apiUrl = VPS_BASE_URL + "/api/?game=" + GameName;
            }

            UnityWebRequest request;

            if (USE_PROXY && USE_PHP_API)
            {
                // Utiliser le proxy GET pour l'API PHP (évite les problèmes de POST)
                string proxyUrl = LOCAL_PROXY_URL + "?url=" + UnityWebRequest.EscapeURL(apiUrl);
                request = UnityWebRequest.Get(proxyUrl);
                Debug.Log("HighscoreManager: Using proxy GET to " + apiUrl);
            }
            else if (USE_PROXY)
            {
                // Utiliser le proxy POST pour les autres APIs
                string proxyUrl = LOCAL_PROXY_URL + "?url=" + UnityWebRequest.EscapeURL(apiUrl);

                request = new UnityWebRequest(proxyUrl)
                {
                    method = UnityWebRequest.kHttpVerbPOST,
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(entry)))
                    {
                        contentType = "application/json"
                    },
                    downloadHandler = new DownloadHandlerBuffer()
                };
                Debug.Log("HighscoreManager: Using proxy to post to " + apiUrl);
            }
            else
            {
                // Appel direct
                request = new UnityWebRequest(apiUrl)
                {
                    method = UnityWebRequest.kHttpVerbPOST,
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(entry)))
                    {
                        contentType = "application/json"
                    },
                    downloadHandler = new DownloadHandlerBuffer()
                };
                Debug.Log("HighscoreManager: Direct post to " + apiUrl);
            }

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                Debug.LogError("HighscoreManager: " + request.error);
            else
                Debug.Log("HighscoreManager: Highscore set successfully!");

            yield return FetchHighscores();
        }

        public static bool IsHighscore(int score)
        {
            return IsHighscore(null, score);
        }
        
        public static bool IsHighscore(string name, int score)
        {
            if (!HasFetchedHighscores) {
                Debug.LogError("HighscoreManager: IsHighscore() appelé avant que les highscores ne soient récupérés. Appelez la coroutine FetchHighscores() avant d'utiliser IsHighscore().");
                return false;
            }

            if (name == null)
            {
                if (Highscores == null || Highscores.Count < NumHighscores || score > Highscores[NumHighscores - 1].score)
                    return true;
            } else {
                HighscoreEntry? entry = Highscores.Find(entry => entry.name == name);
                if(entry?.score < score)
                    return true;
            }
            return false;
        }
    }
}

