using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int pointsPerKill = 100;
    [SerializeField] private int levelCompletionBonus = 1000;
    [SerializeField] private int bossKillBonus = 2000;
    [SerializeField] private float timeScoreFactor = 10f;

    private int enemiesKilled = 0;
    private int totalScore = 0;
    private float levelStartTime;
    private bool levelActive = true;

    public static ScoreManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private string currentLevelId = "LEVEL1"; // Set this in the Unity Inspector for each level
    [SerializeField] private string nextLevelId = "LEVEL2"; // Set this in the Unity Inspector for each level

    private int currentScore = 0;
    private string currentSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == currentSceneName)
        {
            ResetLevel();
        }
        currentSceneName = scene.name;
    }

    private void OnDestroy()
    {
        CharachterEvents.charachterDamaged -= OnCharacterDamaged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CharachterEvents.charachterDamaged += OnCharacterDamaged;
        ResetLevel();
    }

    public void ResetLevel()
    {
        enemiesKilled = 0;
        totalScore = 0;
        currentScore = 0;
        levelStartTime = Time.time;
        levelActive = true;
    }

    public void EnemyKilled(GameObject enemy)
    {
        bool isBoss = enemy.GetComponent<WrathBoss>() != null;

        enemiesKilled++;
        totalScore += pointsPerKill;
        currentScore = totalScore;

        if (isBoss)
        {
            totalScore += bossKillBonus;
            currentScore = totalScore;
            CompleteLevel(true);
        }
    }

    public void PlayerDied()
    {
        if (!levelActive) return;
        levelActive = false;
    }

    public void CompleteLevel(bool killedBoss)
    {
        if (!levelActive) return;

        levelActive = false;
        totalScore += levelCompletionBonus;
        currentScore = totalScore;

        float timeBonus = CalculateTimeScore();
        totalScore += Mathf.RoundToInt(timeBonus);
        currentScore = totalScore;
    }

    private float CalculateTimeScore()
    {
        float timeTaken = Time.time - levelStartTime;
        return Mathf.Max(0, 1000 - (timeTaken * timeScoreFactor));
    }

    private void OnCharacterDamaged(GameObject character, int damage)
    {
        if (character.CompareTag("Player"))
        {
            Damagable playerDamagable = character.GetComponent<Damagable>();
            if (playerDamagable != null && !playerDamagable.IsAlive)
            {
                PlayerDied();
            }
        }

        if (!character.CompareTag("Player"))
        {
            Damagable enemyDamagable = character.GetComponent<Damagable>();
            if (enemyDamagable != null && !enemyDamagable.IsAlive)
            {
                EnemyKilled(character);
            }
        }
    }

    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public int GetScore()
    {
        return totalScore;
    }

    public int GetKills()
    {
        return enemiesKilled;
    }

    public float GetTimeTaken()
    {
        return Time.time - levelStartTime;
    }

    public int GetTimeBonus()
    {
        return Mathf.RoundToInt(CalculateTimeScore());
    }

    public int GetLevelCompletionBonus()
    {
        return levelCompletionBonus;
    }

    public int GetBossKillBonus()
    {
        return bossKillBonus;
    }

    public void AddScore(int points)
    {
        currentScore += points;
        totalScore += points;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public string GetCurrentLevelId()
    {
        return currentLevelId;
    }

    public string GetNextLevelId()
    {
        return nextLevelId;
    }
}