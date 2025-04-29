using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int pointsPerKill = 100;
    [SerializeField] private int levelCompletionBonus = 1000;
    [SerializeField] private int bossKillBonus = 2000;
    [SerializeField] private float timeScoreFactor = 10f;

    // UI references removed as they're now handled by GameOverManager

    private int enemiesKilled = 0;
    private int totalScore = 0;
    private float levelStartTime;
    private bool levelActive = true;

    public static ScoreManager Instance { get; private set; }

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
        }
    }

    private void Start()
    {
        CharachterEvents.charachterDamaged += OnCharacterDamaged;
        ResetLevel();
    }

    private void OnDestroy()
    {
        CharachterEvents.charachterDamaged -= OnCharacterDamaged;
    }

    public void ResetLevel()
    {
        enemiesKilled = 0;
        levelStartTime = Time.time;
        levelActive = true;
    }

    public void EnemyKilled(GameObject enemy)
    {
        bool isBoss = enemy.GetComponent<WrathBoss>() != null;

        enemiesKilled++;
        totalScore += pointsPerKill;

        if (isBoss)
        {
            totalScore += bossKillBonus;
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

        float timeBonus = CalculateTimeScore();
        totalScore += Mathf.RoundToInt(timeBonus);
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
}