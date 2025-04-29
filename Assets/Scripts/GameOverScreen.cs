using UnityEngine;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public GameObject endGamePanel;
    public float displayDelay = 1.5f;

    public RectTransform titleTextRect;
    public TextMeshProUGUI titleText;

    // Score UI elements
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI bonusText;

    private Damagable playerDamagable;
    private bool endGameSequenceStarted = false;
    private bool isLevelCompleted = false;

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

        // Subscribe to character events for tracking boss death
        CharachterEvents.charachterDamaged += OnCharacterDamaged;
    }

    private void OnDestroy()
    {
        CharachterEvents.charachterDamaged -= OnCharacterDamaged;
    }

    void Update()
    {
        // Check for player death
        if (!endGameSequenceStarted && playerDamagable != null && !playerDamagable.IsAlive)
        {
            StartEndGameSequence(false);
        }
    }

    private void OnCharacterDamaged(GameObject character, int damage)
    {
        // Check if this was a boss being killed
        if (!endGameSequenceStarted && !character.CompareTag("Player"))
        {
            WrathBoss boss = character.GetComponent<WrathBoss>();
            Damagable bossDamagable = character.GetComponent<Damagable>();

            if (boss != null && bossDamagable != null && !bossDamagable.IsAlive)
            {
                // Boss died, mark level as completed
                StartEndGameSequence(true);

                // Notify score manager that boss was killed
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

            // Set appropriate title text
            if (titleText != null)
            {
                titleText.text = isLevelCompleted ? "LEVEL COMPLETE" : "GAME OVER";
                titleText.color = isLevelCompleted ?
                    new Color(0.1f, 0.8f, 0.2f, 0) :  // Green for complete
                    new Color(0.8f, 0.1f, 0.1f, 0);   // Red for game over
            }

            // Update the score UI elements
            UpdateScoreUI();

            if (titleTextRect != null)
            {
                titleTextRect.localScale = Vector3.one;
                StartCoroutine(BreathingEffect());
            }

            // Show/hide bonus text based on level completion
            if (bonusText != null)
            {
                bonusText.gameObject.SetActive(isLevelCompleted);
            }
        }

        Time.timeScale = 0f;
    }

    private void UpdateScoreUI()
    {
        if (ScoreManager.Instance != null)
        {
            if (scoreText != null)
                scoreText.text = "Score: " + ScoreManager.Instance.GetScore().ToString();

            if (killsText != null)
                killsText.text = "Enemies Slain: " + ScoreManager.Instance.GetKills().ToString();

            if (timeText != null)
            {
                float timeTaken = ScoreManager.Instance.GetTimeTaken();
                timeText.text = "Time: " + FormatTime(timeTaken);
            }

            if (bonusText != null && isLevelCompleted)
                bonusText.text = "Boss Bonus: +2000";
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

    // For simulation/testing
    public void SimulateEndGame(bool isLevelComplete)
    {
        if (!endGameSequenceStarted)
        {
            StartEndGameSequence(isLevelComplete);
        }
    }
}