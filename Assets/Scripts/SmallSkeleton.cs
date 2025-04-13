using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damagable))]

public class SmallSkeleton : MonoBehaviour
{
    public float walkSpeed = 0.5f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;

    Animator animator;
    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Damagable damagable;

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector;

    public WalkableDirection WalkDirection
    { 
        get { return _walkDirection; } 
        set {
            if (_walkDirection != value) { 
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x *-1, gameObject.transform.localScale.y);
                if (value == WalkableDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                }else if (value == WalkableDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }
            }
            _walkDirection = value; 
        }
    }

    public bool _hasTarget = false;
    public float walkStopRate = 0.6f;

    public bool HasTarget { 
        get { 
            return _hasTarget; 
        } 
        private set {
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
        get {
            return animator.GetFloat(AnimationStrings.attackCooldown);
        }

        private set
        {
            animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
        }
            
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damagable = GetComponent<Damagable>();
    }

    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if(touchingDirections.IsGrounded && touchingDirections.IsOnWall) 
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
        else if (WalkDirection == WalkableDirection.Left){ 
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            Debug.LogError("No Walkable Direction");
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

}
