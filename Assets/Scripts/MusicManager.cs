using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gère la musique de fond qui persiste entre les scènes
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Settings")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton pattern - si une instance existe déjà, détruire celle-ci
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre les scènes

        // Créer l'AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;

        // Ajouter un AudioListener pour s'assurer qu'on entend toujours la musique
        if (FindObjectOfType<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }

        Debug.Log("MusicManager: Initialisé");

        // S'abonner à l'événement de changement de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Quand une nouvelle scène est chargée, vérifier s'il y a un AudioListener
        StartCoroutine(CheckAudioListener());
    }

    IEnumerator CheckAudioListener()
    {
        // Attendre une frame pour que la scène soit complètement chargée
        yield return null;

        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        AudioListener myListener = GetComponent<AudioListener>();

        if (listeners.Length == 0)
        {
            if (myListener == null)
            {
                gameObject.AddComponent<AudioListener>();
                Debug.Log("MusicManager: AudioListener ajouté");
            }
        }
        else if (listeners.Length > 1 && myListener != null)
        {
            // S'il y a plusieurs listeners, supprimer le nôtre
            Destroy(myListener);
        }
    }

    void Start()
    {
        StartCoroutine(PlayMusicLoop());
    }

    /// <summary>
    /// Coroutine qui s'assure que la musique joue en continu
    /// Utilise WaitForSecondsRealtime pour ignorer Time.timeScale
    /// </summary>
    IEnumerator PlayMusicLoop()
    {
        while (true)
        {
            if (audioSource != null && musicClip != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                    Debug.Log("MusicManager: Musique relancée");
                }
            }
            // Vérifie toutes les 0.5 secondes en temps réel (ignore Time.timeScale)
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    void OnEnable()
    {
        // Relancer la coroutine si l'objet est réactivé
        StopAllCoroutines();
        StartCoroutine(PlayMusicLoop());
    }

    /// <summary>
    /// Change le volume de la musique
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Met la musique en pause
    /// </summary>
    public void Pause()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// Reprend la musique
    /// </summary>
    public void Resume()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }
}
