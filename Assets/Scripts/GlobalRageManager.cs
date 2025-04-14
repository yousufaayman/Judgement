using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobalRageManager : MonoBehaviour
{
    public static GlobalRageManager Instance { get; private set; }

    [System.Serializable]
    public class RageEvent : UnityEvent<float> { }
    public RageEvent onRageChanged = new RageEvent();

    [Header("Rage Settings")]
    [SerializeField] private float _globalRage = 0f;
    [SerializeField] private float maxRage = 100f;
    [SerializeField] private float passiveRageBuildupRate = 0.5f;          

    [Header("Rage Influence")]
    [SerializeField] private float individualToGlobalRageFactor = 0.2f;          
    [SerializeField] private float globalToIndividualRageFactor = 0.3f;        

    [SerializeField] private float maxRageContributionPerFrame = 5f;

    [SerializeField] private float diminishingReturnsFactor = 0.8f;
    private float rageContributionThisFrame = 0f;

    [Header("Rage Decay Settings")]
    [SerializeField] private float rageDecayRate = 3f;     
    [SerializeField] private float rageDecayDelay = 5f;       
    private float timeSinceLastRageIncrease = 0f;        

    public float RageDecayDelay
    {
        get { return rageDecayDelay; }
        set { rageDecayDelay = value; }
    }

    private List<SmallSkeleton> wrathEnemies = new List<SmallSkeleton>();

    public float GlobalRage
    {
        get { return _globalRage; }
        private set
        {
            float oldValue = _globalRage;
            _globalRage = Mathf.Clamp(value, 0, maxRage);

            if (oldValue != _globalRage)
            {
                onRageChanged.Invoke(_globalRage / maxRage);     
            }
        }
    }

    public float NormalizedRage => GlobalRage / maxRage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FindAllWrathEnemies();

        CharachterEvents.charachterDamaged += OnCharacterDamaged;
    }

    private void OnDestroy()
    {
        CharachterEvents.charachterDamaged -= OnCharacterDamaged;
    }

    private void Update()
    {
        bool rageIncreased = false;

        if (passiveRageBuildupRate > 0 && GlobalRage < maxRage * 0.2f)
        {
            GlobalRage += passiveRageBuildupRate * Time.deltaTime;
            rageIncreased = true;
        }

        if (rageIncreased)
        {
            timeSinceLastRageIncrease = 0f;
        }
        else
        {
            timeSinceLastRageIncrease += Time.deltaTime;
        }

        if (timeSinceLastRageIncrease > rageDecayDelay)
        {
            GlobalRage -= rageDecayRate * Time.deltaTime;
        }

        UpdateWrathEnemiesRage();

        rageContributionThisFrame = 0f;
    }

    private void OnCharacterDamaged(GameObject character, int damage)
    {
        float rageToAdd = 0;

        if (character.CompareTag("Player"))
        {
            rageToAdd = Mathf.Min(damage * 0.08f, maxRage * 0.1f);
        }
        else
        {
            rageToAdd = Mathf.Min(damage * 0.1f, maxRage * 0.05f);
        }

        GlobalRage += rageToAdd;
        timeSinceLastRageIncrease = 0f;
    }

    public void FindAllWrathEnemies()
    {
        wrathEnemies.Clear();
        SmallSkeleton[] enemies = FindObjectsOfType<SmallSkeleton>();

        foreach (SmallSkeleton enemy in enemies)
        {
            wrathEnemies.Add(enemy);
        }
    }

    public void RegisterWrathEnemy(SmallSkeleton enemy)
    {
        if (!wrathEnemies.Contains(enemy))
        {
            wrathEnemies.Add(enemy);
        }
    }

    public void UnregisterWrathEnemy(SmallSkeleton enemy)
    {
        if (wrathEnemies.Contains(enemy))
        {
            wrathEnemies.Remove(enemy);
        }
    }

    public void AddRageFromEnemy(float individualRage, float maxIndividualRage)
    {
        float normalizedIndividualRage = individualRage / maxIndividualRage;

        float effectiveFactor = individualToGlobalRageFactor * Mathf.Pow(diminishingReturnsFactor, rageContributionThisFrame);

        float rageToAdd = Mathf.Min(
            normalizedIndividualRage * effectiveFactor,
            maxRageContributionPerFrame * Time.deltaTime
        );

        if (rageToAdd > 0)
        {
            GlobalRage += rageToAdd;
            timeSinceLastRageIncrease = 0f;

            rageContributionThisFrame += rageToAdd;
        }
    }

    private void UpdateWrathEnemiesRage()
    {
        float maxInfluencePerEnemy = globalToIndividualRageFactor * 0.1f;

        foreach (SmallSkeleton enemy in wrathEnemies)
        {
            if (enemy != null)
            {
                float influence = Mathf.Min(
                    NormalizedRage * globalToIndividualRageFactor,
                    maxInfluencePerEnemy
                );

                enemy.ApplyGlobalRageInfluence(influence);
            }
        }
    }
}