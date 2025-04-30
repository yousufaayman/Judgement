using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSFXManager : MonoBehaviour
{
    public static MenuSFXManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop any playing sounds when changing scenes
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlaySuccessSound()
    {
        if (sfxSource != null && successSound != null)
        {
            sfxSource.Stop(); // Stop any currently playing sound
            sfxSource.PlayOneShot(successSound);
        }
    }

    public void PlayErrorSound()
    {
        if (sfxSource != null && errorSound != null)
        {
            sfxSource.Stop(); // Stop any currently playing sound
            sfxSource.PlayOneShot(errorSound);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
} 