using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damagable))]
public class WrathEnemy : MonoBehaviour
{
    [Header("Basic Movement")]
    public float baseWalkSpeed = 0.5f;
    [SerializeField] private float walkSpeed;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    public float walkStopRate = 0.6f;

    [Header("Rage Properties")]
    [SerializeField] private float currentRage = 0f;
    [SerializeField] private float maxRage = 100f;
    [SerializeField] private float rageDecayRate = 5f; 
    [SerializeField] private float rageBuildOnDamage = 20f; 
    [SerializeField] private float rageBuildOnSight = 5f; 
    [SerializeField] private float rageBuildOnAllyDeath = 10f; 

    [Header("Rage Effects")]
    [SerializeField] private float maxRageWalkSpeed = 1.5f; 
    [SerializeField] private float baseAttackDamage = 10f;
    [SerializeField] private float maxRageAttackDamage = 25f;
    [SerializeField] private float baseAttackCooldownTime = 2f;
    [SerializeField] private float minRageAttackCooldownTime = 0.5f; 
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rageColor = new Color(1f, 0.3f, 0.3f, 1f); 

    [Header("Visual Effects")]
    [SerializeField] private GameObject rageParticles;
    [SerializeField] private float maxParticleEmission = 20f; 

    private Animator animator;
    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Damagable damagable;
    private SpriteRenderer spriteRenderer;
    private ParticleSystem particleSystem;
    private Attack attackComponent;

    public float RagePercent => currentRage / maxRage;
    public float RageModifiedSpeed => Mathf.Lerp(baseWalkSpeed, maxRageWalkSpeed, RagePercent);
    public float RageModifiedAttackDamage => Mathf.Lerp(baseAttackDamage, maxRageAttackDamage, RagePercent);
    public float RageModifiedAttackCooldown => Mathf.Lerp(baseAttackCooldownTime, minRageAttackCooldownTime, RagePercent);

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector;

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

    private bool _hasTarget = false;

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

    private static readonly string rageParameter = "ragePercent";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damagable = GetComponent<Damagable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        attackComponent = GetComponentInChildren<Attack>();

        if (rageParticles != null)
        {
            particleSystem = rageParticles.GetComponent<ParticleSystem>();
        }

        damagable.damageableHit.AddListener(OnDamageReceived);

    }

    private void Start()
    {
        walkSpeed = baseWalkSpeed;

        if (attackComponent != null)
        {
            attackComponent.attackDamage = Mathf.RoundToInt(baseAttackDamage);
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
    }

    private void UpdateRage()
    {
        currentRage = Mathf.Max(0, currentRage - (rageDecayRate * Time.deltaTime));

        if (HasTarget)
        {
            currentRage = Mathf.Min(maxRage, currentRage + (rageBuildOnSight * Time.deltaTime));
        }

        animator.SetFloat(rageParameter, RagePercent);
    }

    private void ApplyRageEffects()
    {
        walkSpeed = RageModifiedSpeed;

        if (attackComponent != null)
        {
            attackComponent.attackDamage = Mathf.RoundToInt(RageModifiedAttackDamage);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(normalColor, rageColor, RagePercent);
        }

        if (particleSystem != null)
        {
            var emission = particleSystem.emission;
            emission.rateOverTime = RagePercent * maxParticleEmission;

            if (RagePercent > 0.1f && !rageParticles.activeSelf)
            {
                rageParticles.SetActive(true);
            }
            else if (RagePercent <= 0.1f && rageParticles.activeSelf)
            {
                rageParticles.SetActive(false);
            }
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

        currentRage = Mathf.Min(maxRage, currentRage + rageBuildOnDamage);

        if (damagable.Health < damagable.MaxHealth * 0.25f)
        {
            currentRage = Mathf.Min(maxRage, currentRage + rageBuildOnDamage * 0.5f);
        }
    }

    public void OnAllyDeath(GameObject ally)
    {
        if (ally != gameObject)
        {
            float distance = Vector2.Distance(transform.position, ally.transform.position);
            if (distance < 5f) 
            {
                currentRage = Mathf.Min(maxRage, currentRage + rageBuildOnAllyDeath);
            }
        }
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }
}