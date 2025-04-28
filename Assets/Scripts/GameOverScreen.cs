using UnityEngine;
using TMPro;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverPanel;
    public float deathDelay = 1.5f;
    public RectTransform gameOverTextRect;
    public TextMeshProUGUI gameOverText;

    private Damagable playerDamagable;
    private bool gameOverSequenceStarted = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverText == null && gameOverTextRect != null)
            gameOverText = gameOverTextRect.GetComponent<TextMeshProUGUI>();

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            playerDamagable = player.GetComponent<Damagable>();

        if (playerDamagable == null)
            Debug.LogWarning("GameOverScreen: Could not find PlayerController with Damagable in scene.");
    }

    void Update()
    {
        if (!gameOverSequenceStarted && playerDamagable != null && !playerDamagable.IsAlive)
        {
            StartGameOverSequence();
        }
    }

    private void StartGameOverSequence()
    {
        gameOverSequenceStarted = true;
        StartCoroutine(ShowGameOverWithDelay());
    }

    private IEnumerator ShowGameOverWithDelay()
    {
        yield return new WaitForSecondsRealtime(deathDelay);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverText != null && gameOverTextRect != null)
            {
                gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0);
                gameOverTextRect.localScale = Vector3.one;
                StartCoroutine(BreathingEffect());
            }
        }

        Time.timeScale = 0f;
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

            if (gameOverText != null)
            {
                Color textColor = gameOverText.color;
                gameOverText.color = new Color(textColor.r, textColor.g, textColor.b, t);
            }

            float scale = Mathf.Lerp(minScale, maxScale, t);
            gameOverTextRect.localScale = new Vector3(scale, scale, 1f);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (gameOverText != null)
        {
            Color textColor = gameOverText.color;
            gameOverText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        }

        while (true)
        {
            elapsedTime = 0f;

            while (elapsedTime < breatheDuration)
            {
                float t = elapsedTime / breatheDuration;
                float scale = Mathf.Lerp(minScale, maxScale, t);
                gameOverTextRect.localScale = new Vector3(scale, scale, 1f);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            elapsedTime = 0f;

            while (elapsedTime < breatheDuration)
            {
                float t = elapsedTime / breatheDuration;
                float scale = Mathf.Lerp(maxScale, minScale, t);
                gameOverTextRect.localScale = new Vector3(scale, scale, 1f);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}