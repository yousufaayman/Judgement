using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuAudio : MonoBehaviour
{
    public static MenuAudio Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip menuTheme;
    [SerializeField] private List<string> gameScenes = new List<string> { "Level1", "Level2", "Level3" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (audioSource != null && menuTheme != null)
        {
            audioSource.clip = menuTheme;
            audioSource.loop = true;
            audioSource.Play();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (gameScenes.Contains(scene.name))
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
} 