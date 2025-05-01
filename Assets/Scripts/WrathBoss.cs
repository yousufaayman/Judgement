using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Damagable), typeof(TouchingDirections))]
public class WrathBoss : MonoBehaviour
{
    [Header("Zones")]
    public DetectionZone playerDetectionZone;       
    public DetectionZone hitboxDetectionZone;      
    public DetectionZone cliffDetectionZone;      

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;              

    [Header("Movement")]
    public float moveSpeed = 3f;

    // Time warp multipliers
    private float _timeWarpMoveMultiplier = 1f;
    private float _timeWarpAttackMultiplier = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private TouchingDirections touchingDirections;

    public float AttackCooldown
    {
        get => animator.GetFloat(AnimationStrings.attackCooldown);
        private set => animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
    }

    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();

        if (cliffDetectionZone != null)
            cliffDetectionZone.noCollidersRemain.AddListener(FlipDirection);
    }

    void Update()
    {
        if (AttackCooldown > 0f)
            AttackCooldown -= Time.deltaTime;

        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall)
            FlipDirection();

        bool inChaseZone = IsPlayerIn(playerDetectionZone);
        bool inHitZone = IsPlayerIn(hitboxDetectionZone);

        animator.SetBool(AnimationStrings.hasTarget, inChaseZone || inHitZone);

        if (inHitZone && !isAttacking)
        {
            FacePlayer();
            HandleAttackStance();
        }
        else if (inChaseZone)
        {
            FacePlayer();
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    bool IsPlayerIn(DetectionZone zone)
    {
        foreach (var c in zone.detectedColliders)
            if (c.CompareTag("Player"))
                return true;
        return false;
    }

    void HandleAttackStance()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool(AnimationStrings.canMove, false);

        if (isAttacking) return;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetTrigger(AnimationStrings.attackTrigger);
        
        // Wait for the initial hit to land (assuming it's about 0.2 seconds into the animation)
        yield return new WaitForSeconds(0.2f * _timeWarpAttackMultiplier);
        
        // Set the cooldown after the initial hit
        AttackCooldown = attackCooldown * _timeWarpAttackMultiplier;
        
        // Wait for the remaining cooldown time
        yield return new WaitForSeconds((attackCooldown - 0.2f) * _timeWarpAttackMultiplier);
        isAttacking = false;
    }

    void ChasePlayer()
    {
        animator.SetBool(AnimationStrings.canMove, true);
        var col = playerDetectionZone.detectedColliders.Find(c => c.CompareTag("Player"));
        if (col == null) return;
        Vector2 dir = ((Vector2)col.transform.position - rb.position).normalized;
        rb.velocity = new Vector2(dir.x * moveSpeed * _timeWarpMoveMultiplier, rb.velocity.y);
    }

    void Patrol()
    {
        animator.SetBool(AnimationStrings.canMove, true);
        float dir = transform.localScale.x > 0 ? 1f : -1f;
        rb.velocity = new Vector2(dir * moveSpeed * _timeWarpMoveMultiplier, rb.velocity.y);
    }

    void FacePlayer()
    {
        Collider2D col = playerDetectionZone.detectedColliders.Find(c => c.CompareTag("Player"))
            ?? hitboxDetectionZone.detectedColliders.Find(c => c.CompareTag("Player"));
        if (col == null) return;
        bool toRight = col.transform.position.x > transform.position.x;
        Vector3 s = transform.localScale;
        if ((toRight && s.x < 0) || (!toRight && s.x > 0))
        {
            s.x *= -1;
            transform.localScale = s;
        }
    }

    void FlipDirection()
    {
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // Time warp methods
    public void ApplyTimeWarp(float moveMultiplier, float attackMultiplier)
    {
        _timeWarpMoveMultiplier = moveMultiplier;
        _timeWarpAttackMultiplier = attackMultiplier;
    }

    public void ResetTimeWarp()
    {
        _timeWarpMoveMultiplier = 1f;
        _timeWarpAttackMultiplier = 1f;
    }
}
