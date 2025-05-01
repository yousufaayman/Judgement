using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damagable))]
public class SlothEnemy : MonoBehaviour
{
    [Header("Basic Movement")]
    public float walkSpeed = 0.5f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    public float walkStopRate = 0.6f;

    // Time warp multipliers
    private float _timeWarpWalkMultiplier = 1f;
    private float _timeWarpAttackMultiplier = 1f;

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

    public bool HasTarget
    {
        get
        {
            return animator.GetBool(AnimationStrings.hasTarget);
        }
        private set
        {
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(AnimationStrings.lockVelocity);
        }
        set
        {
            animator.SetBool(AnimationStrings.lockVelocity, value);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damagable = GetComponent<Damagable>();
        attackComponent = GetComponentInChildren<Attack>();

        if (damagable != null)
            damagable.damageableHit.AddListener(OnHit);
    }

    private void OnHit(int damage, Vector2 knockback)
    {
        animator.SetTrigger(AnimationStrings.hitTrigger);
    }

    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime * _timeWarpAttackMultiplier;
        }
    }

    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded && (touchingDirections.IsOnWall || cliffDetectionZone.detectedColliders.Count == 0))
        {
            FlipDirection();
        }

        if (!LockVelocity)
        {
            if (CanMove && touchingDirections.IsGrounded)
            {
                rb.velocity = new Vector2(walkSpeed * walkDirectionVector.x * _timeWarpWalkMultiplier, rb.velocity.y);
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
            WalkDirection = WalkableDirection.Right;
        }
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }

    // Time warp methods
    public void ApplyTimeWarp(float walkMultiplier, float attackMultiplier)
    {
        _timeWarpWalkMultiplier = walkMultiplier;
        _timeWarpAttackMultiplier = attackMultiplier;
    }

    public void ResetTimeWarp()
    {
        _timeWarpWalkMultiplier = 1f;
        _timeWarpAttackMultiplier = 1f;
    }
}