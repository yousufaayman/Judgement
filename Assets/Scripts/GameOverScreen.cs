using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject endGamePanel;
    public float displayDelay = 1.5f;

    public RectTransform titleTextRect;
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI scoreText;

    public Button restartButton;
    public Button exitButton;

    private Damagable playerDamagable;
    private bool endGameSequenceStarted = false;
    private bool isLevelCompleted = false;

    private int killsCount;
    private float timeTaken;

    void Start()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (titleText == null && titleTextRect != null)
            titleText = titleTextRect.GetComponent<TextMeshProUGUI>();

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            playerDamagable = player.GetComponent<Damagable>();

        if (playerDamagable == null)
            Debug.LogWarning("GameOverManager: Could not find PlayerController with Damagable in scene.");

        CharachterEvents.charachterDamaged += OnCharacterDamaged;

        SetupButtons();
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }
        else
        {
            Debug.LogWarning("GameOverManager: Restart button not assigned in Inspector.");
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ReturnToMainMenu);
        }
        else
        {
            Debug.LogWarning("GameOverManager: Exit button not assigned in Inspector.");
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;    
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;    
        SceneManager.LoadScene("MainMenu");

    }

    private void OnDestroy()
    {
        CharachterEvents.charachterDamaged -= OnCharacterDamaged;
    }

    void Update()
    {
        if (!endGameSequenceStarted && playerDamagable != null && !playerDamagable.IsAlive)
        {
            StartEndGameSequence(false);
        }
    }

    private void OnCharacterDamaged(GameObject character, int damage)
    {
        if (!endGameSequenceStarted && !character.CompareTag("Player"))
        {
            WrathBoss boss = character.GetComponent<WrathBoss>();
            Damagable bossDamagable = character.GetComponent<Damagable>();

            if (boss != null && bossDamagable != null && !bossDamagable.IsAlive)
            {
                StartEndGameSequence(true);

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.EnemyKilled(character);
                }
            }
        }
    }

    public void StartEndGameSequence(bool levelCompleted)
    {
        endGameSequenceStarted = true;
        isLevelCompleted = levelCompleted;
        StartCoroutine(ShowEndGameWithDelay());
    }

    private IEnumerator ShowEndGameWithDelay()
    {
        yield return new WaitForSecondsRealtime(displayDelay);

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);

            if (titleText != null)
            {
                titleText.text = isLevelCompleted ? "LEVEL COMPLETE" : "GAME OVER";
                titleText.color = isLevelCompleted ?
                    new Color(0.1f, 0.8f, 0.2f, 0) :     
                    new Color(0.8f, 0.1f, 0.1f, 0);       
            }

            UpdateScoreUI();

            if (titleTextRect != null)
            {
                titleTextRect.localScale = Vector3.one;
                StartCoroutine(BreathingEffect());
            }

        }

        StoreGameData();

        Time.timeScale = 0f;
    }

    private void StoreGameData()
    {
        if (ScoreManager.Instance != null)
        {
            killsCount = ScoreManager.Instance.GetKills();
            timeTaken = ScoreManager.Instance.GetTimeTaken();

        }
    }

    private void SaveToBackend()
    {
        Debug.Log("Saved to backend - Score: " + ScoreManager.Instance.GetScore() +
                  ", Kills: " + killsCount +
                  ", Time: " + FormatTime(timeTaken));
    }

    private void UpdateScoreUI()
    {
        if (ScoreManager.Instance != null)
        {
            if (scoreText != null)
                scoreText.text = "Score: " + ScoreManager.Instance.GetScore().ToString();
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private IEnumerator BreathingEffect()
    {
        float minScale = 0.985f;
        float maxScale = 1.015f;
        float breathingSpeed = 1.0f;
        float breatheDuration = 1f / breathingSpeed;
        float fadeTime = breatheDuration * 0.8f;

        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            float t = elapsedTime / fadeTime;

            if (titleText != null)
            {
                Color textColor = titleText.color;
                titleText.color = new Color(textColor.r, textColor.g, textColor.b, t);
            }

            float scale = Mathf.Lerp(minScale, maxScale, t);
            titleTextRect.localScale = new Vector3(scale, scale, 1f);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (titleText != null)
        {
            Color textColor = titleText.color;
            titleText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        }

        while (true)
        {
            elapsedTime = 0f;

            while (elapsedTime < breatheDuration)
            {
                float t = elapsedTime / breatheDuration;
                float scale = Mathf.Lerp(minScale, maxScale, t);
                titleTextRect.localScale = new Vector3(scale, scale, 1f);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            elapsedTime = 0f;

            while (elapsedTime < breatheDuration)
            {
                float t = elapsedTime / breatheDuration;
                float scale = Mathf.Lerp(maxScale, minScale, t);
                titleTextRect.localScale = new Vector3(scale, scale, 1f);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    public void SimulateEndGame(bool isLevelComplete)
    {
        if (!endGameSequenceStarted)
        {
            StartEndGameSequence(isLevelComplete);
        }
    }
}