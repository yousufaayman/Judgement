using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damagable))]
public class SmallSkeleton : MonoBehaviour
{
    [Header("Basic Movement")]
    public float walkSpeed = 0.5f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    public float walkStopRate = 0.6f;

    [Header("Rage Properties")]
    [SerializeField] private float currentRage = 0f;
    [SerializeField] private float maxRage = 100f;
    [SerializeField] private float rageDecayRate = 5f; 
    [SerializeField] private float rageBuildOnDamage = 20f; 
    [SerializeField] private float rageBuildOnSight = 5f; 
    [SerializeField] private float baseWalkSpeed = 0.5f; 
    [SerializeField] private float maxRageWalkSpeed = 1.5f; 
    [SerializeField] private int baseAttackDamage = 10; 
    [SerializeField] private int maxRageAttackDamage = 25;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rageColor = Color.red;

    [SerializeField] private float rageContributionCooldown = 1.0f;
    private float rageContributionTimer = 0f;

    [SerializeField] private float rageContributionFactor = 0.1f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private float RagePercent => currentRage / maxRage;
    private float RageModifiedSpeed => Mathf.Lerp(baseWalkSpeed, maxRageWalkSpeed, RagePercent);
    private int RageModifiedDamage => Mathf.RoundToInt(Mathf.Lerp(baseAttackDamage, maxRageAttackDamage, RagePercent));

    private Animator animator;
    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Damagable damagable;
    private Attack attackComponent;

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right;

    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set
        {
            if (_walkDirection != value)
            {
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);
                if (value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }
                else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
            }
            _walkDirection = value;
        }
    }

    public bool _hasTarget = false;

    public bool HasTarget
    {
        get
        {
            return _hasTarget;
        }
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public float AttackCooldown
    {
        get
        {
            return animator.GetFloat(AnimationStrings.attackCooldown);
        }

        private set
        {
            animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
        }
    }

    public float GetCurrentRage()
    {
        return currentRage;
    }

    public float GetMaxRage()
    {
        return maxRage;
    }

    public void ApplyGlobalRageInfluence(float globalRageInfluence)
    {
        float rageToAdd = (0.5f + globalRageInfluence * 5f);

        float oldRage = currentRage;
        currentRage = Mathf.Min(maxRage, currentRage + rageToAdd);

        if (currentRage > oldRage + 2f && spriteRenderer != null)
        {
            StartCoroutine(FlashRageEffect());
        }
    }

    private IEnumerator FlashRageEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damagable = GetComponent<Damagable>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        attackComponent = GetComponentInChildren<Attack>();

        baseWalkSpeed = walkSpeed;

        if (attackComponent != null)
            baseAttackDamage = attackComponent.attackDamage;

        if (damagable != null)
            damagable.damageableHit.AddListener(OnDamageReceived);
    }

    private void Start()
    {
        if (GlobalRageManager.Instance != null)
        {
            GlobalRageManager.Instance.RegisterWrathEnemy(this);

            if (RagePercent > 0.2f)
            {
                GlobalRageManager.Instance.AddRageFromEnemy(currentRage * rageContributionFactor, maxRage);
            }
        }
    }

    private void OnDestroy()
    {
        if (GlobalRageManager.Instance != null)
        {
            GlobalRageManager.Instance.UnregisterWrathEnemy(this);
        }
    }

    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        UpdateRage();

        ApplyRageEffects();

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }

        rageContributionTimer -= Time.deltaTime;

        if (GlobalRageManager.Instance != null && RagePercent > 0.2f && HasTarget && rageContributionTimer <= 0)
        {
            GlobalRageManager.Instance.AddRageFromEnemy(currentRage * rageContributionFactor, maxRage);

            rageContributionTimer = rageContributionCooldown;
        }
    }

    private void UpdateRage()
    {
        float globalRageFactor = 1.0f;
        if (GlobalRageManager.Instance != null)
        {
            globalRageFactor = Mathf.Max(0.5f, GlobalRageManager.Instance.NormalizedRage);
        }

        float adjustedDecayRate = rageDecayRate * (2.0f - globalRageFactor);
        currentRage = Mathf.Max(0, currentRage - (adjustedDecayRate * Time.deltaTime));

        if (HasTarget)
        {
            float maxRageGainPerFrame = 2f * Time.deltaTime;
            float rageGain = Mathf.Min(rageBuildOnSight * Time.deltaTime, maxRageGainPerFrame);
            currentRage = Mathf.Min(maxRage, currentRage + rageGain);
        }
    }

    private void ApplyRageEffects()
    {
        walkSpeed = RageModifiedSpeed;

        if (attackComponent != null)
        {
            attackComponent.attackDamage = RageModifiedDamage;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(normalColor, rageColor, RagePercent);
        }
    }

    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall)
        {
            FlipDirection();
        }

        if (!damagable.LockVelocity)
        {
            if (CanMove)
            {
                rb.velocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, walkStopRate), rb.velocity.y);
            }
        }
    }

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            Debug.LogError("No Walkable Direction");
        }
    }

    public void OnDamageReceived(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);

        float rageGain = Mathf.Min(rageBuildOnDamage, maxRage * 0.25f);
        currentRage = Mathf.Min(maxRage, currentRage + rageGain);

        if (damagable.Health < damagable.MaxHealth * 0.3f)
        {
            float lowHealthRageBonus = Mathf.Min(rageBuildOnDamage * 0.5f, maxRage * 0.1f);
            currentRage = Mathf.Min(maxRage, currentRage + lowHealthRageBonus);
        }

        if (GlobalRageManager.Instance != null)
        {
            GlobalRageManager.Instance.AddRageFromEnemy(currentRage * rageContributionFactor * 2f, maxRage);
            rageContributionTimer = rageContributionCooldown * 0.5f;
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y);
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }
    
    public void SynchronizeWithGlobalRage(float normalizedGlobalRage)
    {
        float maxAllowedRage = maxRage * Mathf.Min(normalizedGlobalRage * 1.5f, 1.0f);

        if (currentRage > maxAllowedRage)
        {
            currentRage = Mathf.Max(currentRage - (rageDecayRate * 5f * Time.deltaTime), maxAllowedRage);
        }
    }

}