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
    public float hitboxDuration = 0.5f;           

    [Header("Movement")]
    public float moveSpeed = 3f;

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

        if (isAttacking)
            return;

        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall)
            FlipDirection();

        bool inChaseZone = IsPlayerIn(playerDetectionZone);
        bool inHitZone = IsPlayerIn(hitboxDetectionZone);

        animator.SetBool(AnimationStrings.hasTarget, inChaseZone || inHitZone);

        if (inHitZone)
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

        AttackCooldown = attackCooldown;

        animator.SetTrigger(AnimationStrings.attackTrigger);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    void ChasePlayer()
    {
        animator.SetBool(AnimationStrings.canMove, true);
        var col = playerDetectionZone.detectedColliders.Find(c => c.CompareTag("Player"));
        if (col == null) return;
        Vector2 dir = ((Vector2)col.transform.position - rb.position).normalized;
        rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);
    }

    void Patrol()
    {
        animator.SetBool(AnimationStrings.canMove, true);
        float dir = transform.localScale.x > 0 ? 1f : -1f;
        rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
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
}
